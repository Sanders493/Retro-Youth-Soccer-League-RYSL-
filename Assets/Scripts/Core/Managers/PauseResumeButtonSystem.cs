using UnityEngine;

/// <summary>
/// Handles pause menu button interactions for the soccer game.
/// </summary>
public class PauseGameButton : MonoBehaviour
{
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private GameObject pauseMenuPanel;

    public bool IsPauseMenuOpen
    {
        get; private set;
    }

    /// <summary>
    /// Pauses the match and opens the pause menu.
    /// </summary>
    public void PauseGame()
    {
        if (mainGameManager == null) return;

        mainGameManager.PauseGame();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        IsPauseMenuOpen = true;
    }

    /// <summary>
    /// Resumes the match and closes the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        if (mainGameManager == null) return;

        mainGameManager.ResumeGame();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        IsPauseMenuOpen = false;
    }

    /// <summary>
    /// Restarts the current match.
    /// </summary>
    public void RestartMatch()
    {
        if (mainGameManager == null) return;

        mainGameManager.RestartMatch();
    }

    /// <summary>
    /// Returns the player to the team selection screen.
    /// </summary>
    public void ReturnToTeamSelection()
    {
        if (mainGameManager == null) return;

        mainGameManager.ReturnToTeamSelection();
    }

    /// <summary>
    /// Returns the player to the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (mainGameManager == null) return;

        mainGameManager.ReturnToMainMenu();
    }

    /// <summary>
    /// Toggles the pause menu on or off.
    /// </summary>
    public void TogglePause()
    {
        if (IsPauseMenuOpen)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
}
