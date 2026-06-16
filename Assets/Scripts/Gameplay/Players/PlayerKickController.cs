using UnityEngine;

/// <summary>
/// Performs pass, shooting, and ball-possession actions for a player.
/// </summary>
public sealed class PlayerKickController : MonoBehaviour
{
    [SerializeField] private SoccerBall ball;
    [SerializeField] private GameState gameState;
    [SerializeField] private PlayerActor[] teammates;

    [SerializeField] private float kickRange = 1.5f;
    [SerializeField] private float passPower = 6f;
    [SerializeField] private float shootPower = 10f;

    private PlayerActor playerActor;

    public bool HasBall =>
        ball != null &&
        ball.CurrentController == gameObject;

    public Vector2 OpponentGoalPosition =>
        gameState != null && playerActor != null
            ? gameState.GetOpposingGoalPosition(playerActor.TeamId)
            : transform.position;

    /// <summary>
    /// Retrieves the player actor associated with this controller.
    /// </summary>
    private void Awake()
    {
        playerActor = GetComponent<PlayerActor>();

        if (ball == null)
        {
            Debug.LogError(
                $"{name}: No SoccerBall has been assigned.",
                this);
        }

        if (gameState == null)
        {
            Debug.LogError(
                $"{name}: No GameState has been assigned.",
                this);
        }
    }

    /// <summary>
    /// Gives this player possession when setting up the match.
    /// </summary>
    public void TakeStartingPossession()
    {
        if (ball == null)
            return;

        ball.SetController(gameObject);
    }

    /// <summary>
    /// Passes the ball toward another actor.
    /// </summary>
    /// <param name="targetActorId">
    /// The identifier of the intended receiver.
    /// </param>
    /// <param name="sender">The actor performing the pass.</param>
    public void PassToActor(
        string targetActorId,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        PlayerActor target =
            GetTeammate(targetActorId);

        if (target == null)
            return;

        PassToPosition(
            target.Position,
            sender);
    }

    /// <summary>
    /// Passes the ball toward a world position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended pass destination.
    /// </param>
    /// <param name="sender">The actor performing the pass.</param>
    public void PassToPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        Vector2 direction =
            targetPosition -
            (Vector2)ball.transform.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        ball.Kick(
            direction.normalized,
            passPower,
            sender);
    }

    /// <summary>
    /// Shoots the ball toward a world position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended shot destination.
    /// </param>
    /// <param name="sender">The actor performing the shot.</param>
    public void ShootAtPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        Vector2 direction =
            targetPosition -
            (Vector2)ball.transform.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        ball.Kick(
            direction.normalized,
            shootPower,
            sender);
    }

    /// <summary>
    /// Attempts to take possession of the nearby ball.
    /// </summary>
    public void TryTakeBall()
    {
        if (ball == null ||
            playerActor == null ||
            HasBall ||
            !IsBallClose())
        {
            return;
        }

        ball.SetController(gameObject);
    }

    /// <summary>
    /// Finds the nearest configured teammate.
    /// </summary>
    /// <returns>The nearest valid teammate.</returns>
    public PlayerActor GetNearestTeammate()
    {
        PlayerActor nearest = null;
        float shortestSqrDistance = Mathf.Infinity;

        if (teammates == null)
            return null;

        foreach (PlayerActor teammate in teammates)
        {
            if (!IsValidTeammate(teammate))
                continue;

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
    /// <returns>The matching teammate.</returns>
    private PlayerActor GetTeammate(string targetActorId)
    {
        if (string.IsNullOrWhiteSpace(targetActorId) ||
            teammates == null)
        {
            return null;
        }

        foreach (PlayerActor teammate in teammates)
        {
            if (!IsValidTeammate(teammate))
                continue;

            if (teammate.ActorId == targetActorId)
                return teammate;
        }

        return null;
    }

    /// <summary>
    /// Determines whether an actor is a valid teammate.
    /// </summary>
    /// <param name="teammate">The actor being checked.</param>
    /// <returns>
    /// True when the actor is an active member of the same team.
    /// </returns>
    private bool IsValidTeammate(PlayerActor teammate)
    {
        return teammate != null
               && playerActor != null
               && teammate != playerActor
               && teammate.IsActive
               && teammate.TeamId == playerActor.TeamId;
    }

    /// <summary>
    /// Determines whether the sender can currently kick the ball.
    /// </summary>
    /// <param name="sender">The actor requesting the kick.</param>
    /// <returns>
    /// True when the sender is this player and owns the ball.
    /// </returns>
    private bool CanKick(GameObject sender)
    {
        return ball != null
               && sender == gameObject
               && ball.CurrentController == sender
               && IsBallClose();
    }

    /// <summary>
    /// Determines whether the ball is within interaction range.
    /// </summary>
    /// <returns>True when the ball is within kick range.</returns>
    private bool IsBallClose()
    {
        if (ball == null)
            return false;

        return Vector2.Distance(
            transform.position,
            ball.transform.position) <= kickRange;
    }
}