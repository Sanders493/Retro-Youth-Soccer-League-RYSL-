using UnityEngine;

/// <summary>
/// Performs passing, shooting, and ball-possession actions for a player.
/// Kick strength scales with target distance up to configured maximum power.
/// </summary>
public sealed class PlayerKickController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoccerBall ball;
    [SerializeField] private GameState gameState;
    [SerializeField] private PlayerActor[] teammates;

    [Header("Ball Interaction")]
    [Tooltip(
        "The maximum distance from the ball at which the actor can kick " +
        "or take possession.")]
    [SerializeField]
    private float kickRange = 1.5f;

    [Header("Pass Power")]
    [Tooltip(
        "The minimum impulse applied to a short pass.")]
    [SerializeField]
    private float minimumPassPower = 2f;

    [Tooltip(
        "The maximum impulse that can be applied to a pass.")]
    [SerializeField]
    private float maximumPassPower = 12f;

    [Tooltip(
        "The target distance at which maximum pass power is used.")]
    [SerializeField]
    private float distanceForMaximumPassPower = 10f;

    [Header("Shot Power")]
    [Tooltip(
        "The minimum impulse applied to a short shot.")]
    [SerializeField]
    private float minimumShootPower = 5f;

    [Tooltip(
        "The maximum impulse that can be applied to a shot.")]
    [SerializeField]
    private float maximumShootPower = 16f;

    [Tooltip(
        "The target distance at which maximum shot power is used.")]
    [SerializeField]
    private float distanceForMaximumShootPower = 12f;
    [Header("Clearance Power")]
    [SerializeField]
    private float minimumClearancePower = 10f;

    [SerializeField]
    private float maximumClearancePower = 18f;

    [SerializeField]
    private float distanceForMaximumClearancePower = 14f;
    private PlayerActor playerActor;

    /// <summary>
    /// Gets whether this player currently controls the ball.
    /// </summary>
    public bool HasBall =>
        ball != null
        && ball.CurrentController == gameObject;

    /// <summary>
    /// Gets the world position of the opposing goal.
    /// </summary>
    public Vector2 OpponentGoalPosition =>
        gameState != null
        && playerActor != null
            ? gameState.GetOpposingGoalPosition(
                playerActor.TeamId)
            : transform.position;

    /// <summary>
    /// Retrieves the components used by this controller.
    /// </summary>
    private void Awake()
    {
        playerActor =
            GetComponent<PlayerActor>();

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

        if (playerActor == null)
        {
            Debug.LogError(
                $"{name}: No PlayerActor was found.",
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

        ball.SetController(
            gameObject);
    }

    /// <summary>
    /// Passes the ball toward another actor.
    /// </summary>
    /// <param name="targetActorId">
    /// The identifier of the intended receiver.
    /// </param>
    /// <param name="sender">
    /// The actor performing the pass.
    /// </param>
    public void PassToActor(
        string targetActorId,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        PlayerActor target =
            GetTeammate(
                targetActorId);

        if (target == null)
            return;

        PassToPosition(
            target.Position,
            sender);
    }

    /// <summary>
    /// Passes the ball toward a world position using power based on the
    /// distance to the target.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended pass destination.
    /// </param>
    /// <param name="sender">
    /// The actor performing the pass.
    /// </param>
    public void PassToPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        Vector2 direction =
            targetPosition
            - (Vector2)ball.transform.position;

        float targetDistance =
            direction.magnitude;

        if (targetDistance <= Mathf.Epsilon)
            return;

        float calculatedPower =
            CalculateDistanceBasedPower(
                targetDistance,
                minimumPassPower,
                maximumPassPower,
                distanceForMaximumPassPower);

        Debug.Log(
            $"{name}: Pass target={targetPosition}, " +
            $"distance={targetDistance:F2}, " +
            $"power={calculatedPower:F2}.",
            this);

        ball.Kick(
            direction.normalized,
            calculatedPower,
            sender);
    }

    /// <summary>
    /// Shoots the ball toward a world position using power based on the
    /// distance to the target.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended shot destination.
    /// </param>
    /// <param name="sender">
    /// The actor performing the shot.
    /// </param>
    public void ShootAtPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        Vector2 direction =
            targetPosition
            - (Vector2)ball.transform.position;

        float targetDistance =
            direction.magnitude;

        if (targetDistance <= Mathf.Epsilon)
            return;

        float calculatedPower =
            CalculateDistanceBasedPower(
                targetDistance,
                minimumShootPower,
                maximumShootPower,
                distanceForMaximumShootPower);

        Debug.Log(
            $"{name}: Shot target={targetPosition}, " +
            $"distance={targetDistance:F2}, " +
            $"power={calculatedPower:F2}.",
            this);

        ball.Kick(
            direction.normalized,
            calculatedPower,
            sender);
    }

    /// <summary>
    /// Calculates kick power by mapping target distance between the minimum
    /// and maximum configured power.
    /// </summary>
    /// <param name="targetDistance">
    /// The world-space distance from the ball to the target.
    /// </param>
    /// <param name="minimumPower">
    /// The power used at zero distance.
    /// </param>
    /// <param name="maximumPower">
    /// The greatest permitted kick power.
    /// </param>
    /// <param name="maximumPowerDistance">
    /// The distance at which maximum power is reached.
    /// </param>
    /// <returns>
    /// A kick power between the configured minimum and maximum.
    /// </returns>
    private float CalculateDistanceBasedPower(
        float targetDistance,
        float minimumPower,
        float maximumPower,
        float maximumPowerDistance)
    {
        if (maximumPowerDistance <= Mathf.Epsilon)
            return maximumPower;

        float normalizedDistance =
            Mathf.Clamp01(
                targetDistance
                / maximumPowerDistance);

        return Mathf.Lerp(
            minimumPower,
            maximumPower,
            normalizedDistance);
    }

    /// <summary>
    /// Attempts to take possession of a nearby loose or opponent-controlled ball.
    /// </summary>
    public void TryTakeBall()
    {
        if (ball == null
            || playerActor == null)
        {
            return;
        }

        bool isClose =
            IsBallClose();

        Debug.Log(
            $"{name}: TryTakeBall. " +
            $"Close={isClose}, " +
            $"CurrentOwner={ball.CurrentController?.name ?? "None"}",
            this);

        if (!isClose)
            return;

        ball.SetController(
            gameObject);

        Debug.Log(
            $"{name}: Owner after SetController=" +
            $"{ball.CurrentController?.name ?? "None"}",
            this);
    }
    /// <summary>
    /// Clears the ball toward a world position using stronger distance-based
    /// power than a normal pass.
    /// </summary>
    public void ClearToPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (!CanKick(sender))
            return;

        Vector2 direction =
            targetPosition
            - (Vector2)ball.transform.position;

        float targetDistance =
            direction.magnitude;

        if (targetDistance <= Mathf.Epsilon)
            return;

        float calculatedPower =
            CalculateDistanceBasedPower(
                targetDistance,
                minimumClearancePower,
                maximumClearancePower,
                distanceForMaximumClearancePower);

        ball.Kick(
            direction.normalized,
            calculatedPower,
            sender);
    }
    /// <summary>
    /// Finds the nearest configured teammate.
    /// </summary>
    /// <returns>
    /// The nearest valid teammate, or null when none is found.
    /// </returns>
    public PlayerActor GetNearestTeammate()
    {
        PlayerActor nearest = null;

        float shortestSquaredDistance =
            Mathf.Infinity;

        if (teammates == null)
            return null;

        foreach (PlayerActor teammate in teammates)
        {
            if (!IsValidTeammate(teammate))
                continue;

            float squaredDistance =
                (teammate.Position
                 - (Vector2)transform.position)
                .sqrMagnitude;

            if (squaredDistance
                >= shortestSquaredDistance)
            {
                continue;
            }

            shortestSquaredDistance =
                squaredDistance;

            nearest =
                teammate;
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
    /// The matching teammate, or null when none is found.
    /// </returns>
    private PlayerActor GetTeammate(
        string targetActorId)
    {
        if (string.IsNullOrWhiteSpace(
                targetActorId)
            || teammates == null)
        {
            return null;
        }

        foreach (PlayerActor teammate in teammates)
        {
            if (!IsValidTeammate(teammate))
                continue;

            if (teammate.ActorId
                == targetActorId)
            {
                return teammate;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether an actor is a valid teammate.
    /// </summary>
    /// <param name="teammate">
    /// The actor being checked.
    /// </param>
    /// <returns>
    /// True when the actor is an active member of the same team.
    /// </returns>
    private bool IsValidTeammate(
        PlayerActor teammate)
    {
        return teammate != null
            && playerActor != null
            && teammate != playerActor
            && teammate.IsActive
            && teammate.TeamId
                == playerActor.TeamId;
    }

    /// <summary>
    /// Determines whether the sender can currently kick the ball.
    /// </summary>
    /// <param name="sender">
    /// The actor requesting the kick.
    /// </param>
    /// <returns>
    /// True when the sender is this player and currently owns the ball.
    /// </returns>
    private bool CanKick(
        GameObject sender)
    {
        return ball != null
            && sender == gameObject
            && ball.CurrentController == sender
            && IsBallClose();
    }

    /// <summary>
    /// Determines whether the ball is within interaction range using collider
    /// separation rather than transform-center distance.
    /// </summary>
    /// <returns>
    /// True when the player is close enough to interact with the ball.
    /// </returns>
    private bool IsBallClose()
    {
        if (ball == null)
            return false;

        Collider2D playerCollider =
            GetComponent<Collider2D>();

        Collider2D ballCollider =
            ball.GetComponent<Collider2D>();

        if (playerCollider != null
            && ballCollider != null)
        {
            ColliderDistance2D colliderDistance =
                playerCollider.Distance(
                    ballCollider);

            return colliderDistance.distance
                   <= kickRange;
        }

        return Vector2.Distance(
                   transform.position,
                   ball.transform.position)
               <= kickRange;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        kickRange =
            Mathf.Max(
                0f,
                kickRange);

        minimumPassPower =
            Mathf.Max(
                0f,
                minimumPassPower);

        maximumPassPower =
            Mathf.Max(
                minimumPassPower,
                maximumPassPower);

        distanceForMaximumPassPower =
            Mathf.Max(
                0.01f,
                distanceForMaximumPassPower);

        minimumShootPower =
            Mathf.Max(
                0f,
                minimumShootPower);

        maximumShootPower =
            Mathf.Max(
                minimumShootPower,
                maximumShootPower);

        distanceForMaximumShootPower =
            Mathf.Max(
                0.01f,
                distanceForMaximumShootPower);
        
        minimumClearancePower =
            Mathf.Max(
                0f,
                minimumClearancePower);

        maximumClearancePower =
            Mathf.Max(
                minimumClearancePower,
                maximumClearancePower);

        distanceForMaximumClearancePower =
            Mathf.Max(
                0.01f,
                distanceForMaximumClearancePower);
    }
#endif
}