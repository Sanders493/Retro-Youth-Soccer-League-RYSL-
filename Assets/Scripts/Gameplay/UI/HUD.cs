using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System;

/// <summary>
/// Updates UI fields for timer, team names, score, and simple controls reminder.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private TeamSelectionManager teamSelectionManager;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text friendlyTeamName;
    [SerializeField] private TMP_Text enemyTeamName;
    [SerializeField] private int homeScore;
    [SerializeField] private int awayScore;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text controlsReminder; 
    
    /// <summary>
    /// Sets scores to 0, sets the team name with the team selection manager. Sets enemy team name and controls reminder if not already set
    /// </summary>
    private void Start() {
        homeScore = 0;
        awayScore = 0;

        if (teamSelectionManager == null) {
            Debug.LogWarning("TeamSelectionManager is missing.");
            return;
        }

        friendlyTeamName.text = teamSelectionManager.SelectedTeamName;

        if (string.IsNullOrEmpty(enemyTeamName.text)) {
            enemyTeamName.text = "Opponent";
        }

        if (string.IsNullOrEmpty(controlsReminder.text)) {
            controlsReminder.text = "Move - arrow keys\nSprint - shift\nPass - S\nShoot + Take ball - D\nSwitch player - A";
        }
    }

    /// <summary>
    /// Calls functions to update timer and score texts
    /// </summary>
    private void Update() {
        UpdateTimer();
        UpdateScore();
    }

    /// <summary>
    /// Updates the match timer text
    /// </summary>
    private void UpdateTimer() {
        if (MatchManager.Instance == null) {
            return;
        }

        TimeSpan timeRemaining = MatchManager.Instance.TimeRemaining;
        timerText.text = timeRemaining.ToString(@"mm\:ss");
    }

    /// <summary>
    /// Updates the score text
    /// </summary>
    private void UpdateScore() {
        if (MatchManager.Instance == null) {
            return;
        }

        homeScore = MatchManager.Instance.PlayerScore;
        awayScore = MatchManager.Instance.OpponentScore;
        scoreText.text = $"{homeScore} - {awayScore}";
    }
}
