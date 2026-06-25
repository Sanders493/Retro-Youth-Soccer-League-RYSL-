using System;
using UnityEngine;

/// <summary>
/// Manages match time, scores, and match length options
/// </summary>
public class MatchManager : MonoBehaviour, IMatchManager
{
    public static MatchManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CountdownClock clock;

    public bool IsMatchOver { get; private set; }
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }

    public TimeSpan TimeRemaining => clock.TimeRemaining;
    private float matchLengthMinutes = 8f;

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

    private void OnEnable()
    {
        clock.onCountdownFinished += EndMatch;
    }

    private void OnDisable()
    {
        if (CountdownClock.Instance != null)
        {
            clock.onCountdownFinished -= EndMatch;
        }
    }
    public void ResetMatch()
    {
        this.IsMatchOver = false;

        this.PlayerScore = 0;
        this.OpponentScore = 0;
    }

    public void StartMatchTimer()
    {
        clock.BeginCountdown(matchLengthMinutes);
    }

    public void PauseMatchTimer()
    {
        clock.PauseCountdown();
    }

    public void ResumeMatchTimer()
    {
        clock.ResumeCountdown();
    }

    public void AddPlayerGoal()
    {
        /* Todo: Goal scored
                ↓
                Pause timer
                ↓
                Freeze players
                ↓
                Wait 2–3 seconds
                ↓
                Reset kickoff positions
                ↓
                Resume timer
        */
        if (IsMatchOver)
            return;

        PlayerScore++;
        PauseMatchTimer();
        new WaitForSeconds(3);
        ResumeMatchTimer();
    }

    public void AddOpponentGoal()
    {
        /* Todo: Goal scored
                ↓
                Pause timer
                ↓
                Freeze players
                ↓
                Wait 2–3 seconds
                ↓
                Reset kickoff positions
                ↓
                Resume timer
        */
        if (IsMatchOver)
            return;
        
        OpponentScore++;
        PauseMatchTimer();
        new WaitForSeconds(3);
        ResumeMatchTimer();
    }

    public void EndMatch()
    {
        this.IsMatchOver = true;
    }
}