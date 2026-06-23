using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a short goal cutscene after a goal is scored,
/// pauses gameplay, displays score and timer, then resumes the match.
/// </summary>
public class GoalCutsceneManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private MatchManager matchManager;

    [Header("Ball Reset")]
    [SerializeField] private Transform ball;
    [SerializeField] private Transform ballResetPoint;

    [Header("Cutscene UI")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Image cutsceneBackground;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goalMessageText;

    [Header("Cutscene Settings")]
    [SerializeField] private float cutsceneDuration = 3f;

    private Rigidbody2D ballRigidbody;
    private bool cutsceneActive;

    public bool CutsceneActive
    {
        get; private set;
    }

    /// <summary>
    /// Gets the ball Rigidbody2D and hides the cutscene panel when the scene starts.
    /// </summary>
    private void Awake()
    {
        if (ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }

        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Starts the goal cutscene if one is not already active.
    /// </summary>
    /// <param name="playerScored">Whether the player team scored the goal.</param>
    public void PlayGoalCutscene(bool playerScored)
    {
        if (cutsceneActive) return;

        StartCoroutine(GoalCutsceneRoutine(playerScored));
    }

    /// <summary>
    /// Runs the full goal cutscene sequence.
    /// </summary>
    /// <param name="playerScored">Whether the player team scored the goal.</param>
    /// <returns>Waits during the cutscene before gameplay resumes.</returns>
    private IEnumerator GoalCutsceneRoutine(bool playerScored)
    {
        cutsceneActive = true;
        CutsceneActive = true;

        PauseMatchObjects();
        ShowCutsceneUI(playerScored);

        yield return new WaitForSecondsRealtime(cutsceneDuration);

        ResetBallToCenter();
        HideCutsceneUI();
        ResumeMatchObjects();

        cutsceneActive = false;
        CutsceneActive = false;
    }

    /// <summary>
    /// Pauses game time and stops the ball movement during the cutscene.
    /// </summary>
    private void PauseMatchObjects()
    {
        Time.timeScale = 0f;

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Resumes game time after the cutscene ends.
    /// </summary>
    private void ResumeMatchObjects()
    {
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Shows the cutscene panel with the current score and timer.
    /// </summary>
    /// <param name="playerScored">Whether the player team scored.</param>
    private void ShowCutsceneUI(bool playerScored)
    {
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(true);
        }

        if (goalMessageText != null)
        {
            goalMessageText.text = playerScored ? "Goal! Great teamwork!" : "Opponent scored! Keep practicing!";
        }

        if (scoreText != null && matchManager != null)
        {
            scoreText.text = "Score: " + matchManager.PlayerScore + " - " + matchManager.OpponentScore;
        }

        if (timerText != null && matchManager != null)
        {
            timerText.text = "Time: " + matchManager.GetFormattedTime();
        }
    }

    /// <summary>
    /// Hides the cutscene panel.
    /// </summary>
    private void HideCutsceneUI()
    {
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Resets the ball back to center field and stops movement.
    /// </summary>
    private void ResetBallToCenter()
    {
        if (ball != null && ballResetPoint != null)
        {
            ball.position = ballResetPoint.position;
        }

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }
}
