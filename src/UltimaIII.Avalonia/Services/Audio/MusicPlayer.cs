using System;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Background music generator that creates looping chiptune patterns.
/// </summary>
public class MusicPlayer
{
    private readonly ChiptuneGenerator _generator;

    public MusicPlayer(ChiptuneGenerator generator)
    {
        _generator = generator;
    }

    /// <summary>
    /// Generates a music pattern for the given track.
    /// Returns a float array that can be looped for continuous playback.
    /// </summary>
    public float[] GenerateMusicPattern(MusicTrack track)
    {
        if (track == MusicTrack.None) return Array.Empty<float>();

        var pattern = MusicPatterns.GetPattern(track);
        return GeneratePatternSamples(pattern);
    }

    private float[] GeneratePatternSamples(MusicPatternData pattern)
    {
        // Multi-section arrangement: concatenate sections per SectionOrder
        if (pattern.Sections != null && pattern.SectionOrder != null)
        {
            return GenerateMultiSectionSamples(pattern);
        }

        // Legacy single-section path
        return GenerateSingleSectionSamples(
            pattern.MelodyNotes, pattern.BassNotes,
            pattern.MelodyWaveform, pattern.BassWaveform,
            pattern.MelodyVolume, pattern.BassVolume,
            pattern.HasPercussion, pattern.Tempo);
    }

    private float[] GenerateMultiSectionSamples(MusicPatternData pattern)
    {
        var allSamples = new System.Collections.Generic.List<float[]>();
        float melodyPhase = 0f;
        float bassPhase = 0f;

        foreach (int sectionIndex in pattern.SectionOrder!)
        {
            var section = pattern.Sections![sectionIndex];
            var sectionSamples = GenerateSingleSectionSamples(
                section.MelodyNotes, section.BassNotes,
                section.MelodyWaveform, section.BassWaveform,
                section.MelodyVolume, section.BassVolume,
                section.HasPercussion, pattern.Tempo,
                ref melodyPhase, ref bassPhase);
            allSamples.Add(sectionSamples);
        }

        // Concatenate all section samples
        int totalLength = 0;
        foreach (var s in allSamples) totalLength += s.Length;

        var result = new float[totalLength];
        int offset = 0;
        foreach (var s in allSamples)
        {
            Array.Copy(s, 0, result, offset, s.Length);
            offset += s.Length;
        }

        return result;
    }

    private float[] GenerateSingleSectionSamples(
        float[] melodyNotes, float[] bassNotes,
        WaveformType melodyWaveform, WaveformType bassWaveform,
        float melodyVolume, float bassVolume,
        bool hasPercussion, int tempo)
    {
        float mp = 0f, bp = 0f;
        return GenerateSingleSectionSamples(melodyNotes, bassNotes,
            melodyWaveform, bassWaveform, melodyVolume, bassVolume,
            hasPercussion, tempo, ref mp, ref bp);
    }

    private float[] GenerateSingleSectionSamples(
        float[] melodyNotes, float[] bassNotes,
        WaveformType melodyWaveform, WaveformType bassWaveform,
        float melodyVolume, float bassVolume,
        bool hasPercussion, int tempo,
        ref float melodyPhase, ref float bassPhase)
    {
        // Calculate samples per note based on tempo
        float beatsPerSecond = tempo / 60f;
        float secondsPerSixteenth = 1f / (beatsPerSecond * 4f);
        int samplesPerNote = (int)(ChiptuneGenerator.SampleRate * secondsPerSixteenth);

        // Total pattern length
        int patternLengthSamples = samplesPerNote * melodyNotes.Length;
        var result = new float[patternLengthSamples];
        var random = new Random();

        for (int i = 0; i < patternLengthSamples; i++)
        {
            int noteIndex = i / samplesPerNote;
            int sampleInNote = i % samplesPerNote;
            float noteProgress = (float)sampleInNote / samplesPerNote;

            float sample = 0f;

            // Melody
            if (noteIndex < melodyNotes.Length)
            {
                float freq = melodyNotes[noteIndex];
                if (freq > 0)
                {
                    float melodySample = GenerateWaveformSample(freq, melodyWaveform, ref melodyPhase);
                    float envelope = GetNoteEnvelope(noteProgress);
                    sample += melodySample * melodyVolume * envelope;
                }
            }

            // Bass (bass notes repeat at a different rate if shorter)
            int bassNoteIndex = noteIndex % bassNotes.Length;
            if (bassNoteIndex < bassNotes.Length)
            {
                float bassFreq = bassNotes[bassNoteIndex];
                if (bassFreq > 0)
                {
                    float bassSample = GenerateWaveformSample(bassFreq, bassWaveform, ref bassPhase);
                    sample += bassSample * bassVolume;
                }
            }

            // Percussion (if enabled)
            if (hasPercussion)
            {
                // Add kick on beats 1 and 3 (every 4 notes in 4/4)
                if (noteIndex % 4 == 0 && sampleInNote < samplesPerNote / 4)
                {
                    float kickProgress = (float)sampleInNote / (samplesPerNote / 4);
                    float kickFreq = 60f * (1f - kickProgress * 0.5f);
                    float kickPhase = 0f;
                    float kickSample = GenerateWaveformSample(kickFreq, WaveformType.Triangle, ref kickPhase);
                    sample += kickSample * 0.3f * (1f - kickProgress);
                }

                // Add hi-hat on off-beats
                if (noteIndex % 2 == 1 && sampleInNote < samplesPerNote / 8)
                {
                    float hihatProgress = (float)sampleInNote / (samplesPerNote / 8);
                    sample += ((float)random.NextDouble() * 2f - 1f) * 0.1f * (1f - hihatProgress);
                }
            }

            // Clamp output
            result[i] = Math.Clamp(sample, -1f, 1f);
        }

        return result;
    }

    private float GenerateWaveformSample(float frequency, WaveformType waveform, ref float phase)
    {
        float sample = waveform switch
        {
            WaveformType.Square => phase < 0.5f ? 1f : -1f,
            WaveformType.Triangle => phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase,
            WaveformType.Sawtooth => 2f * phase - 1f,
            _ => phase < 0.5f ? 1f : -1f
        };

        phase += frequency / ChiptuneGenerator.SampleRate;
        if (phase >= 1f) phase -= 1f;

        return sample;
    }

    private static float GetNoteEnvelope(float progress)
    {
        // Quick attack, sustain, and release
        if (progress < 0.05f)
        {
            return progress / 0.05f;
        }
        else if (progress < 0.8f)
        {
            return 1f - (progress - 0.05f) * 0.2f;
        }
        else
        {
            return 0.8f * (1f - (progress - 0.8f) / 0.2f);
        }
    }
}
