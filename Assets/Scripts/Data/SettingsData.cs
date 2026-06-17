using System;

/// <summary>
/// Stores settings data that should persist after closing and reopening the game.
/// </summary>
[Serializable]
public class SettingsData
{
    public float MusicVolume = 1f;
    public float SoundVolume = 1f;
    public bool IsFullscreen = true;
    public bool ShowControlReminders = true;
}
