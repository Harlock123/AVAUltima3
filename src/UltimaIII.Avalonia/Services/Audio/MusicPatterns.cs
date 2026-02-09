using System;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Defines melodic and rhythmic patterns for each music track.
/// </summary>
public static class MusicPatterns
{
    /// <summary>
    /// Gets the pattern data for a music track.
    /// </summary>
    public static MusicPatternData GetPattern(MusicTrack track)
    {
        return track switch
        {
            MusicTrack.MainMenu => GetMainMenuPattern(),
            MusicTrack.Overworld => GetOverworldPattern(),
            MusicTrack.OverworldNight => GetOverworldNightPattern(),
            MusicTrack.Town => GetTownPattern(),
            MusicTrack.Dungeon => GetDungeonPattern(),
            MusicTrack.Combat => GetCombatPattern(),
            MusicTrack.Victory => GetVictoryPattern(),
            MusicTrack.Defeat => GetDefeatPattern(),
            MusicTrack.DungeonDoom => GetDungeonDoomPattern(),
            MusicTrack.DungeonDoomDeep => MakeDeepVariant(GetDungeonDoomPattern()),
            MusicTrack.DungeonFire => GetDungeonFirePattern(),
            MusicTrack.DungeonFireDeep => MakeDeepVariant(GetDungeonFirePattern()),
            MusicTrack.DungeonTime => GetDungeonTimePattern(),
            MusicTrack.DungeonTimeDeep => MakeDeepVariant(GetDungeonTimePattern()),
            MusicTrack.DungeonSnake => GetDungeonSnakePattern(),
            MusicTrack.DungeonSnakeDeep => MakeDeepVariant(GetDungeonSnakePattern()),
            MusicTrack.CombatDoom => GetCombatDoomPattern(),
            MusicTrack.CombatFire => GetCombatFirePattern(),
            MusicTrack.CombatTime => GetCombatTimePattern(),
            MusicTrack.CombatSnake => GetCombatSnakePattern(),
            _ => GetMainMenuPattern()
        };
    }

    private static MusicPatternData GetMainMenuPattern()
    {
        // Epic/mysterious - slow arpeggios, minor key
        return new MusicPatternData
        {
            Tempo = 80,
            MelodyNotes = new[]
            {
                NoteFrequencies.E4, NoteFrequencies.Rest, NoteFrequencies.G4, NoteFrequencies.Rest,
                NoteFrequencies.B4, NoteFrequencies.Rest, NoteFrequencies.E5, NoteFrequencies.Rest,
                NoteFrequencies.D5, NoteFrequencies.Rest, NoteFrequencies.B4, NoteFrequencies.Rest,
                NoteFrequencies.G4, NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.C5, NoteFrequencies.Rest,
                NoteFrequencies.E5, NoteFrequencies.Rest, NoteFrequencies.A5, NoteFrequencies.Rest,
                NoteFrequencies.G5, NoteFrequencies.Rest, NoteFrequencies.E5, NoteFrequencies.Rest,
                NoteFrequencies.C5, NoteFrequencies.Rest, NoteFrequencies.A4, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.25f,
            BassVolume = 0.3f
        };
    }

    private static MusicPatternData GetOverworldPattern()
    {
        // Adventurous march - bright, major key, steady rhythm
        return new MusicPatternData
        {
            Tempo = 120,
            MelodyNotes = new[]
            {
                NoteFrequencies.C4, NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.C5,
                NoteFrequencies.B4, NoteFrequencies.G4, NoteFrequencies.E4, NoteFrequencies.D4,
                NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.C5, NoteFrequencies.E5,
                NoteFrequencies.D5, NoteFrequencies.C5, NoteFrequencies.B4, NoteFrequencies.G4,
                NoteFrequencies.F4, NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.F5,
                NoteFrequencies.E5, NoteFrequencies.C5, NoteFrequencies.A4, NoteFrequencies.G4,
                NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.E5, NoteFrequencies.G5,
                NoteFrequencies.F5, NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.C5
            },
            BassNotes = new[]
            {
                NoteFrequencies.C2, NoteFrequencies.G2, NoteFrequencies.C2, NoteFrequencies.G2,
                NoteFrequencies.C2, NoteFrequencies.G2, NoteFrequencies.C2, NoteFrequencies.G2,
                NoteFrequencies.A2, NoteFrequencies.E3, NoteFrequencies.A2, NoteFrequencies.E3,
                NoteFrequencies.F2, NoteFrequencies.C3, NoteFrequencies.G2, NoteFrequencies.D3
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.25f,
            BassVolume = 0.35f
        };
    }

    private static MusicPatternData GetOverworldNightPattern()
    {
        // Darker variation - lower octave, slower, minor mode
        return new MusicPatternData
        {
            Tempo = 90,
            MelodyNotes = new[]
            {
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.C4, NoteFrequencies.Rest,
                NoteFrequencies.E4, NoteFrequencies.Rest, NoteFrequencies.A4, NoteFrequencies.Rest,
                NoteFrequencies.G4, NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest,
                NoteFrequencies.C4, NoteFrequencies.Rest, NoteFrequencies.B3, NoteFrequencies.Rest,
                NoteFrequencies.D4, NoteFrequencies.Rest, NoteFrequencies.F4, NoteFrequencies.Rest,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.D5, NoteFrequencies.Rest,
                NoteFrequencies.C5, NoteFrequencies.Rest, NoteFrequencies.A4, NoteFrequencies.Rest,
                NoteFrequencies.F4, NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.D2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2
            },
            MelodyWaveform = WaveformType.Triangle,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.2f,
            BassVolume = 0.25f
        };
    }

    private static MusicPatternData GetTownPattern()
    {
        // Peaceful medieval - simple folk melody
        return new MusicPatternData
        {
            Tempo = 100,
            MelodyNotes = new[]
            {
                NoteFrequencies.G4, NoteFrequencies.A4, NoteFrequencies.B4, NoteFrequencies.C5,
                NoteFrequencies.D5, NoteFrequencies.C5, NoteFrequencies.B4, NoteFrequencies.A4,
                NoteFrequencies.G4, NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest,
                NoteFrequencies.D4, NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.Rest,
                NoteFrequencies.A4, NoteFrequencies.B4, NoteFrequencies.C5, NoteFrequencies.D5,
                NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.C5, NoteFrequencies.B4,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.G4, NoteFrequencies.Rest,
                NoteFrequencies.E4, NoteFrequencies.D4, NoteFrequencies.G4, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.G2, NoteFrequencies.D3, NoteFrequencies.G2, NoteFrequencies.D3,
                NoteFrequencies.C3, NoteFrequencies.G3, NoteFrequencies.C3, NoteFrequencies.G3,
                NoteFrequencies.A2, NoteFrequencies.E3, NoteFrequencies.A2, NoteFrequencies.E3,
                NoteFrequencies.D3, NoteFrequencies.A3, NoteFrequencies.G2, NoteFrequencies.D3
            },
            MelodyWaveform = WaveformType.Triangle,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.25f,
            BassVolume = 0.25f
        };
    }

    private static MusicPatternData GetDungeonPattern()
    {
        // Tense/foreboding - dissonant intervals, sparse
        return new MusicPatternData
        {
            Tempo = 60,
            MelodyNotes = new[]
            {
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Ds3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.B3, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Gs3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.E3, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.E2, NoteFrequencies.Rest, NoteFrequencies.E2, NoteFrequencies.Rest,
                NoteFrequencies.E2, NoteFrequencies.Rest, NoteFrequencies.E2, NoteFrequencies.Rest,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.A2, NoteFrequencies.Rest,
                NoteFrequencies.E2, NoteFrequencies.Rest, NoteFrequencies.E2, NoteFrequencies.Rest
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.2f,
            BassVolume = 0.2f
        };
    }

    private static MusicPatternData GetCombatPattern()
    {
        // Fast/urgent - rapid arpeggios, driving bass
        return new MusicPatternData
        {
            Tempo = 160,
            MelodyNotes = new[]
            {
                NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.E5, NoteFrequencies.A5,
                NoteFrequencies.E5, NoteFrequencies.C5, NoteFrequencies.A4, NoteFrequencies.E4,
                NoteFrequencies.G4, NoteFrequencies.B4, NoteFrequencies.D5, NoteFrequencies.G5,
                NoteFrequencies.D5, NoteFrequencies.B4, NoteFrequencies.G4, NoteFrequencies.D4,
                NoteFrequencies.F4, NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.F5,
                NoteFrequencies.C5, NoteFrequencies.A4, NoteFrequencies.F4, NoteFrequencies.C4,
                NoteFrequencies.E4, NoteFrequencies.Gs4, NoteFrequencies.B4, NoteFrequencies.E5,
                NoteFrequencies.B4, NoteFrequencies.Gs4, NoteFrequencies.E4, NoteFrequencies.B3
            },
            BassNotes = new[]
            {
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.E3, NoteFrequencies.A2,
                NoteFrequencies.G2, NoteFrequencies.G2, NoteFrequencies.D3, NoteFrequencies.G2,
                NoteFrequencies.F2, NoteFrequencies.F2, NoteFrequencies.C3, NoteFrequencies.F2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.B2, NoteFrequencies.E2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Sawtooth,
            MelodyVolume = 0.25f,
            BassVolume = 0.3f,
            HasPercussion = true
        };
    }

    private static MusicPatternData GetVictoryPattern()
    {
        // Triumphant fanfare - major key ascending
        return new MusicPatternData
        {
            Tempo = 140,
            MelodyNotes = new[]
            {
                NoteFrequencies.C5, NoteFrequencies.C5, NoteFrequencies.G4, NoteFrequencies.G4,
                NoteFrequencies.A4, NoteFrequencies.A4, NoteFrequencies.G4, NoteFrequencies.Rest,
                NoteFrequencies.F4, NoteFrequencies.F4, NoteFrequencies.E4, NoteFrequencies.E4,
                NoteFrequencies.D4, NoteFrequencies.D4, NoteFrequencies.C4, NoteFrequencies.Rest,
                NoteFrequencies.C5, NoteFrequencies.D5, NoteFrequencies.E5, NoteFrequencies.F5,
                NoteFrequencies.G5, NoteFrequencies.G5, NoteFrequencies.G5, NoteFrequencies.Rest,
                NoteFrequencies.A5, NoteFrequencies.G5, NoteFrequencies.F5, NoteFrequencies.E5,
                NoteFrequencies.D5, NoteFrequencies.E5, NoteFrequencies.C5, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.C3, NoteFrequencies.G3, NoteFrequencies.C3, NoteFrequencies.G3,
                NoteFrequencies.F2, NoteFrequencies.C3, NoteFrequencies.G2, NoteFrequencies.D3,
                NoteFrequencies.C3, NoteFrequencies.E3, NoteFrequencies.G3, NoteFrequencies.C4,
                NoteFrequencies.F3, NoteFrequencies.G3, NoteFrequencies.C3, NoteFrequencies.G3
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.3f,
            BassVolume = 0.35f
        };
    }

    private static MusicPatternData GetDefeatPattern()
    {
        // Somber - descending minor, slow
        return new MusicPatternData
        {
            Tempo = 60,
            MelodyNotes = new[]
            {
                NoteFrequencies.E4, NoteFrequencies.Rest, NoteFrequencies.D4, NoteFrequencies.Rest,
                NoteFrequencies.C4, NoteFrequencies.Rest, NoteFrequencies.B3, NoteFrequencies.Rest,
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.D4, NoteFrequencies.Rest, NoteFrequencies.C4, NoteFrequencies.Rest,
                NoteFrequencies.B3, NoteFrequencies.Rest, NoteFrequencies.A3, NoteFrequencies.Rest,
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.A2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            MelodyWaveform = WaveformType.Triangle,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.2f,
            BassVolume = 0.2f
        };
    }

    // ── Per-Dungeon Exploration Themes ──

    private static MusicPatternData GetDungeonDoomPattern()
    {
        // Doom: Heavy, dreadful, E minor with tritone intervals (E-Bb)
        return new MusicPatternData
        {
            Tempo = 55,
            MelodyNotes = new[]
            {
                NoteFrequencies.E3, NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.As3, NoteFrequencies.Rest,
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.B3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.E3, NoteFrequencies.E3,
                NoteFrequencies.Ds3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.As3, NoteFrequencies.Rest, NoteFrequencies.A3, NoteFrequencies.Rest,
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2,
                NoteFrequencies.As2, NoteFrequencies.As2, NoteFrequencies.A2, NoteFrequencies.A2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.Rest
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.22f,
            BassVolume = 0.25f
        };
    }

    private static MusicPatternData GetDungeonFirePattern()
    {
        // Fire: Aggressive, urgent, D minor with fast chromatic runs
        return new MusicPatternData
        {
            Tempo = 80,
            MelodyNotes = new[]
            {
                NoteFrequencies.D4, NoteFrequencies.Ds4, NoteFrequencies.E4, NoteFrequencies.F4,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.F4, NoteFrequencies.E4,
                NoteFrequencies.D4, NoteFrequencies.Cs4, NoteFrequencies.D4, NoteFrequencies.Rest,
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.D4, NoteFrequencies.Rest,
                NoteFrequencies.F4, NoteFrequencies.G4, NoteFrequencies.A4, NoteFrequencies.As4,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.G4, NoteFrequencies.F4,
                NoteFrequencies.E4, NoteFrequencies.D4, NoteFrequencies.Cs4, NoteFrequencies.D4,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.A3, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.A2, NoteFrequencies.D2,
                NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.A2, NoteFrequencies.D2,
                NoteFrequencies.F2, NoteFrequencies.F2, NoteFrequencies.C3, NoteFrequencies.F2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.A2, NoteFrequencies.D2
            },
            MelodyWaveform = WaveformType.Sawtooth,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.22f,
            BassVolume = 0.25f
        };
    }

    private static MusicPatternData GetDungeonTimePattern()
    {
        // Time: Ethereal, sparse, B minor, bell-like quality, odd phrasing
        return new MusicPatternData
        {
            Tempo = 65,
            MelodyNotes = new[]
            {
                NoteFrequencies.B4, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Fs4,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.D4,
                NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.Fs4, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.A4, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.B4,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Fs4, NoteFrequencies.Rest,
                NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.E4, NoteFrequencies.Rest,
                NoteFrequencies.D4, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.B2, NoteFrequencies.Rest, NoteFrequencies.Fs2, NoteFrequencies.Rest,
                NoteFrequencies.B2, NoteFrequencies.Rest, NoteFrequencies.E2, NoteFrequencies.Rest,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.Fs2, NoteFrequencies.Rest,
                NoteFrequencies.B2, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            MelodyWaveform = WaveformType.Triangle,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.2f,
            BassVolume = 0.2f
        };
    }

    private static MusicPatternData GetDungeonSnakePattern()
    {
        // Snake: Sinuous, creeping, A minor, stalking melody
        return new MusicPatternData
        {
            Tempo = 70,
            MelodyNotes = new[]
            {
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.B3, NoteFrequencies.C4,
                NoteFrequencies.B3, NoteFrequencies.Rest, NoteFrequencies.A3, NoteFrequencies.Rest,
                NoteFrequencies.Gs3, NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.E3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest,
                NoteFrequencies.A3, NoteFrequencies.B3, NoteFrequencies.C4, NoteFrequencies.D4,
                NoteFrequencies.E4, NoteFrequencies.Rest, NoteFrequencies.D4, NoteFrequencies.C4,
                NoteFrequencies.B3, NoteFrequencies.Rest, NoteFrequencies.A3, NoteFrequencies.Gs3,
                NoteFrequencies.A3, NoteFrequencies.Rest, NoteFrequencies.Rest, NoteFrequencies.Rest
            },
            BassNotes = new[]
            {
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.A2, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.A2, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.C3, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.Rest, NoteFrequencies.Gs2, NoteFrequencies.A2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.2f,
            BassVolume = 0.22f
        };
    }

    // ── Deep Variant Helper ──

    private static MusicPatternData MakeDeepVariant(MusicPatternData basePattern)
    {
        return new MusicPatternData
        {
            Tempo = (int)(basePattern.Tempo * 0.75f),
            MelodyNotes = Array.ConvertAll(basePattern.MelodyNotes, n => n > 0 ? n * 0.5f : 0f),
            BassNotes = Array.ConvertAll(basePattern.BassNotes, n => n > 0 ? n * 0.5f : 0f),
            MelodyWaveform = WaveformType.Sawtooth,
            BassWaveform = basePattern.BassWaveform,
            MelodyVolume = basePattern.MelodyVolume * 0.8f,
            BassVolume = basePattern.BassVolume * 1.2f,
            HasPercussion = basePattern.HasPercussion
        };
    }

    // ── Per-Dungeon Combat Themes ──

    private static MusicPatternData GetCombatDoomPattern()
    {
        // Doom combat: Heavy, pounding, E minor, relentless
        return new MusicPatternData
        {
            Tempo = 140,
            MelodyNotes = new[]
            {
                NoteFrequencies.E4, NoteFrequencies.E4, NoteFrequencies.As4, NoteFrequencies.E4,
                NoteFrequencies.B4, NoteFrequencies.E4, NoteFrequencies.As4, NoteFrequencies.A4,
                NoteFrequencies.E4, NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.A4,
                NoteFrequencies.As4, NoteFrequencies.A4, NoteFrequencies.G4, NoteFrequencies.E4,
                NoteFrequencies.E4, NoteFrequencies.E4, NoteFrequencies.B4, NoteFrequencies.E4,
                NoteFrequencies.As4, NoteFrequencies.E4, NoteFrequencies.A4, NoteFrequencies.G4,
                NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.A4, NoteFrequencies.As4,
                NoteFrequencies.B4, NoteFrequencies.As4, NoteFrequencies.A4, NoteFrequencies.E4
            },
            BassNotes = new[]
            {
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.E3, NoteFrequencies.E2,
                NoteFrequencies.As2, NoteFrequencies.As2, NoteFrequencies.E3, NoteFrequencies.As2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.E3, NoteFrequencies.A2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.B2, NoteFrequencies.E2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Sawtooth,
            MelodyVolume = 0.25f,
            BassVolume = 0.3f,
            HasPercussion = true
        };
    }

    private static MusicPatternData GetCombatFirePattern()
    {
        // Fire combat: Frantic, D minor, blazing fast sawtooth
        return new MusicPatternData
        {
            Tempo = 180,
            MelodyNotes = new[]
            {
                NoteFrequencies.D5, NoteFrequencies.A4, NoteFrequencies.D5, NoteFrequencies.F5,
                NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.Cs5, NoteFrequencies.D5,
                NoteFrequencies.A4, NoteFrequencies.D5, NoteFrequencies.F5, NoteFrequencies.G5,
                NoteFrequencies.F5, NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.A4,
                NoteFrequencies.As4, NoteFrequencies.A4, NoteFrequencies.G4, NoteFrequencies.A4,
                NoteFrequencies.D5, NoteFrequencies.Cs5, NoteFrequencies.D5, NoteFrequencies.F5,
                NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.Cs5, NoteFrequencies.A4,
                NoteFrequencies.D5, NoteFrequencies.E5, NoteFrequencies.F5, NoteFrequencies.D5
            },
            BassNotes = new[]
            {
                NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.A2, NoteFrequencies.D2,
                NoteFrequencies.D2, NoteFrequencies.D2, NoteFrequencies.F2, NoteFrequencies.D2,
                NoteFrequencies.As2, NoteFrequencies.As2, NoteFrequencies.F2, NoteFrequencies.As2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.E2, NoteFrequencies.D2
            },
            MelodyWaveform = WaveformType.Sawtooth,
            BassWaveform = WaveformType.Sawtooth,
            MelodyVolume = 0.25f,
            BassVolume = 0.3f,
            HasPercussion = true
        };
    }

    private static MusicPatternData GetCombatTimePattern()
    {
        // Time combat: Mysterious, B minor, dissonant arpeggios
        return new MusicPatternData
        {
            Tempo = 130,
            MelodyNotes = new[]
            {
                NoteFrequencies.B4, NoteFrequencies.D5, NoteFrequencies.Fs5, NoteFrequencies.B5,
                NoteFrequencies.Fs5, NoteFrequencies.D5, NoteFrequencies.B4, NoteFrequencies.A4,
                NoteFrequencies.E4, NoteFrequencies.Fs4, NoteFrequencies.A4, NoteFrequencies.E5,
                NoteFrequencies.A4, NoteFrequencies.Fs4, NoteFrequencies.E4, NoteFrequencies.D4,
                NoteFrequencies.B4, NoteFrequencies.Cs5, NoteFrequencies.D5, NoteFrequencies.Fs5,
                NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.Cs5, NoteFrequencies.B4,
                NoteFrequencies.A4, NoteFrequencies.B4, NoteFrequencies.D5, NoteFrequencies.E5,
                NoteFrequencies.Fs5, NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.B4
            },
            BassNotes = new[]
            {
                NoteFrequencies.B2, NoteFrequencies.B2, NoteFrequencies.Fs2, NoteFrequencies.B2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.B2, NoteFrequencies.E2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.E2, NoteFrequencies.A2,
                NoteFrequencies.B2, NoteFrequencies.B2, NoteFrequencies.Fs2, NoteFrequencies.B2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.25f,
            BassVolume = 0.28f,
            HasPercussion = true
        };
    }

    private static MusicPatternData GetCombatSnakePattern()
    {
        // Snake combat: Sneaky-fast, A minor, darting runs
        return new MusicPatternData
        {
            Tempo = 150,
            MelodyNotes = new[]
            {
                NoteFrequencies.A4, NoteFrequencies.B4, NoteFrequencies.C5, NoteFrequencies.E5,
                NoteFrequencies.D5, NoteFrequencies.C5, NoteFrequencies.B4, NoteFrequencies.A4,
                NoteFrequencies.Gs4, NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.A4,
                NoteFrequencies.E4, NoteFrequencies.Gs4, NoteFrequencies.A4, NoteFrequencies.Rest,
                NoteFrequencies.A4, NoteFrequencies.C5, NoteFrequencies.D5, NoteFrequencies.E5,
                NoteFrequencies.F5, NoteFrequencies.E5, NoteFrequencies.D5, NoteFrequencies.C5,
                NoteFrequencies.B4, NoteFrequencies.A4, NoteFrequencies.Gs4, NoteFrequencies.A4,
                NoteFrequencies.B4, NoteFrequencies.C5, NoteFrequencies.A4, NoteFrequencies.E4
            },
            BassNotes = new[]
            {
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.E3, NoteFrequencies.A2,
                NoteFrequencies.A2, NoteFrequencies.A2, NoteFrequencies.C3, NoteFrequencies.A2,
                NoteFrequencies.F2, NoteFrequencies.F2, NoteFrequencies.C3, NoteFrequencies.F2,
                NoteFrequencies.E2, NoteFrequencies.E2, NoteFrequencies.Gs2, NoteFrequencies.A2
            },
            MelodyWaveform = WaveformType.Square,
            BassWaveform = WaveformType.Triangle,
            MelodyVolume = 0.25f,
            BassVolume = 0.28f,
            HasPercussion = true
        };
    }
}

/// <summary>
/// Data container for a music pattern.
/// </summary>
public class MusicPatternData
{
    public int Tempo { get; set; } = 120;
    public float[] MelodyNotes { get; set; } = Array.Empty<float>();
    public float[] BassNotes { get; set; } = Array.Empty<float>();
    public WaveformType MelodyWaveform { get; set; } = WaveformType.Square;
    public WaveformType BassWaveform { get; set; } = WaveformType.Triangle;
    public float MelodyVolume { get; set; } = 0.25f;
    public float BassVolume { get; set; } = 0.3f;
    public bool HasPercussion { get; set; } = false;
}
