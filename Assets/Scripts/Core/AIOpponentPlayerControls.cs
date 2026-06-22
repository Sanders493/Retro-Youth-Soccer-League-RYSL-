using UnityEngine;

/// <summary>
/// Controls an AI soccer player that can chase the ball, return to formation, and shoot toward the opponent goal.
/// </summary>
public class AIOpponentPlayer : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private Transform homePosition;
    [SerializeField] private Transform opponentGoal;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float kickRange = 1.2f;
    [SerializeField] private float shootPower = 8f;
    [SerializeField] private float returnDistance = 0.2f;

    private Rigidbody2D playerRigidbody;
    private SoccerBall soccerBall;

    public bool IsChasingBall
    {
        get; private set;
    }

    /// <summary>
    /// Gets required Rigidbody2D and SoccerBall references.
    /// </summary>
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (ball != null)
        {
            soccerBall = ball.GetComponent<SoccerBall>();
        }
    }

    /// <summary>
    /// Updates the AI player's movement and decision making.
    /// </summary>
    private void FixedUpdate()
    {
        if (ball == null || homePosition == null) return;

        if (IsBallCloseEnoughToChase())
        {
            IsChasingBall = true;
            MoveToward(ball.position);
            TryShootBall();
        }
        else
        {
            IsChasingBall = false;
            ReturnToHomePosition();
        }
    }

    /// <summary>
    /// Checks if the ball is close enough for the AI player to chase.
    /// </summary>
    /// <returns>True if the ball is inside chase range.</returns>
    private bool IsBallCloseEnoughToChase()
    {
        return Vector2.Distance(transform.position, ball.position) <= chaseRange;
    }

    /// <summary>
    /// Moves the AI player toward a target position.
    /// </summary>
    /// <param name="targetPosition">The position the AI should move toward.</param>
    private void MoveToward(Vector2 targetPosition)
    {
        Vector2 newPosition = Vector2.MoveTowards(
            playerRigidbody.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        playerRigidbody.MovePosition(newPosition);
    }

    /// <summary>
    /// Moves the AI player back to its assigned home formation position.
    /// </summary>
    private void ReturnToHomePosition()
    {
        if (Vector2.Distance(transform.position, homePosition.position) <= returnDistance) return;

        MoveToward(homePosition.position);
    }

    /// <summary>
    /// Attempts to shoot the ball toward the opponent goal if the ball is close enough.
    /// </summary>
    private void TryShootBall()
    {
        if (soccerBall == null || opponentGoal == null) return;

        if (Vector2.Distance(transform.position, ball.position) > kickRange) return;

        Vector2 shootDirection = opponentGoal.position - transform.position;
        soccerBall.Kick(shootDirection, shootPower);
    }
}
