using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the full gameplay loop and connects match, rewards, power-ups,
/// saving, settings, and end game systems.
/// </summary>
public class MainGameManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private GameState gameState;

    [Header("Managers")]
    [Tooltip("Assign a component that implements IMatchManager.")]
    [SerializeField] private MonoBehaviour matchManagerSource;
    [SerializeField] private MatchRewardSystem matchRewardSystem;
    [SerializeField] private PowerUpSystem powerUpSystem;
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField]
    private AudioTextMessageManager audioTextMessageManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endGamePanel;

    [Header("UI Text")]
    [SerializeField] private TMP_Text gameStateText;
    [SerializeField] private TMP_Text finalResultText;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string teamSelectionSceneName = "TeamSelection";

    private IMatchManager matchManager;

    public bool IsGamePaused
    {
        get;
        private set;
    }

    /// <summary>
    /// Resolves manager interfaces before gameplay begins.
    /// </summary>
    private void Awake()
    {
        matchManager =
            matchManagerSource as IMatchManager;

        if (matchManagerSource != null
            && matchManager == null)
        {
            Debug.LogError(
                $"{matchManagerSource.name} does not implement "
                + $"{nameof(IMatchManager)}.",
                this);
        }
    }

    /// <summary>
    /// Sets up the match when the gameplay scene begins.
    /// </summary>
    private void Start()
    {
        SetupGameplayScene();
    }

    /// <summary>
    /// Checks whether the active match has ended.
    /// </summary>
    private void Update()
    {
        if (gameState == null
            || !gameState.IsMatchActive
            || matchManager == null)
        {
            return;
        }

        if (matchManager.IsMatchOver)
        {
            EndMatch();
        }
    }

    /// <summary>
    /// Prepares UI, save data, settings, match systems, and starts the match.
    /// </summary>
    private void SetupGameplayScene()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }

        if (saveSystem != null)
        {
            saveSystem.LoadGame();
        }

        if (settingsManager != null)
        {
            settingsManager.LoadSettings();
        }

        if (matchManager != null)
        {
            matchManager.ResetMatch();
        }

        StartMatch();
    }

    /// <summary>
    /// Starts the active soccer match.
    /// </summary>
    public void StartMatch()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;

        if (gameState != null)
        {
            gameState.StartMatch();
        }

        if (matchManager != null)
        {
            matchManager.StartMatchTimer();
        }
    }

    /// <summary>
    /// Adds a goal to the player's score and updates reward tracking.
    /// </summary>
    public void AddPlayerGoal()
    {
        if (gameState == null
            || !gameState.IsMatchActive)
        {
            return;
        }

        if (matchManager != null)
        {
            matchManager.AddPlayerGoal();
        }

        if (matchRewardSystem != null)
        {
            matchRewardSystem.AddPlayerGoal();
        }

        if (audioTextMessageManager != null)
        {
            audioTextMessageManager.ShowGoalMessage();
        }
    }

    /// <summary>
    /// Adds a goal to the opponent's score.
    /// </summary>
    public void AddOpponentGoal()
    {
        if (gameState == null
            || !gameState.IsMatchActive)
        {
            return;
        }

        if (matchManager != null)
        {
            matchManager.AddOpponentGoal();
        }
    }

    /// <summary>
    /// Pauses the match and opens the pause panel.
    /// </summary>
    public void PauseGame()
    {
        if (gameState == null
            || gameState.CurrentMatchState != EMatchState.Playing)
        {
            return;
        }

        gameState.PauseMatch();
        IsGamePaused = true;

        if (matchManager != null)
        {
            matchManager.PauseMatchTimer();
        }

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Resumes the match from the paused state.
    /// </summary>
    public void ResumeGame()
    {
        if (gameState == null
            || gameState.CurrentMatchState != EMatchState.Paused)
        {
            return;
        }

        IsGamePaused = false;
        Time.timeScale = 1f;

        gameState.ResumeMatch();

        if (matchManager != null)
        {
            matchManager.ResumeMatchTimer();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Ends the match, gives rewards, and displays the end game screen.
    /// </summary>
    public void EndMatch()
    {
        if (gameState != null
            && gameState.CurrentMatchState == EMatchState.Ended)
        {
            return;
        }

        IsGamePaused = false;
        Time.timeScale = 1f;

        if (gameState != null)
        {
            gameState.EndMatch();
        }

        if (matchManager != null)
        {
            matchManager.PauseMatchTimer();
        }

        if (matchRewardSystem != null)
        {
            matchRewardSystem.GiveEndMatchRewards();
        }

        ShowFinalResult();

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Shows whether the player won, lost, or tied the match.
    /// </summary>
    private void ShowFinalResult()
    {
        if (matchManager == null)
        {
            return;
        }

        if (matchManager.PlayerScore
            > matchManager.OpponentScore)
        {
            if (finalResultText != null)
            {
                finalResultText.text = "You win!";
            }

            if (audioTextMessageManager != null)
            {
                audioTextMessageManager.ShowWinMessage();
            }
        }
        else if (matchManager.PlayerScore
                 < matchManager.OpponentScore)
        {
            if (finalResultText != null)
            {
                finalResultText.text = "You lose!";
            }

            if (audioTextMessageManager != null)
            {
                audioTextMessageManager.ShowLoseMessage();
            }
        }
        else
        {
            if (finalResultText != null)
            {
                finalResultText.text = "Tie game!";
            }

            if (audioTextMessageManager != null)
            {
                audioTextMessageManager
                    .ShowKeepPracticingMessage();
            }
        }
    }

    /// <summary>
    /// Restarts the current match scene.
    /// </summary>
    public void RestartMatch()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Loads the team selection scene.
    /// </summary>
    public void ReturnToTeamSelection()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            teamSelectionSceneName);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainMenuSceneName);
    }
}