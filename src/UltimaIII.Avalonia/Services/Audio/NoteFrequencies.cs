using System;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Musical note frequencies for procedural music generation.
/// Frequencies are in Hz, based on A4 = 440Hz equal temperament.
/// </summary>
public static class NoteFrequencies
{
    // Octave 2 (Bass)
    public const float C2 = 65.41f;
    public const float Cs2 = 69.30f;
    public const float D2 = 73.42f;
    public const float Ds2 = 77.78f;
    public const float E2 = 82.41f;
    public const float F2 = 87.31f;
    public const float Fs2 = 92.50f;
    public const float G2 = 98.00f;
    public const float Gs2 = 103.83f;
    public const float A2 = 110.00f;
    public const float As2 = 116.54f;
    public const float B2 = 123.47f;

    // Octave 3
    public const float C3 = 130.81f;
    public const float Cs3 = 138.59f;
    public const float D3 = 146.83f;
    public const float Ds3 = 155.56f;
    public const float E3 = 164.81f;
    public const float F3 = 174.61f;
    public const float Fs3 = 185.00f;
    public const float G3 = 196.00f;
    public const float Gs3 = 207.65f;
    public const float A3 = 220.00f;
    public const float As3 = 233.08f;
    public const float B3 = 246.94f;

    // Octave 4 (Middle)
    public const float C4 = 261.63f;
    public const float Cs4 = 277.18f;
    public const float D4 = 293.66f;
    public const float Ds4 = 311.13f;
    public const float E4 = 329.63f;
    public const float F4 = 349.23f;
    public const float Fs4 = 369.99f;
    public const float G4 = 392.00f;
    public const float Gs4 = 415.30f;
    public const float A4 = 440.00f;
    public const float As4 = 466.16f;
    public const float B4 = 493.88f;

    // Octave 5
    public const float C5 = 523.25f;
    public const float Cs5 = 554.37f;
    public const float D5 = 587.33f;
    public const float Ds5 = 622.25f;
    public const float E5 = 659.25f;
    public const float F5 = 698.46f;
    public const float Fs5 = 739.99f;
    public const float G5 = 783.99f;
    public const float Gs5 = 830.61f;
    public const float A5 = 880.00f;
    public const float As5 = 932.33f;
    public const float B5 = 987.77f;

    // Octave 6 (High)
    public const float C6 = 1046.50f;
    public const float Cs6 = 1108.73f;
    public const float D6 = 1174.66f;
    public const float Ds6 = 1244.51f;
    public const float E6 = 1318.51f;
    public const float F6 = 1396.91f;
    public const float Fs6 = 1479.98f;
    public const float G6 = 1567.98f;

    /// <summary>
    /// Rest/silence marker (frequency of 0).
    /// </summary>
    public const float Rest = 0f;

    /// <summary>
    /// Gets frequency for a note shifted by semitones.
    /// </summary>
    public static float Transpose(float baseFreq, int semitones)
    {
        if (baseFreq <= 0) return 0;
        return baseFreq * MathF.Pow(2f, semitones / 12f);
    }

    /// <summary>
    /// Gets frequency shifted by octaves.
    /// </summary>
    public static float OctaveShift(float baseFreq, int octaves)
    {
        if (baseFreq <= 0) return 0;
        return baseFreq * MathF.Pow(2f, octaves);
    }
}
