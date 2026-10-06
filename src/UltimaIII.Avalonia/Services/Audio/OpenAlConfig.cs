using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Sets OpenAL Soft's mixing period. Its context attributes don't reach the PipeWire backend, but
/// its configuration does: OpenAL Soft reads the file named by <c>ALSOFT_CONF</c> after the
/// system's and the user's own <c>alsoft.conf</c>, so a file holding just the period overrides
/// that and nothing else. A player's own <c>ALSOFT_CONF</c> is left alone.
/// </summary>
public static class OpenAlConfig
{
    public const string Variable = "ALSOFT_CONF";

    /// <summary>Overrides the automatic choice: small, medium or large.</summary>
    public const string BufferVariable = "AVAULTIMA3_AUDIO_BUFFER";

    /// <summary>
    /// The period to use (0 = OpenAL's default): small is OpenAL's own default, medium 1024 frames
    /// (about 21 ms), large 2048 (about 43 ms). Automatic picks large in a virtual machine, whose
    /// emulated sound card underruns constantly at the default size.
    /// </summary>
    public static int PeriodFrames() =>
        Environment.GetEnvironmentVariable(BufferVariable)?.Trim().ToLowerInvariant() switch
        {
            "small" => 0,
            "medium" => 1024,
            "large" => 2048,
            _ => VirtualMachine.Detect() ? 2048 : 0,
        };

    public static void UsePeriod(int periodFrames)
    {
        if (periodFrames <= 0) return;
        try
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Variable))) return;
            var path = Path.Combine(Path.GetTempPath(), $"avaultima3-openal-{periodFrames}.conf");
            File.WriteAllText(path, $"# Written by AVAUltima3: OpenAL Soft's mixing period.\n[general]\nperiod_size = {periodFrames}\nperiods = 3\n");
            SetNativeEnvironment(Variable, path);
            Console.WriteLine($"Audio: OpenAL period set to {periodFrames} frames");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DllNotFoundException or EntryPointNotFoundException)
        {
            Console.WriteLine($"Audio: Could not set the audio period - {ex.Message}");
        }
    }

    /// <summary>
    /// Sets a variable native code can see: .NET keeps its own copy of the environment on Unix,
    /// which OpenAL Soft's getenv never reads, so this goes through libc's setenv there.
    /// </summary>
    private static unsafe void SetNativeEnvironment(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
        if (OperatingSystem.IsWindows()) return;
        string[] libraries = OperatingSystem.IsMacOS() ? ["/usr/lib/libSystem.B.dylib"] : ["libc.so.6", "libc"];
        foreach (var library in libraries)
        {
            if (!NativeLibrary.TryLoad(library, out var handle)) continue;
            if (!NativeLibrary.TryGetExport(handle, "setenv", out var setenv)) continue;
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(name + '\0');
            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value + '\0');
            fixed (byte* n = nameBytes, v = valueBytes)
                ((delegate* unmanaged<byte*, byte*, int, int>)setenv)(n, v, 1);
            return;
        }
    }
}

/// <summary>
/// Whether we are running in a virtual machine (as systemd-detect-virt would tell): the firmware's
/// vendor names on PCs, QEMU's machine and firmware devices on ARM, a hypervisor node or CPU flag.
/// </summary>
public static class VirtualMachine
{
    private static readonly string[] VendorMarks =
        ["QEMU", "KVM", "VMware", "VirtualBox", "innotek", "Parallels", "Xen", "Bochs", "Apple Virtualization", "Virtual Machine", "BHYVE", "UTM"];

    public static bool Detect()
    {
        try
        {
            if (!OperatingSystem.IsLinux()) return false;
            foreach (var f in new[] { "/sys/class/dmi/id/sys_vendor", "/sys/class/dmi/id/product_name", "/sys/class/dmi/id/board_vendor" })
                if (Read(f) is { } text && VendorMarks.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase))) return true;
            if (Read("/proc/device-tree/compatible") is { } machine && machine.Contains("dummy-virt", StringComparison.Ordinal)) return true;
            if (Directory.Exists("/proc/device-tree/hypervisor") || File.Exists("/sys/hypervisor/type")) return true;
            if (Directory.Exists("/sys/firmware/qemu_fw_cfg")
                || Directory.Exists("/sys/bus/platform/devices") && Directory.EnumerateFileSystemEntries("/sys/bus/platform/devices").Any(d => d.Contains("fw-cfg", StringComparison.Ordinal) || d.Contains("fw_cfg", StringComparison.Ordinal)))
                return true;
            return Read("/proc/cpuinfo") is { } cpu && cpu.Contains(" hypervisor", StringComparison.Ordinal);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static string? Read(string path) => File.Exists(path) ? File.ReadAllText(path) : null;
}
