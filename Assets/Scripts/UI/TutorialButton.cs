using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles opening, closing, and navigating tutorial-related UI buttons.
/// </summary>
public class TutorialButton : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private string practiceSceneName = "PracticeMode";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsTutorialOpen
    {
        get; private set;
    }

    /// <summary>
    /// Opens the tutorial panel and pauses tutorial state.
    /// </summary>
    public void OpenTutorial()
    {
        if (tutorialPanel == null) return;

        tutorialPanel.SetActive(true);
        IsTutorialOpen = true;
    }

    /// <summary>
    /// Closes the tutorial panel and updates tutorial state.
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialPanel == null) return;

        tutorialPanel.SetActive(false);
        IsTutorialOpen = false;
    }

    /// <summary>
    /// Toggles the tutorial panel on or off.
    /// </summary>
    public void ToggleTutorial()
    {
        if (IsTutorialOpen)
        {
            CloseTutorial();
        }
        else
        {
            OpenTutorial();
        }
    }

    /// <summary>
    /// Loads the practice mode scene.
    /// </summary>
    public void LoadPracticeMode()
    {
        SceneManager.LoadScene(practiceSceneName);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
