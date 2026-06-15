using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles opening, closing, and navigating credits-related UI buttons.
/// </summary>
public class CreditsButton : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsCreditsOpen
    {
        get; private set;
    }

    /// <summary>
    /// Initializes the credits panel state when the scene starts.
    /// </summary>
    private void Start()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }

        IsCreditsOpen = false;
    }


    /// <summary>
    /// Opens the credits panel.
    /// </summary>
    public void OpenCredits()
    {
        if (creditsPanel == null) return;

        creditsPanel.SetActive(true);
        IsCreditsOpen = true;
    }

    /// <summary>
    /// Closes the credits panel.
    /// </summary>
    public void CloseCredits()
    {
        if (creditsPanel == null) return;

        creditsPanel.SetActive(false);
        IsCreditsOpen = false;
    }

    /// <summary>
    /// Returns the player to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}