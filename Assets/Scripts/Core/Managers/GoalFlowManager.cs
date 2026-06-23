using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the full goal flow after the ball enters a goal,
/// including score updates, messages, ball reset, player reset, and kickoff delay.
/// </summary>
public class GoalFlowManager : MonoBehaviour
{
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private Transform ball;
    [SerializeField] private Transform ballResetPoint;
    [SerializeField] private Rigidbody2D ballRigidbody;

    [SerializeField] private Transform[] playersToReset;
    [SerializeField] private Transform[] playerResetPoints;

    [SerializeField] private TMP_Text goalMessageText;
    [SerializeField] private float goalPauseDuration = 2f;

    private bool goalFlowActive;

    public bool GoalFlowActive
    {
        get; private set;
    }

    /// <summary>
    /// Starts the goal flow for the player or opponent after a goal is detected.
    /// </summary>
    /// <param name="playerScored">True if the player team scored, false if the opponent scored.</param>
    public void StartGoalFlow(bool playerScored)
    {
        if (goalFlowActive) return;

        StartCoroutine(GoalFlowRoutine(playerScored));
    }

    /// <summary>
    /// Runs the full goal sequence after a goal is scored.
    /// </summary>
    /// <param name="playerScored">True if the player team scored, false if the opponent scored.</param>
    /// <returns>Waits during the goal pause before restarting play.</returns>
    private IEnumerator GoalFlowRoutine(bool playerScored)
    {
        goalFlowActive = true;
        GoalFlowActive = true;

        if (playerScored)
        {
            mainGameManager.AddPlayerGoal();
            ShowGoalMessage("Goal! Great teamwork!");
        }
        else
        {
            mainGameManager.AddOpponentGoal();
            ShowGoalMessage("Opponent scored! Keep practicing!");
        }

        StopBall();

        yield return new WaitForSeconds(goalPauseDuration);

        ResetBall();
        ResetPlayers();
        ClearGoalMessage();

        goalFlowActive = false;
        GoalFlowActive = false;
    }

    /// <summary>
    /// Stops the ball movement after a goal is scored.
    /// </summary>
    private void StopBall()
    {
        if (ballRigidbody == null && ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Resets the ball back to the center or assigned reset point.
    /// </summary>
    private void ResetBall()
    {
        if (ball == null || ballResetPoint == null) return;

        ball.position = ballResetPoint.position;
        StopBall();
    }

    /// <summary>
    /// Resets players back to their assigned reset positions after a goal.
    /// </summary>
    private void ResetPlayers()
    {
        if (playersToReset == null || playerResetPoints == null) return;

        int resetCount = Mathf.Min(playersToReset.Length, playerResetPoints.Length);

        for (int i = 0; i < resetCount; i++)
        {
            if (playersToReset[i] == null || playerResetPoints[i] == null) continue;

            playersToReset[i].position = playerResetPoints[i].position;

            Rigidbody2D playerRigidbody = playersToReset[i].GetComponent<Rigidbody2D>();

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
            }
        }
    }

    /// <summary>
    /// Displays the goal message text.
    /// </summary>
    /// <param name="message">The message shown after a goal is scored.</param>
    private void ShowGoalMessage(string message)
    {
        if (goalMessageText != null)
        {
            goalMessageText.text = message;
        }
    }

    /// <summary>
    /// Clears the goal message text.
    /// </summary>
    private void ClearGoalMessage()
    {
        if (goalMessageText != null)
        {
            goalMessageText.text = "";
        }
    }
}
