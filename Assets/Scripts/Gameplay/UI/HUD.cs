using UnityEngine;
using UnityEngine.UI; 

/// <summary>
/// Displays timer, team names, score, active player indicator, and simple controls reminder. Controls reminder is an image. 
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private TeamSelectionManager teamSelectionManager;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text friendlyTeamName;
    [SerializeField] private TMP_Text enemyTeamName = "Opponent";
    [SerializeField] private string homeScore;
    [SerializeField] private string awayScore;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text activePlayerIndicator = "Player";
    [SerializeField] private Image ControlsReminder; 
    
    /// <summary>
    /// Sets scores to 0, sets the team name with the team selection manager
    /// </summary>
    private void Start() {
        homeScore = 0;
        awayScore = 0;

        if (teamSelectionManager == null) {
            Debug.LogWarning("TeamSelectionManager is missing.");
            return;
        }

        friendlyTeamName = teamSelectionManager.SelectedTeamName;
    }

    /// <summary>
    /// Calls functions to update timer, score, and active player
    /// </summary>
    private void Update() {
        UpdateTimer();
        UpdateScore();
        UpdateActivePlayerIndicator();
    }

    /// <summary>
    /// Updates the match timer 
    /// </summary>
    private void UpdateTimer() {
        if (MatchManager.Instance == null) {
            Debug.LogWarning("MatchManager not found.");
            return;
        }

        TimeSpan timeRemaining = CountdownClock.Instance.TimeRemaining;
        timerText.text = timeRemaining.ToString(@"mm\:ss");
    }

    /// <summary>
    /// Updates the scores
    /// </summary>
    private void UpdateScore() {
        if (MatchManager.Instance == null) {
            Debug.LogWarning("MatchManager not found.");
            return;
        }

        homeScore = MatchManager.Instance.PlayerScore;
        awayScore = MatchManager.Instance.OpponentScore;
        scoreText.text = $"{homeScore} - {awayScore}";
    }

    /// <summary>
    /// Updates the position of the active player indicator. The indicator is a TMP text string positioned above the sprite the player is currently controlling. 
    /// </summary>
    private void UpdateActivePlayerIndicator() {
        
    }

}
