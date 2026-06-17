using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerKickController : MonoBehaviour
{
    [SerializeField] private SoccerBall ball;
    [SerializeField] private Transform opponentGoal;
    [SerializeField] private Transform[] teammates;

    [SerializeField] private float kickRange = 1.5f;
    [SerializeField] private float passPower = 6f;
    [SerializeField] private float shootPower = 10f;

    private PlayerInputReader inputReader;

    /// <summary>
    /// Gets required player movement component.
    /// </summary>
    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    /// <summary>
    /// Checks for pass and shoot input.
    /// </summary>
    private void Update()
    {
        if (inputReader.ActionInput == EActionInputType.PassPressed)
        {
            PassBall();
        }

        if (inputReader.ActionInput == EActionInputType.ShootPressed)
        {
            ShootBall();
        }
    }

    /// <summary>
    /// Passes the ball to the nearest teammate or aimed direction.
    /// </summary>
    private void PassBall()
    {
        if (!IsBallClose()) return;

        Transform nearestTeammate = GetNearestTeammate();
        Vector2 direction = nearestTeammate != null
            ? nearestTeammate.position - transform.position
            : inputReader.MovementInput;

        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }

        ball.Kick(direction, passPower);
    }

    /// <summary>
    /// Shoots the ball toward the opponent goal.
    /// </summary>
    private void ShootBall()
    {
        if (!IsBallClose()) return;

        Vector2 direction = opponentGoal.position - transform.position;
        ball.Kick(direction, shootPower);
    }

    /// <summary>
    /// Checks if the ball is close enough for the player to kick.
    /// </summary>
    /// <returns>True if the ball is within kick range.</returns>
    private bool IsBallClose()
    {
        return Vector2.Distance(transform.position, ball.transform.position) <= kickRange;
    }

    /// <summary>
    /// Finds the nearest teammate to pass the ball to.
    /// </summary>
    /// <returns>The closest teammate transform.</returns>
    private Transform GetNearestTeammate()
    {
        Transform nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Transform teammate in teammates)
        {
            if (teammate == null) continue;

            float distance = Vector2.Distance(transform.position, teammate.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = teammate;
            }
        }

        return nearest;
    }
}
