using UnityEngine;

/// <summary>
/// Detects when the soccer ball enters a goal trigger and manages the goal event.
/// Updates the score through the MainGameManager, starts the GoalCutsceneManager,
/// pauses gameplay temporarily, displays the current score and timer,
/// resets the ball to center field, and resumes the match after the cutscene.
/// </summary>
public class GoalDetectionSystem : MonoBehaviour
{
    [SerializeField] private MainGameManager mainGameManager;

    [SerializeField] private GoalCutsceneManager goalCutsceneManager;

    [SerializeField]
    [Tooltip("True if this goal belongs to the player team.")]
    private bool isPlayerGoal;

    /// <summary>
    /// Detects when the soccer ball enters the goal trigger.
    /// </summary>
    /// <param name="collision">The collider entering the goal trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ball"))
        {
            return;
        }

        bool playerScored = !isPlayerGoal;

        UpdateScore(playerScored);

        StartGoalCutscene(playerScored);
    }

    /// <summary>
    /// Updates the score depending on which team scored.
    /// </summary>
    /// <param name="playerScored">
    /// True if the player team scored.
    /// False if the opponent team scored.
    /// </param>
    private void UpdateScore(bool playerScored)
    {
        if (mainGameManager == null)
        {
            Debug.LogWarning("MainGameManager is missing.");
            return;
        }

        if (playerScored)
        {
            mainGameManager.AddPlayerGoal();
        }
        else
        {
            mainGameManager.AddOpponentGoal();
        }
    }

    /// <summary>
    /// Starts the goal cutscene after the score is updated.
    /// </summary>
    /// <param name="playerScored">
    /// True if the player team scored.
    /// False if the opponent team scored.
    /// </param>
    private void StartGoalCutscene(bool playerScored)
    {
        if (goalCutsceneManager == null)
        {
            Debug.LogWarning("GoalCutsceneManager is missing.");
            return;
        }

        goalCutsceneManager.PlayGoalCutscene(playerScored);
    }
}
