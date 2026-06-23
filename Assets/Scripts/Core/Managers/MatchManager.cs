using System;
using UnityEngine;

/// <summary>
/// Manages match time, scores, and match length options.
/// Provides score updates and formatted timer information for gameplay UI and goal cutscenes.
/// </summary>
public class MatchManager : MonoBehaviour, IMatchManager
{
    public static MatchManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CountdownClock clock;

    [Header("Match Settings")]
    [SerializeField] private float matchLengthMinutes = 8f;

    public bool IsMatchOver { get; private set; }

    public int PlayerScore { get; private set; }

    public int OpponentScore { get; private set; }

    public TimeSpan TimeRemaining
    {
        get
        {
            if (clock == null)
            {
                return TimeSpan.Zero;
            }

            return clock.TimeRemaining;
        }
    }

    /// <summary>
    /// Sets up the MatchManager singleton instance.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Subscribes to the countdown finished event.
    /// </summary>
    private void OnEnable()
    {
        if (clock != null)
        {
            clock.onCountdownFinished += EndMatch;
        }
    }

    /// <summary>
    /// Unsubscribes from the countdown finished event.
    /// </summary>
    private void OnDisable()
    {
        if (clock != null)
        {
            clock.onCountdownFinished -= EndMatch;
        }
    }

    /// <summary>
    /// Resets the match score and match-over state.
    /// </summary>
    public void ResetMatch()
    {
        IsMatchOver = false;
        PlayerScore = 0;
        OpponentScore = 0;
    }

    /// <summary>
    /// Starts the match timer using the selected match length.
    /// </summary>
    public void StartMatchTimer()
    {
        if (clock == null)
        {
            Debug.LogWarning("CountdownClock is missing.");
            return;
        }

        clock.BeginCountdown(matchLengthMinutes);
    }

    /// <summary>
    /// Pauses the match timer.
    /// </summary>
    public void PauseMatchTimer()
    {
        if (clock == null)
        {
            return;
        }

        clock.PauseCountdown();
    }

    /// <summary>
    /// Resumes the match timer.
    /// </summary>
    public void ResumeMatchTimer()
    {
        if (clock == null || IsMatchOver)
        {
            return;
        }

        clock.ResumeCountdown();
    }

    /// <summary>
    /// Adds one goal to the player score.
    /// Goal pause, ball reset, and cutscene flow should be handled by GoalCutsceneManager.
    /// </summary>
    public void AddPlayerGoal()
    {
        if (IsMatchOver)
        {
            return;
        }

        PlayerScore++;
    }

    /// <summary>
    /// Adds one goal to the opponent score.
    /// Goal pause, ball reset, and cutscene flow should be handled by GoalCutsceneManager.
    /// </summary>
    public void AddOpponentGoal()
    {
        if (IsMatchOver)
        {
            return;
        }

        OpponentScore++;
    }

    /// <summary>
    /// Ends the match and prevents additional score updates.
    /// </summary>
    public void EndMatch()
    {
        IsMatchOver = true;
    }

    /// <summary>
    /// Gets the current match timer as formatted minute and second text.
    /// </summary>
    /// <returns>The formatted match timer as MM:SS.</returns>
    public string GetFormattedTime()
    {
        TimeSpan timeRemaining = TimeRemaining;

        int minutes = Mathf.Max(0, timeRemaining.Minutes);
        int seconds = Mathf.Max(0, timeRemaining.Seconds);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
