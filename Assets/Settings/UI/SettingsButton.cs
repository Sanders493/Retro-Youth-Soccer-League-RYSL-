using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles settings menu button functionality.
/// </summary>
public class SettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsSettingsOpen
    {
        get; private set;
    }

    /// <summary>
    /// Opens the settings panel.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel is not assigned.");
            return;
        }

        settingsPanel.SetActive(true);
        IsSettingsOpen = true;
    }

    /// <summary>
    /// Closes the settings panel.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel is not assigned.");
            return;
        }

        settingsPanel.SetActive(false);
        IsSettingsOpen = false;
    }

    /// <summary>
    /// Toggles the settings panel on or off.
    /// </summary>
    public void ToggleSettings()
    {
        if (IsSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    /// <summary>
    /// Returns the player to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }
}
