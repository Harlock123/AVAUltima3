using System;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Procedural chiptune waveform generator for authentic 8-bit audio synthesis.
/// </summary>
public class ChiptuneGenerator
{
    public const int SampleRate = 44100;

    /// <summary>
    /// Generates a square wave (classic 8-bit lead sound).
    /// </summary>
    public float[] SquareWave(float frequency, float duration, float volume = 0.3f)
    {
        int samples = (int)(SampleRate * duration);
        var buffer = new float[samples];
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            buffer[i] = (phase < 0.5f ? 1f : -1f) * volume;
            phase += frequency / SampleRate;
            if (phase >= 1f) phase -= 1f;
        }

        return buffer;
    }

    /// <summary>
    /// Generates a triangle wave (bass and softer tones).
    /// </summary>
    public float[] TriangleWave(float frequency, float duration, float volume = 0.4f)
    {
        int samples = (int)(SampleRate * duration);
        var buffer = new float[samples];
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            float value = phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase;
            buffer[i] = value * volume;
            phase += frequency / SampleRate;
            if (phase >= 1f) phase -= 1f;
        }

        return buffer;
    }

    /// <summary>
    /// Generates a sawtooth wave (rich, buzzy tones).
    /// </summary>
    public float[] SawtoothWave(float frequency, float duration, float volume = 0.25f)
    {
        int samples = (int)(SampleRate * duration);
        var buffer = new float[samples];
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            buffer[i] = (2f * phase - 1f) * volume;
            phase += frequency / SampleRate;
            if (phase >= 1f) phase -= 1f;
        }

        return buffer;
    }

    /// <summary>
    /// Generates white noise (percussion, explosions, footsteps).
    /// </summary>
    public float[] WhiteNoise(float duration, float volume = 0.2f)
    {
        int samples = (int)(SampleRate * duration);
        var buffer = new float[samples];
        var random = new Random();

        for (int i = 0; i < samples; i++)
        {
            buffer[i] = ((float)random.NextDouble() * 2f - 1f) * volume;
        }

        return buffer;
    }

    /// <summary>
    /// Generates silence.
    /// </summary>
    public float[] Silence(float duration)
    {
        return new float[(int)(SampleRate * duration)];
    }

    /// <summary>
    /// Applies an ADSR envelope to a buffer.
    /// </summary>
    public float[] ApplyEnvelope(float[] buffer, float attack, float decay, float sustainLevel, float release)
    {
        var result = new float[buffer.Length];
        int attackSamples = (int)(attack * SampleRate);
        int decaySamples = (int)(decay * SampleRate);

        for (int i = 0; i < buffer.Length; i++)
        {
            float envelope;
            if (i < attackSamples)
            {
                envelope = (float)i / attackSamples;
            }
            else if (i < attackSamples + decaySamples)
            {
                float decayProgress = (float)(i - attackSamples) / decaySamples;
                envelope = 1f - (1f - sustainLevel) * decayProgress;
            }
            else
            {
                envelope = sustainLevel;
            }

            result[i] = buffer[i] * envelope;
        }

        return result;
    }

    /// <summary>
    /// Generates a pitch sweep from start to end frequency.
    /// </summary>
    public float[] PitchSweep(float startFreq, float endFreq, float duration, float volume = 0.3f, WaveformType waveform = WaveformType.Square)
    {
        int samples = (int)(SampleRate * duration);
        var buffer = new float[samples];
        float phase = 0f;
        var random = new Random();

        for (int i = 0; i < samples; i++)
        {
            float progress = (float)i / samples;
            float freq = startFreq + (endFreq - startFreq) * progress;

            float value = waveform switch
            {
                WaveformType.Square => phase < 0.5f ? 1f : -1f,
                WaveformType.Triangle => phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase,
                WaveformType.Sawtooth => 2f * phase - 1f,
                WaveformType.Noise => (float)random.NextDouble() * 2f - 1f,
                _ => phase < 0.5f ? 1f : -1f
            };

            buffer[i] = value * volume * (1f - progress * 0.5f);
            phase += freq / SampleRate;
            if (phase >= 1f) phase -= 1f;
        }

        return buffer;
    }

    /// <summary>
    /// Generates an arpeggio from a sequence of frequencies.
    /// </summary>
    public float[] Arpeggio(float[] frequencies, float noteLength, int repeats = 1, float volume = 0.3f, WaveformType waveform = WaveformType.Square)
    {
        int samplesPerNote = (int)(SampleRate * noteLength);
        int totalSamples = samplesPerNote * frequencies.Length * repeats;
        var buffer = new float[totalSamples];
        float phase = 0f;

        for (int i = 0; i < totalSamples; i++)
        {
            int noteIndex = (i / samplesPerNote) % frequencies.Length;
            float freq = frequencies[noteIndex];

            float value;
            if (freq <= 0)
            {
                value = 0;
            }
            else
            {
                value = waveform switch
                {
                    WaveformType.Square => phase < 0.5f ? 1f : -1f,
                    WaveformType.Triangle => phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase,
                    WaveformType.Sawtooth => 2f * phase - 1f,
                    _ => phase < 0.5f ? 1f : -1f
                };
            }

            // Note envelope
            int sampleInNote = i % samplesPerNote;
            float noteProgress = (float)sampleInNote / samplesPerNote;
            float envelope = noteProgress < 0.1f ? noteProgress / 0.1f : 1f - (noteProgress - 0.1f) * 0.3f;
            envelope = MathF.Max(0, envelope);

            buffer[i] = value * volume * envelope;

            if (freq > 0)
            {
                phase += freq / SampleRate;
                if (phase >= 1f) phase -= 1f;
            }
        }

        return buffer;
    }

    /// <summary>
    /// Mixes multiple buffers together.
    /// </summary>
    public float[] Mix(params float[][] buffers)
    {
        if (buffers.Length == 0) return Array.Empty<float>();

        int maxLength = 0;
        foreach (var buf in buffers)
            if (buf.Length > maxLength) maxLength = buf.Length;

        var result = new float[maxLength];

        foreach (var buf in buffers)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                result[i] += buf[i];
            }
        }

        // Clamp to prevent clipping
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Clamp(result[i], -1f, 1f);
        }

        return result;
    }

    /// <summary>
    /// Concatenates buffers in sequence.
    /// </summary>
    public float[] Sequence(params float[][] buffers)
    {
        int totalLength = 0;
        foreach (var buf in buffers)
            totalLength += buf.Length;

        var result = new float[totalLength];
        int offset = 0;

        foreach (var buf in buffers)
        {
            Array.Copy(buf, 0, result, offset, buf.Length);
            offset += buf.Length;
        }

        return result;
    }
}

public enum WaveformType
{
    Square,
    Triangle,
    Sawtooth,
    Noise
}
