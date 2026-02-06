using System;
using System.Collections.Generic;
using System.Threading;
using Silk.NET.OpenAL;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Cross-platform audio service using OpenAL for procedural chiptune playback.
/// </summary>
public unsafe class AudioService : IAudioService
{
    private static AudioService? _instance;
    private static readonly object _lock = new();

    private AL? _al;
    private ALContext? _alc;
    private Device* _device;
    private Context* _context;

    private readonly ChiptuneGenerator _generator;
    private readonly SoundEffectPlayer _effectPlayer;
    private readonly MusicPlayer _musicPlayerService;

    private uint _musicSource;
    private uint _musicBuffer1;
    private uint _musicBuffer2;
    private float[]? _currentMusicPattern;
    private int _musicPatternPosition;
    private Thread? _musicThread;
    private volatile bool _musicPlaying;
    private volatile bool _disposed;

    private readonly List<uint> _sfxSources = new();
    private readonly object _sfxLock = new();

    private MusicTrack _currentTrack = MusicTrack.None;

    private float _masterVolume = 0.7f;
    private float _musicVolumeLevel = 0.5f;
    private float _sfxVolumeLevel = 0.8f;
    private bool _isMuted;

    public static AudioService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AudioService();
                }
            }
            return _instance;
        }
    }

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Math.Clamp(value, 0f, 1f);
            UpdateMusicVolume();
        }
    }

    public float MusicVolume
    {
        get => _musicVolumeLevel;
        set
        {
            _musicVolumeLevel = Math.Clamp(value, 0f, 1f);
            UpdateMusicVolume();
        }
    }

    public float SfxVolume
    {
        get => _sfxVolumeLevel;
        set => _sfxVolumeLevel = Math.Clamp(value, 0f, 1f);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            UpdateMusicVolume();
        }
    }

    public bool IsAvailable { get; private set; }

    private AudioService()
    {
        _generator = new ChiptuneGenerator();
        _effectPlayer = new SoundEffectPlayer(_generator);
        _musicPlayerService = new MusicPlayer(_generator);

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();

            _device = _alc.OpenDevice(null);
            if (_device == null)
            {
                Console.WriteLine("Audio: Failed to open audio device");
                IsAvailable = false;
                return;
            }

            _context = _alc.CreateContext(_device, null);
            if (_context == null)
            {
                Console.WriteLine("Audio: Failed to create audio context");
                _alc.CloseDevice(_device);
                IsAvailable = false;
                return;
            }

            _alc.MakeContextCurrent(_context);

            // Create music source and buffers
            _musicSource = _al.GenSource();
            _musicBuffer1 = _al.GenBuffer();
            _musicBuffer2 = _al.GenBuffer();

            // Set music source properties
            _al.SetSourceProperty(_musicSource, SourceBoolean.Looping, false);
            UpdateMusicVolume();

            IsAvailable = true;
            Console.WriteLine("Audio: OpenAL initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio: Initialization failed - {ex.Message}");
            IsAvailable = false;
        }
    }

    private void UpdateMusicVolume()
    {
        if (!IsAvailable || _al == null) return;

        float volume = _isMuted ? 0f : _masterVolume * _musicVolumeLevel;
        _al.SetSourceProperty(_musicSource, SourceFloat.Gain, volume);
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (!IsAvailable || _isMuted || _al == null) return;

        try
        {
            var samples = _effectPlayer.GenerateEffect(effect);
            if (samples == null || samples.Length == 0) return;

            // Convert float samples to 16-bit PCM
            var pcmData = ConvertToPcm16(samples);

            uint buffer = _al.GenBuffer();
            uint source = _al.GenSource();

            fixed (short* pData = pcmData)
            {
                _al.BufferData(buffer, BufferFormat.Mono16, pData, pcmData.Length * sizeof(short), ChiptuneGenerator.SampleRate);
            }

            _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
            _al.SetSourceProperty(source, SourceFloat.Gain, _masterVolume * _sfxVolumeLevel);
            _al.SourcePlay(source);

            // Track for cleanup
            lock (_sfxLock)
            {
                _sfxSources.Add(source);
            }

            // Clean up finished sources periodically
            CleanupFinishedSources();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio: Error playing SFX {effect} - {ex.Message}");
        }
    }

    private void CleanupFinishedSources()
    {
        if (_al == null) return;

        lock (_sfxLock)
        {
            for (int i = _sfxSources.Count - 1; i >= 0; i--)
            {
                uint source = _sfxSources[i];
                _al.GetSourceProperty(source, GetSourceInteger.SourceState, out int state);

                if ((SourceState)state == SourceState.Stopped)
                {
                    _al.GetSourceProperty(source, GetSourceInteger.Buffer, out int buffer);
                    _al.DeleteSource(source);
                    if (buffer != 0)
                    {
                        _al.DeleteBuffer((uint)buffer);
                    }
                    _sfxSources.RemoveAt(i);
                }
            }
        }
    }

    public void PlayMusic(MusicTrack track, bool crossfade = true)
    {
        if (!IsAvailable || _al == null) return;
        if (track == _currentTrack) return;

        StopMusic(false);

        _currentTrack = track;

        if (track == MusicTrack.None) return;

        try
        {
            _currentMusicPattern = _musicPlayerService.GenerateMusicPattern(track);
            if (_currentMusicPattern == null || _currentMusicPattern.Length == 0) return;

            _musicPatternPosition = 0;
            _musicPlaying = true;

            // Start music playback thread
            _musicThread = new Thread(MusicPlaybackLoop)
            {
                IsBackground = true,
                Name = "MusicPlayback"
            };
            _musicThread.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio: Error playing music {track} - {ex.Message}");
        }
    }

    private void MusicPlaybackLoop()
    {
        if (_al == null || _currentMusicPattern == null) return;

        const int bufferSizeSamples = 4096;
        var pcmBuffer = new short[bufferSizeSamples];

        try
        {
            // Fill initial buffers
            FillMusicBuffer(_musicBuffer1, pcmBuffer);
            FillMusicBuffer(_musicBuffer2, pcmBuffer);

            // Queue buffers and start playing
            _al.SourceQueueBuffers(_musicSource, new[] { _musicBuffer1, _musicBuffer2 });
            _al.SourcePlay(_musicSource);

            while (_musicPlaying && !_disposed)
            {
                _al.GetSourceProperty(_musicSource, GetSourceInteger.BuffersProcessed, out int processed);

                while (processed > 0 && _musicPlaying)
                {
                    var unqueuedBuffers = new uint[1];
                    _al.SourceUnqueueBuffers(_musicSource, unqueuedBuffers);

                    FillMusicBuffer(unqueuedBuffers[0], pcmBuffer);
                    _al.SourceQueueBuffers(_musicSource, new[] { unqueuedBuffers[0] });

                    processed--;
                }

                // Check if source stopped (buffer underrun)
                _al.GetSourceProperty(_musicSource, GetSourceInteger.SourceState, out int state);
                if ((SourceState)state == SourceState.Stopped && _musicPlaying)
                {
                    _al.SourcePlay(_musicSource);
                }

                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio: Music playback error - {ex.Message}");
        }
    }

    private void FillMusicBuffer(uint buffer, short[] pcmBuffer)
    {
        if (_al == null || _currentMusicPattern == null) return;

        for (int i = 0; i < pcmBuffer.Length; i++)
        {
            float sample = _currentMusicPattern[_musicPatternPosition];
            pcmBuffer[i] = (short)(sample * 32767f);

            _musicPatternPosition++;
            if (_musicPatternPosition >= _currentMusicPattern.Length)
            {
                _musicPatternPosition = 0; // Loop
            }
        }

        fixed (short* pData = pcmBuffer)
        {
            _al.BufferData(buffer, BufferFormat.Mono16, pData, pcmBuffer.Length * sizeof(short), ChiptuneGenerator.SampleRate);
        }
    }

    public void StopMusic(bool fadeOut = true)
    {
        _musicPlaying = false;
        _currentTrack = MusicTrack.None;

        if (_musicThread != null && _musicThread.IsAlive)
        {
            _musicThread.Join(100);
        }

        if (_al != null && IsAvailable)
        {
            _al.SourceStop(_musicSource);

            // Unqueue all buffers
            _al.GetSourceProperty(_musicSource, GetSourceInteger.BuffersQueued, out int queued);
            if (queued > 0)
            {
                var buffers = new uint[queued];
                _al.SourceUnqueueBuffers(_musicSource, buffers);
            }
        }
    }

    public void PauseMusic()
    {
        if (_al != null && IsAvailable)
        {
            _al.SourcePause(_musicSource);
        }
    }

    public void ResumeMusic()
    {
        if (_al != null && IsAvailable)
        {
            _al.SourcePlay(_musicSource);
        }
    }

    private static short[] ConvertToPcm16(float[] samples)
    {
        var pcm = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            pcm[i] = (short)(Math.Clamp(samples[i], -1f, 1f) * 32767f);
        }
        return pcm;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopMusic(false);

        if (_al != null && IsAvailable)
        {
            // Cleanup SFX sources
            lock (_sfxLock)
            {
                foreach (var source in _sfxSources)
                {
                    _al.GetSourceProperty(source, GetSourceInteger.Buffer, out int buffer);
                    _al.DeleteSource(source);
                    if (buffer != 0)
                    {
                        _al.DeleteBuffer((uint)buffer);
                    }
                }
                _sfxSources.Clear();
            }

            // Cleanup music
            _al.DeleteSource(_musicSource);
            _al.DeleteBuffer(_musicBuffer1);
            _al.DeleteBuffer(_musicBuffer2);
        }

        if (_alc != null)
        {
            if (_context != null)
            {
                _alc.MakeContextCurrent(null);
                _alc.DestroyContext(_context);
            }
            if (_device != null)
            {
                _alc.CloseDevice(_device);
            }
        }

        _al?.Dispose();
        _alc?.Dispose();

        _instance = null;
    }
}
