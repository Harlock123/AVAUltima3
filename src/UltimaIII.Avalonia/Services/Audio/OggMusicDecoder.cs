using System;
using System.IO;
using NVorbis;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Decodes OGG Vorbis files into mono float[] PCM at 44100 Hz
/// for use with the existing AudioService streaming pipeline.
/// </summary>
public static class OggMusicDecoder
{
    private const int TargetSampleRate = ChiptuneGenerator.SampleRate; // 44100

    /// <summary>
    /// Attempts to load and decode an OGG file into a mono float[] at 44100 Hz.
    /// Returns null if the file doesn't exist or decoding fails.
    /// </summary>
    public static float[]? TryLoadOgg(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            using var reader = new VorbisReader(filePath);

            int channels = reader.Channels;
            int sampleRate = reader.SampleRate;
            long totalSamples = reader.TotalSamples;

            // Read all interleaved samples
            var interleaved = new float[totalSamples * channels];
            int samplesRead = reader.ReadSamples(interleaved, 0, interleaved.Length);
            if (samplesRead <= 0)
                return null;

            // Downmix to mono if stereo
            float[] mono;
            if (channels >= 2)
            {
                int monoLength = samplesRead / channels;
                mono = new float[monoLength];
                for (int i = 0; i < monoLength; i++)
                {
                    float sum = 0f;
                    for (int ch = 0; ch < channels; ch++)
                        sum += interleaved[i * channels + ch];
                    mono[i] = sum / channels;
                }
            }
            else
            {
                mono = interleaved.Length == samplesRead
                    ? interleaved
                    : interleaved[..samplesRead];
            }

            // Resample if source rate differs from target
            if (sampleRate != TargetSampleRate)
            {
                mono = Resample(mono, sampleRate, TargetSampleRate);
            }

            Console.WriteLine($"Audio: Loaded OGG '{Path.GetFileName(filePath)}' " +
                              $"({channels}ch, {sampleRate}Hz, {mono.Length / (float)TargetSampleRate:F1}s)");
            return mono;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio: Failed to decode OGG '{filePath}' - {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Linear interpolation resampler.
    /// </summary>
    private static float[] Resample(float[] source, int sourceRate, int targetRate)
    {
        double ratio = (double)sourceRate / targetRate;
        int outputLength = (int)(source.Length / ratio);
        var output = new float[outputLength];

        for (int i = 0; i < outputLength; i++)
        {
            double srcPos = i * ratio;
            int srcIndex = (int)srcPos;
            double frac = srcPos - srcIndex;

            float sample1 = source[srcIndex];
            float sample2 = srcIndex + 1 < source.Length ? source[srcIndex + 1] : sample1;
            output[i] = (float)(sample1 + (sample2 - sample1) * frac);
        }

        return output;
    }
}
