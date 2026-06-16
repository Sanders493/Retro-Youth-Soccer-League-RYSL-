using UnityEngine;

/// <summary>
/// Controls an AI goalkeeper that guards the goal, reacts to the ball, dives, and throws the ball back into play.
/// </summary>
public class AIGoalkeeper : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private Transform goalCenter;
    [SerializeField] private Transform throwTarget;

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float diveSpeed = 8f;
    [SerializeField] private float catchRange = 1f;
    [SerializeField] private float penaltyBoxRange = 5f;
    [SerializeField] private float throwPower = 7f;
    [SerializeField] private float maxGoalieMoveY = 2f;

    private Rigidbody2D goalkeeperRigidbody;
    private Rigidbody2D ballRigidbody;
    private Vector2 startingPosition;
    private bool hasBall;

    public bool HasBall
    {
        get; private set;
    }

    /// <summary>
    /// Gets required Rigidbody2D components and stores the goalkeeper starting position.
    /// </summary>
    private void Awake()
    {
        goalkeeperRigidbody = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;

        if (ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Updates goalkeeper behavior based on the ball location.
    /// </summary>
    private void FixedUpdate()
    {
        if (ball == null || goalCenter == null || ballRigidbody == null) return;

        if (hasBall)
        {
            HoldBall();
            return;
        }

        if (IsBallInPenaltyBox())
        {
            MoveTowardBall();

            if (IsBallCloseEnoughToCatch())
            {
                CatchBall();
            }
        }
        else
        {
            ReturnToGoalCenter();
        }
    }

    /// <summary>
    /// Checks if the ball has entered the goalkeeper penalty box area.
    /// </summary>
    /// <returns>True if the ball is close enough to the goal area.</returns>
    private bool IsBallInPenaltyBox()
    {
        return Vector2.Distance(ball.position, goalCenter.position) <= penaltyBoxRange;
    }

    /// <summary>
    /// Moves the goalkeeper toward the ball while staying near the goal area.
    /// </summary>
    private void MoveTowardBall()
    {
        Vector2 targetPosition = ball.position;
        targetPosition.x = transform.position.x;
        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            startingPosition.y - maxGoalieMoveY,
            startingPosition.y + maxGoalieMoveY
        );

        Vector2 newPosition = Vector2.MoveTowards(
            goalkeeperRigidbody.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        goalkeeperRigidbody.MovePosition(newPosition);
    }

    /// <summary>
    /// Makes the goalkeeper quickly dive toward the ball.
    /// </summary>
    public void DiveTowardBall()
    {
        if (ball == null) return;

        Vector2 diveDirection = ball.position - transform.position;
        goalkeeperRigidbody.AddForce(diveDirection.normalized * diveSpeed, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Checks if the ball is close enough for the goalkeeper to catch.
    /// </summary>
    /// <returns>True if the ball is inside catch range.</returns>
    private bool IsBallCloseEnoughToCatch()
    {
        return Vector2.Distance(transform.position, ball.position) <= catchRange;
    }

    /// <summary>
    /// Catches the ball and stops its movement.
    /// </summary>
    private void CatchBall()
    {
        hasBall = true;
        HasBall = true;

        ballRigidbody.velocity = Vector2.zero;
        ballRigidbody.angularVelocity = 0f;

        Invoke(nameof(ThrowBall), 1f);
    }

    /// <summary>
    /// Keeps the ball attached to the goalkeeper while preparing to throw.
    /// </summary>
    private void HoldBall()
    {
        ball.position = transform.position + new Vector3(0.5f, 0f, 0f);
    }

    /// <summary>
    /// Throws the ball toward a teammate or target point.
    /// </summary>
    private void ThrowBall()
    {
        if (throwTarget == null)
        {
            hasBall = false;
            HasBall = false;
            return;
        }

        Vector2 throwDirection = throwTarget.position - transform.position;

        hasBall = false;
        HasBall = false;

        ballRigidbody.AddForce(throwDirection.normalized * throwPower, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Moves the goalkeeper back to the center of the goal area.
    /// </summary>
    private void ReturnToGoalCenter()
    {
        Vector2 targetPosition = new Vector2(transform.position.x, startingPosition.y);

        Vector2 newPosition = Vector2.MoveTowards(
            goalkeeperRigidbody.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        goalkeeperRigidbody.MovePosition(newPosition);
    }
}
