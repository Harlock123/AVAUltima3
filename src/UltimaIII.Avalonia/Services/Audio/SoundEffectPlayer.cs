namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Generates procedural sound effects for game events.
/// </summary>
public class SoundEffectPlayer
{
    private readonly ChiptuneGenerator _generator;

    public SoundEffectPlayer(ChiptuneGenerator generator)
    {
        _generator = generator;
    }

    /// <summary>
    /// Generates audio samples for the specified sound effect.
    /// </summary>
    public float[]? GenerateEffect(SoundEffect effect)
    {
        return effect switch
        {
            // Movement
            SoundEffect.Footstep => GenerateFootstep(),
            SoundEffect.Blocked => GenerateBlocked(),

            // Doors
            SoundEffect.DoorOpen => GenerateDoorOpen(),
            SoundEffect.DoorLocked => GenerateDoorLocked(),

            // Combat - Melee
            SoundEffect.SwordSwing => GenerateSwordSwing(),
            SoundEffect.SwordHit => GenerateSwordHit(),
            SoundEffect.SwordMiss => GenerateSwordMiss(),

            // Combat - Results
            SoundEffect.MonsterDeath => GenerateMonsterDeath(),
            SoundEffect.PlayerHit => GeneratePlayerHit(),

            // Magic
            SoundEffect.SpellCast => GenerateSpellCast(),
            SoundEffect.SpellHeal => GenerateSpellHeal(),
            SoundEffect.SpellDamage => GenerateSpellDamage(),

            // UI
            SoundEffect.MenuSelect => GenerateMenuSelect(),
            SoundEffect.MenuConfirm => GenerateMenuConfirm(),
            SoundEffect.MenuCancel => GenerateMenuCancel(),

            // Pickups & Events
            SoundEffect.GoldPickup => GenerateGoldPickup(),
            SoundEffect.ItemPickup => GenerateItemPickup(),
            SoundEffect.LevelUp => GenerateLevelUp(),

            // Environment
            SoundEffect.Stairs => GenerateStairs(),
            SoundEffect.Teleport => GenerateTeleport(),

            _ => null
        };
    }

    // Movement sounds

    private float[] GenerateFootstep()
    {
        var noise = _generator.WhiteNoise(0.05f, 0.15f);
        return _generator.ApplyEnvelope(noise, 0.005f, 0.02f, 0.3f, 0.02f);
    }

    private float[] GenerateBlocked()
    {
        var bonk = _generator.SquareWave(NoteFrequencies.C2, 0.1f, 0.3f);
        return _generator.ApplyEnvelope(bonk, 0.01f, 0.05f, 0.2f, 0.04f);
    }

    // Door sounds

    private float[] GenerateDoorOpen()
    {
        return _generator.PitchSweep(NoteFrequencies.C3, NoteFrequencies.G4, 0.2f, 0.25f, WaveformType.Square);
    }

    private float[] GenerateDoorLocked()
    {
        var notes = new[] { NoteFrequencies.E4, NoteFrequencies.C4, NoteFrequencies.E4, NoteFrequencies.C4 };
        return _generator.Arpeggio(notes, 0.04f, 1, 0.3f, WaveformType.Square);
    }

    // Combat - Melee sounds

    private float[] GenerateSwordSwing()
    {
        return _generator.PitchSweep(2000f, 400f, 0.12f, 0.2f, WaveformType.Noise);
    }

    private float[] GenerateSwordHit()
    {
        var impact = _generator.SquareWave(NoteFrequencies.E3, 0.08f, 0.35f);
        var noise = _generator.WhiteNoise(0.06f, 0.25f);
        return _generator.Mix(
            _generator.ApplyEnvelope(impact, 0.005f, 0.03f, 0.2f, 0.04f),
            _generator.ApplyEnvelope(noise, 0.005f, 0.02f, 0.3f, 0.03f)
        );
    }

    private float[] GenerateSwordMiss()
    {
        return _generator.PitchSweep(1500f, 300f, 0.08f, 0.15f, WaveformType.Noise);
    }

    // Combat - Result sounds

    private float[] GenerateMonsterDeath()
    {
        var descend = _generator.PitchSweep(NoteFrequencies.A4, NoteFrequencies.C2, 0.3f, 0.3f, WaveformType.Square);
        var noise = _generator.WhiteNoise(0.15f, 0.2f);
        return _generator.Mix(
            _generator.ApplyEnvelope(descend, 0.01f, 0.1f, 0.5f, 0.15f),
            _generator.ApplyEnvelope(noise, 0.05f, 0.05f, 0.3f, 0.05f)
        );
    }

    private float[] GeneratePlayerHit()
    {
        var impact = _generator.PitchSweep(NoteFrequencies.E4, NoteFrequencies.E2, 0.15f, 0.35f, WaveformType.Square);
        return _generator.ApplyEnvelope(impact, 0.01f, 0.05f, 0.4f, 0.08f);
    }

    // Magic sounds

    private float[] GenerateSpellCast()
    {
        var notes = new[] { NoteFrequencies.C4, NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.C5 };
        var arpeggio = _generator.Arpeggio(notes, 0.06f, 2, 0.25f, WaveformType.Square);
        var shimmer = _generator.Arpeggio(
            new[] { NoteFrequencies.G5, NoteFrequencies.E5, NoteFrequencies.G5, NoteFrequencies.E5 },
            0.03f, 4, 0.15f, WaveformType.Triangle);
        return _generator.Mix(arpeggio, shimmer);
    }

    private float[] GenerateSpellHeal()
    {
        var notes = new[] { NoteFrequencies.C4, NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.C5, NoteFrequencies.E5 };
        var melody = _generator.Arpeggio(notes, 0.1f, 1, 0.3f, WaveformType.Triangle);
        return _generator.ApplyEnvelope(melody, 0.02f, 0.1f, 0.7f, 0.2f);
    }

    private float[] GenerateSpellDamage()
    {
        var descend = _generator.PitchSweep(NoteFrequencies.C5, NoteFrequencies.C3, 0.2f, 0.3f, WaveformType.Sawtooth);
        var noise = _generator.WhiteNoise(0.15f, 0.25f);
        return _generator.Mix(
            _generator.ApplyEnvelope(descend, 0.01f, 0.08f, 0.4f, 0.1f),
            _generator.ApplyEnvelope(noise, 0.02f, 0.05f, 0.3f, 0.08f)
        );
    }

    // UI sounds

    private float[] GenerateMenuSelect()
    {
        var beep = _generator.SquareWave(NoteFrequencies.G5, 0.05f, 0.2f);
        return _generator.ApplyEnvelope(beep, 0.005f, 0.01f, 0.6f, 0.03f);
    }

    private float[] GenerateMenuConfirm()
    {
        var tone1 = _generator.SquareWave(NoteFrequencies.C5, 0.06f, 0.25f);
        var tone2 = _generator.SquareWave(NoteFrequencies.G5, 0.08f, 0.25f);
        return _generator.Sequence(
            _generator.ApplyEnvelope(tone1, 0.005f, 0.02f, 0.5f, 0.02f),
            _generator.ApplyEnvelope(tone2, 0.005f, 0.02f, 0.5f, 0.04f)
        );
    }

    private float[] GenerateMenuCancel()
    {
        var tone1 = _generator.SquareWave(NoteFrequencies.E5, 0.05f, 0.2f);
        var tone2 = _generator.SquareWave(NoteFrequencies.C4, 0.08f, 0.2f);
        return _generator.Sequence(
            _generator.ApplyEnvelope(tone1, 0.005f, 0.015f, 0.4f, 0.02f),
            _generator.ApplyEnvelope(tone2, 0.005f, 0.02f, 0.3f, 0.04f)
        );
    }

    // Pickup and event sounds

    private float[] GenerateGoldPickup()
    {
        var notes = new[] { NoteFrequencies.E5, NoteFrequencies.G5, NoteFrequencies.E5, NoteFrequencies.C6 };
        var jingle = _generator.Arpeggio(notes, 0.04f, 1, 0.3f, WaveformType.Square);
        return _generator.ApplyEnvelope(jingle, 0.01f, 0.05f, 0.6f, 0.1f);
    }

    private float[] GenerateItemPickup()
    {
        var notes = new[] { NoteFrequencies.C5, NoteFrequencies.E5, NoteFrequencies.G5 };
        return _generator.Arpeggio(notes, 0.06f, 1, 0.3f, WaveformType.Triangle);
    }

    private float[] GenerateLevelUp()
    {
        var notes = new[]
        {
            NoteFrequencies.C4, NoteFrequencies.D4, NoteFrequencies.E4, NoteFrequencies.F4,
            NoteFrequencies.G4, NoteFrequencies.A4, NoteFrequencies.B4, NoteFrequencies.C5
        };
        var fanfare = _generator.Arpeggio(notes, 0.08f, 1, 0.35f, WaveformType.Square);
        var harmony = _generator.Arpeggio(
            new[] { NoteFrequencies.E4, NoteFrequencies.G4, NoteFrequencies.C5, NoteFrequencies.E5 },
            0.16f, 2, 0.2f, WaveformType.Triangle);
        return _generator.Mix(fanfare, harmony);
    }

    // Environment sounds

    private float[] GenerateStairs()
    {
        var down = _generator.PitchSweep(NoteFrequencies.G4, NoteFrequencies.C3, 0.15f, 0.2f, WaveformType.Triangle);
        var up = _generator.PitchSweep(NoteFrequencies.C3, NoteFrequencies.E4, 0.15f, 0.2f, WaveformType.Triangle);
        return _generator.Sequence(down, up);
    }

    private float[] GenerateTeleport()
    {
        var sweep = _generator.PitchSweep(NoteFrequencies.C3, NoteFrequencies.C6, 0.4f, 0.25f, WaveformType.Square);
        var shimmer = _generator.Arpeggio(
            new[] { NoteFrequencies.G5, NoteFrequencies.C6, NoteFrequencies.E6, NoteFrequencies.G6 },
            0.05f, 2, 0.15f, WaveformType.Triangle);
        return _generator.Mix(
            _generator.ApplyEnvelope(sweep, 0.05f, 0.1f, 0.5f, 0.2f),
            shimmer
        );
    }
}
