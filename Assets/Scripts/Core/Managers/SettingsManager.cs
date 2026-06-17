using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages player settings, saves them to a JSON file, loads them, and applies them to the game.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private string settingsFileName = "settings_data.json";

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle controlRemindersToggle;
    [SerializeField] private TMP_Text controlReminderText;

    private SettingsData settingsData;

    public SettingsData CurrentSettings
    {
        get; private set;
    }

    private string SettingsFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, settingsFileName);
        }
    }

    /// <summary>
    /// Loads saved settings and applies them when the scene starts.
    /// </summary>
    private void Awake()
    {
        LoadSettings();
        UpdateSettingsUI();
        ApplySettings();
    }

    /// <summary>
    /// Changes and saves the music volume setting.
    /// </summary>
    /// <param name="volume">The music volume value from 0 to 1.</param>
    public void SetMusicVolume(float volume)
    {
        settingsData.MusicVolume = volume;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes and saves the sound effects volume setting.
    /// </summary>
    /// <param name="volume">The sound effects volume value from 0 to 1.</param>
    public void SetSoundVolume(float volume)
    {
        settingsData.SoundVolume = volume;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes and saves the fullscreen setting.
    /// </summary>
    /// <param name="isFullscreen">Whether the game should be fullscreen.</param>
    public void SetFullscreen(bool isFullscreen)
    {
        settingsData.IsFullscreen = isFullscreen;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes and saves whether control reminders are visible.
    /// </summary>
    /// <param name="showReminders">Whether control reminders should be shown.</param>
    public void SetControlReminders(bool showReminders)
    {
        settingsData.ShowControlReminders = showReminders;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Saves the current settings data to a JSON file.
    /// </summary>
    public void SaveSettings()
    {
        CurrentSettings = settingsData;

        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// Loads settings from a JSON file, or creates default settings if no file exists.
    /// </summary>
    public void LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            settingsData = new SettingsData();
            CurrentSettings = settingsData;
            SaveSettings();
            return;
        }

        string json = File.ReadAllText(SettingsFilePath);
        settingsData = JsonUtility.FromJson<SettingsData>(json);
        CurrentSettings = settingsData;
    }

    /// <summary>
    /// Applies the loaded settings to the game.
    /// </summary>
    private void ApplySettings()
    {
        Screen.fullScreen = settingsData.IsFullscreen;

        AudioListener.volume = settingsData.MusicVolume;

        if (controlReminderText != null)
        {
            controlReminderText.gameObject.SetActive(settingsData.ShowControlReminders);

            if (settingsData.ShowControlReminders)
            {
                controlReminderText.text = "Move: WASD / Arrow Keys | Pass: J | Shoot: K";
            }
        }
    }

    /// <summary>
    /// Updates the settings menu UI to match the loaded settings.
    /// </summary>
    private void UpdateSettingsUI()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = settingsData.MusicVolume;
        }

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.value = settingsData.SoundVolume;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = settingsData.IsFullscreen;
        }

        if (controlRemindersToggle != null)
        {
            controlRemindersToggle.isOn = settingsData.ShowControlReminders;
        }
    }
}
