using UnityEngine;

/// <summary>
/// Detects when the soccer ball enters a goal trigger, updates the score, and resets the ball.
/// </summary>
public class GoalDetectionSystem : MonoBehaviour
{
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private Transform ballResetPoint;
    [SerializeField] private bool isPlayerGoal;

    /// <summary>
    /// Detects when another collider enters the goal trigger.
    /// </summary>
    /// <param name="collision">The collider that entered the goal trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ball")) return;

        if (isPlayerGoal)
        {
            mainGameManager.AddOpponentGoal();
        }
        else
        {
            mainGameManager.AddPlayerGoal();
        }

        ResetBall(collision);
    }

    /// <summary>
    /// Resets the ball to the assigned reset point and stops its movement.
    /// </summary>
    /// <param name="ballCollider">The ball collider that entered the goal.</param>
    private void ResetBall(Collider2D ballCollider)
    {
        if (ballResetPoint != null)
        {
            ballCollider.transform.position = ballResetPoint.position;
        }

        Rigidbody2D ballRigidbody = ballCollider.GetComponent<Rigidbody2D>();

        if (ballRigidbody != null)
        {
            ballRigidbody.velocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }
}
