using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles end game screen button actions.
/// </summary>
public class EndGameButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string replaySceneName = "MatchScene";

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Restarts the match scene.
    /// </summary>
    public void ReplayMatch()
    {
        SceneManager.LoadScene(replaySceneName);
    }

    /// <summary>
    /// Quits the game application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
