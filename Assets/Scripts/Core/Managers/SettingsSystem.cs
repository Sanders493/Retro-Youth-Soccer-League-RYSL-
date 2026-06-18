using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages game settings, saves settings, loads settings, and updates control reminders.
/// </summary>
public class SettingsSystem : MonoBehaviour
{
    [SerializeField] private string settingsFileName = "settings_data.json";

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle controlRemindersToggle;
    [SerializeField] private TMP_Text controlReminderText;

    private SettingsData settingsData;

    private string SettingsFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, settingsFileName);
        }
    }

    /// <summary>
    /// Loads settings and updates the settings menu when the scene starts.
    /// </summary>
    private void Awake()
    {
        LoadSettings();
        UpdateSettingsUI();
        ApplySettings();
    }

    /// <summary>
    /// Changes the music volume setting and saves it.
    /// </summary>
    /// <param name="volume">The new music volume value.</param>
    public void SetMusicVolume(float volume)
    {
        settingsData.MusicVolume = volume;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes the sound volume setting and saves it.
    /// </summary>
    /// <param name="volume">The new sound volume value.</param>
    public void SetSoundVolume(float volume)
    {
        settingsData.SoundVolume = volume;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes the fullscreen setting and saves it.
    /// </summary>
    /// <param name="isFullscreen">Whether fullscreen should be enabled.</param>
    public void SetFullscreen(bool isFullscreen)
    {
        settingsData.IsFullscreen = isFullscreen;
        SaveSettings();
        ApplySettings();
    }

    /// <summary>
    /// Changes whether control reminders are shown and saves it.
    /// </summary>
    /// <param name="showReminders">Whether control reminders should be visible.</param>
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
        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// Loads settings from a JSON file or creates default settings if none exist.
    /// </summary>
    public void LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            settingsData = new SettingsData();
            SaveSettings();
            return;
        }

        string json = File.ReadAllText(SettingsFilePath);
        settingsData = JsonUtility.FromJson<SettingsData>(json);
    }

    /// <summary>
    /// Applies settings to the game.
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
    /// Updates the settings menu UI to match the loaded settings data.
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
