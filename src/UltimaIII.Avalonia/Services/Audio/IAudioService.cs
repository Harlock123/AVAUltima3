using System;

namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Interface for audio playback services.
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Gets or sets the master volume (0.0 to 1.0).
    /// </summary>
    float MasterVolume { get; set; }

    /// <summary>
    /// Gets or sets the music volume (0.0 to 1.0).
    /// </summary>
    float MusicVolume { get; set; }

    /// <summary>
    /// Gets or sets the sound effects volume (0.0 to 1.0).
    /// </summary>
    float SfxVolume { get; set; }

    /// <summary>
    /// Gets or sets whether audio is muted.
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Gets whether audio is currently available and initialized.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Plays a sound effect (fire-and-forget).
    /// </summary>
    void PlaySoundEffect(SoundEffect effect);

    /// <summary>
    /// Plays a music track, optionally with crossfade.
    /// </summary>
    void PlayMusic(MusicTrack track, bool crossfade = true);

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    void StopMusic(bool fadeOut = true);

    /// <summary>
    /// Pauses the currently playing music.
    /// </summary>
    void PauseMusic();

    /// <summary>
    /// Resumes paused music.
    /// </summary>
    void ResumeMusic();
}
