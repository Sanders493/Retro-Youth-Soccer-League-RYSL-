using UnityEngine;

/// <summary>
/// Controls an AI opponent soccer player by chasing the ball,
/// returning to formation, and shooting toward the opponent goal.
/// </summary>
public class AIOpponentPlayerControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ball;
    [SerializeField] private Transform homePosition;
    [SerializeField] private Transform opponentGoal;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float returnDistance = 0.2f;

    [Header("Ball Interaction")]
    [SerializeField] private float kickRange = 1.2f;
    [SerializeField] private float shootPower = 8f;

    private Rigidbody2D rigidbodyComponent;
    private SoccerBall soccerBall;

    /// <summary>
    /// Gets required component references.
    /// </summary>
    private void Awake()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();

        if (ball != null)
        {
            soccerBall = ball.GetComponent<SoccerBall>();
        }
    }

    /// <summary>
    /// Updates AI behavior each physics frame.
    /// </summary>
    private void FixedUpdate()
    {
        if (ball == null || homePosition == null)
        {
            return;
        }

        float distanceToBall =
            Vector2.Distance(
                transform.position,
                ball.position);

        if (distanceToBall <= chaseRange)
        {
            ChaseBall();

            if (distanceToBall <= kickRange)
            {
                ShootBall();
            }
        }
        else
        {
            ReturnToFormation();
        }
    }

    /// <summary>
    /// Moves the AI player toward the ball.
    /// </summary>
    private void ChaseBall()
    {
        Vector2 targetPosition =
            Vector2.MoveTowards(
                rigidbodyComponent.position,
                ball.position,
                moveSpeed * Time.fixedDeltaTime);

        rigidbodyComponent.MovePosition(targetPosition);
    }

    /// <summary>
    /// Returns the AI player to its assigned formation position.
    /// </summary>
    private void ReturnToFormation()
    {
        float distanceToHome =
            Vector2.Distance(
                transform.position,
                homePosition.position);

        if (distanceToHome <= returnDistance)
        {
            return;
        }

        Vector2 targetPosition =
            Vector2.MoveTowards(
                rigidbodyComponent.position,
                homePosition.position,
                moveSpeed * Time.fixedDeltaTime);

        rigidbodyComponent.MovePosition(targetPosition);
    }

    /// <summary>
    /// Shoots the ball toward the opponent goal.
    /// </summary>
    private void ShootBall()
    {
        if (soccerBall == null || opponentGoal == null)
        {
            return;
        }

        Vector2 shootDirection =
            (opponentGoal.position - transform.position).normalized;

        // FIXED: Added gameObject as sender
        soccerBall.Kick(
            shootDirection,
            shootPower,
            gameObject);
    }
}
