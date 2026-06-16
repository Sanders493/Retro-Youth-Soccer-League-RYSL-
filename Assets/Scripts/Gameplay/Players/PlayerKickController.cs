using UnityEngine;

/// <summary>
/// Performs pass, shooting, and ball-possession actions for a player.
/// </summary>
public sealed class PlayerKickController : MonoBehaviour
{
    [SerializeField] private SoccerBall ball;
    [SerializeField] private Transform opponentGoal;
    [SerializeField] private PlayerActor[] teammates;

    [SerializeField] private float kickRange = 1.5f;
    [SerializeField] private float passPower = 6f;
    [SerializeField] private float shootPower = 10f;

    private PlayerActor playerActor;

    public Vector2 OpponentGoalPosition =>
        opponentGoal != null
            ? opponentGoal.position
            : transform.position;

    /// <summary>
    /// Retrieves the player actor associated with this controller.
    /// </summary>
    private void Awake()
    {
        playerActor = GetComponent<PlayerActor>();
    }

    /// <summary>
    /// Passes the ball toward another actor.
    /// </summary>
    /// <param name="targetActorId">
    /// The identifier of the intended receiver.
    /// </param>
    public void PassToActor(string targetActorId)
    {
        PlayerActor target =
            GetTeammate(targetActorId);

        if (target == null)
            return;

        PassToPosition(target.Position);
    }

    /// <summary>
    /// Passes the ball toward a world position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended pass destination.
    /// </param>
    public void PassToPosition(Vector2 targetPosition)
    {
        if (!IsBallClose())
            return;

        Vector2 direction =
            targetPosition - (Vector2)ball.transform.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        ball.Kick(direction.normalized, passPower);

        if (playerActor != null)
            playerActor.SetHasBall(false);
    }

    /// <summary>
    /// Shoots the ball toward a world position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended shot destination.
    /// </param>
    public void ShootAtPosition(Vector2 targetPosition)
    {
        if (!IsBallClose())
            return;

        Vector2 direction =
            targetPosition - (Vector2)ball.transform.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        ball.Kick(direction.normalized, shootPower);

        if (playerActor != null)
            playerActor.SetHasBall(false);
    }

    /// <summary>
    /// Attempts to take possession of the nearby ball.
    /// </summary>
    public void TryTakeBall()
    {
        if (!IsBallClose())
            return;

        if (playerActor != null)
            playerActor.SetHasBall(true);
    }

    /// <summary>
    /// Finds the nearest configured teammate.
    /// </summary>
    /// <returns>
    /// The nearest teammate, or null when none are available.
    /// </returns>
    public PlayerActor GetNearestTeammate()
    {
        PlayerActor nearest = null;
        float shortestSqrDistance = Mathf.Infinity;

        foreach (PlayerActor teammate in teammates)
        {
            if (teammate == null ||
                teammate == playerActor ||
                !teammate.IsActive)
            {
                continue;
            }

            float sqrDistance =
                (teammate.Position -
                 (Vector2)transform.position).sqrMagnitude;

            if (sqrDistance >= shortestSqrDistance)
                continue;

            shortestSqrDistance = sqrDistance;
            nearest = teammate;
        }

        return nearest;
    }

    /// <summary>
    /// Finds a teammate using an actor identifier.
    /// </summary>
    /// <param name="targetActorId">
    /// The identifier of the requested teammate.
    /// </param>
    /// <returns>
    /// The matching teammate, or null if none was found.
    /// </returns>
    private PlayerActor GetTeammate(
        string targetActorId)
    {
        if (string.IsNullOrWhiteSpace(targetActorId))
            return null;

        foreach (PlayerActor teammate in teammates)
        {
            if (teammate != null &&
                teammate.ActorId == targetActorId)
            {
                return teammate;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the ball is within interaction range.
    /// </summary>
    /// <returns>
    /// True when the ball is within kick range.
    /// </returns>
    private bool IsBallClose()
    {
        if (ball == null)
            return false;

        return Vector2.Distance(
            transform.position,
            ball.transform.position) <= kickRange;
    }
}