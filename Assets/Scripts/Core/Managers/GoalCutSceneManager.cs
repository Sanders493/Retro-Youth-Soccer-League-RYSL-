using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private Rigidbody2D ballRb;
    public bool CutsceneActive { get; private set; }

    private void Awake()
    {
        if (matchManager == null)
            matchManager = FindFirstObjectByType<MatchManager>();

        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody2D>();

        if (ballRb == null)
            Debug.LogWarning("GoalCutsceneManager: Ball Rigidbody2D is missing. Assign the Ball Transform that has Rigidbody2D.");

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
    }

    public void PlayGoalCutscene(bool playerScored)
    {
        if (CutsceneActive) return;
        StartCoroutine(GoalCutsceneRoutine(playerScored));
    }

    private IEnumerator GoalCutsceneRoutine(bool playerScored)
    {
        CutsceneActive = true;

        PauseMatchObjects();
        ShowCutsceneUI(playerScored);

        yield return new WaitForSecondsRealtime(cutsceneDuration);

        ResetBallToCenter();
        HideCutsceneUI();
        ResumeMatchObjects();

        CutsceneActive = false;
    }

    private void PauseMatchObjects()
    {
        Time.timeScale = 0f;
        StopBall();
    }

    private void ResumeMatchObjects()
    {
        Time.timeScale = 1f;
    }

    private void ShowCutsceneUI(bool playerScored)
    {
        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        if (goalMessageText != null)
            goalMessageText.text = playerScored ? "Goal! Great teamwork!" : "Opponent scored! Keep practicing!";

        if (scoreText != null && matchManager != null)
            scoreText.text = $"Score: {matchManager.PlayerScore} - {matchManager.OpponentScore}";

        if (timerText != null && matchManager != null)
            timerText.text = $"Time: {matchManager.GetFormattedTime()}";
    }

    private void HideCutsceneUI()
    {
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
    }

    private void ResetBallToCenter()
    {
        if (ball != null && ballResetPoint != null)
            ball.position = ballResetPoint.position;

        StopBall();
    }

    private void StopBall()
    {
        if (ballRb == null) return;

#if UNITY_6000_0_OR_NEWER
        ballRb.linearVelocity = Vector2.zero;
#else
        ballRb.velocity = Vector2.zero;
#endif

        ballRb.angularVelocity = 0f;
    }
}
