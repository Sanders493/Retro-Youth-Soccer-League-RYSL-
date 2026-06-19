using UnityEngine;

/// <summary>
/// Performs passing, shooting, clearance, and ball-possession actions for a
/// player actor.
/// </summary>
[RequireComponent(typeof(PlayerActor))]
[RequireComponent(typeof(Collider2D))]
public sealed class PlayerKickController :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoccerBall ball;
    [SerializeField] private GameState gameState;
    [SerializeField] private PlayerActor[] teammates;

    [Header("Ball Interaction")]
    [Tooltip(
        "The maximum collider separation at which the actor can kick or " +
        "take possession of the ball.")]
    [SerializeField]
    private float kickRange = 1.5f;

    [Header("Pass Power")]
    [Tooltip(
        "The minimum impulse applied to a short pass.")]
    [SerializeField]
    private float minimumPassPower = 2f;

    [Tooltip(
        "The maximum impulse applied to a pass.")]
    [SerializeField]
    private float maximumPassPower = 12f;

    [Tooltip(
        "The target distance at which maximum pass power is reached.")]
    [SerializeField]
    private float distanceForMaximumPassPower = 10f;

    [Header("Shot Power")]
    [Tooltip(
        "The minimum impulse applied to a short shot.")]
    [SerializeField]
    private float minimumShootPower = 5f;

    [Tooltip(
        "The maximum impulse applied to a shot.")]
    [SerializeField]
    private float maximumShootPower = 16f;

    [Tooltip(
        "The target distance at which maximum shot power is reached.")]
    [SerializeField]
    private float distanceForMaximumShootPower = 12f;

    [Header("Clearance Power")]
    [Tooltip(
        "The minimum impulse applied to a short clearance.")]
    [SerializeField]
    private float minimumClearancePower = 10f;

    [Tooltip(
        "The maximum impulse applied to a clearance.")]
    [SerializeField]
    private float maximumClearancePower = 18f;

    [Tooltip(
        "The target distance at which maximum clearance power is reached.")]
    [SerializeField]
    private float distanceForMaximumClearancePower = 14f;
    
    [Header("Goalkeeper Throw Power")]
    [SerializeField]
    private float minimumThrowPower = 3f;

    [SerializeField]
    private float maximumThrowPower = 10f;

    [SerializeField]
    private float distanceForMaximumThrowPower = 10f;

    [Header("Debug")]
    [SerializeField]
    private bool logSuccessfulActions;

    [SerializeField]
    private bool logRejectedActions;

    private PlayerActor playerActor;
    private Collider2D playerCollider;
    private Collider2D cachedBallCollider;

    /// <summary>
    /// Gets whether this actor currently controls the ball.
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
    /// Retrieves and validates required components.
    /// </summary>
    private void Awake()
    {
        playerActor =
            GetComponent<PlayerActor>();

        playerCollider =
            GetComponent<Collider2D>();

        if (ball != null)
        {
            cachedBallCollider =
                ball.GetComponent<Collider2D>();
        }

        ValidateReferences();
    }

    /// <summary>
    /// Logs missing required references.
    /// </summary>
    private void ValidateReferences()
    {
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

        if (playerCollider == null)
        {
            Debug.LogError(
                $"{name}: No Collider2D was found.",
                this);
        }
    }

    /// <summary>
    /// Gives this actor possession when setting up the match.
    /// </summary>
    public void TakeStartingPossession()
    {
        if (ball == null)
        {
            LogRejectedAction(
                "Starting possession",
                "no ball is assigned");

            return;
        }

        bool succeeded =
            ball.SetController(
                gameObject);

        if (succeeded)
        {
            LogSuccessfulAction(
                "Starting possession acquired.");
        }
        else
        {
            LogRejectedAction(
                "Starting possession",
                "SoccerBall rejected the control request");
        }
    }

    /// <summary>
    /// Passes the ball toward a teammate identified by actor ID.
    /// </summary>
    /// <param name="targetActorId">
    /// The intended receiver's actor identifier.
    /// </param>
    /// <param name="sender">
    /// The actor performing the pass.
    /// </param>
    public void PassToActor(
        string targetActorId,
        GameObject sender)
    {
        if (!CanKick(
                sender,
                out string rejectionReason))
        {
            LogRejectedAction(
                "Pass",
                rejectionReason);

            return;
        }

        PlayerActor target =
            GetTeammate(
                targetActorId);

        if (target == null)
        {
            LogRejectedAction(
                "Pass",
                $"teammate {targetActorId} was not found");

            return;
        }

        ExecuteKickTowardPosition(
            target.Position,
            sender,
            minimumPassPower,
            maximumPassPower,
            distanceForMaximumPassPower,
            "Pass");
    }

    /// <summary>
    /// Passes the ball toward a world-space position.
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
        ExecuteKickTowardPosition(
            targetPosition,
            sender,
            minimumPassPower,
            maximumPassPower,
            distanceForMaximumPassPower,
            "Pass");
    }

    /// <summary>
    /// Shoots the ball toward a world-space position.
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
        if (gameState != null
            && gameState.HasActivePass)
        {
            gameState.ClearActivePass();
        }

        ExecuteKickTowardPosition(
            targetPosition,
            sender,
            minimumShootPower,
            maximumShootPower,
            distanceForMaximumShootPower,
            "Shot");
    }

    /// <summary>
    /// Clears the ball toward a world-space position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended clearance destination.
    /// </param>
    /// <param name="sender">
    /// The actor performing the clearance.
    /// </param>
    public void ClearToPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (gameState != null
            && gameState.HasActivePass)
        {
            gameState.ClearActivePass();
        }

        ExecuteKickTowardPosition(
            targetPosition,
            sender,
            minimumClearancePower,
            maximumClearancePower,
            distanceForMaximumClearancePower,
            "Clearance");
    }
    /// <summary>
    /// Throws the controlled ball toward a world-space destination.
    /// </summary>
    public void ThrowToPosition(
        Vector2 targetPosition,
        GameObject sender)
    {
        if (playerActor == null
            || !playerActor.IsGoalkeeper)
        {
            LogRejectedAction(
                "Throw",
                "actor is not a goalkeeper");

            return;
        }

        ExecuteKickTowardPosition(
            targetPosition,
            sender,
            minimumThrowPower,
            maximumThrowPower,
            distanceForMaximumThrowPower,
            "Throw");
    }
    /// <summary>
    /// Validates and performs a distance-scaled kick toward a position.
    /// </summary>
    /// <param name="targetPosition">
    /// The intended destination.
    /// </param>
    /// <param name="sender">
    /// The actor requesting the kick.
    /// </param>
    /// <param name="minimumPower">
    /// The minimum kick impulse.
    /// </param>
    /// <param name="maximumPower">
    /// The maximum kick impulse.
    /// </param>
    /// <param name="maximumPowerDistance">
    /// The target distance at which maximum power is reached.
    /// </param>
    /// <param name="actionName">
    /// The action name used for debugging.
    /// </param>
    private void ExecuteKickTowardPosition(
        Vector2 targetPosition,
        GameObject sender,
        float minimumPower,
        float maximumPower,
        float maximumPowerDistance,
        string actionName)
    {
        if (!CanKick(
                sender,
                out string rejectionReason))
        {
            LogRejectedAction(
                actionName,
                rejectionReason);

            return;
        }

        Vector2 ballPosition =
            ball.transform.position;

        Vector2 direction =
            targetPosition
            - ballPosition;

        float targetDistance =
            direction.magnitude;

        if (targetDistance <= Mathf.Epsilon)
        {
            LogRejectedAction(
                actionName,
                "target is at the ball's current position");

            return;
        }

        float calculatedPower =
            CalculateDistanceBasedPower(
                targetDistance,
                minimumPower,
                maximumPower,
                maximumPowerDistance);

        LogSuccessfulAction(
            $"{actionName}: " +
            $"sender={sender.name}, " +
            $"target={targetPosition}, " +
            $"distance={targetDistance:F2}, " +
            $"power={calculatedPower:F2}.");

        ball.Kick(
            direction.normalized,
            calculatedPower,
            sender);
    }

    /// <summary>
    /// Calculates power by mapping target distance between configured minimum
    /// and maximum values.
    /// </summary>
    /// <param name="targetDistance">
    /// The distance from the ball to the target.
    /// </param>
    /// <param name="minimumPower">
    /// The power used at zero distance.
    /// </param>
    /// <param name="maximumPower">
    /// The greatest permitted power.
    /// </param>
    /// <param name="maximumPowerDistance">
    /// The distance at which maximum power is reached.
    /// </param>
    /// <returns>
    /// The calculated power between the configured limits.
    /// </returns>
    private float CalculateDistanceBasedPower(
        float targetDistance,
        float minimumPower,
        float maximumPower,
        float maximumPowerDistance)
    {
        if (maximumPowerDistance
            <= Mathf.Epsilon)
        {
            return maximumPower;
        }

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
    /// Attempts to take possession of a nearby loose or opponent-controlled
    /// ball.
    /// </summary>
    public void TryTakeBall()
    {
        if (ball == null)
        {
            LogRejectedAction(
                "Take ball",
                "no ball is assigned");

            return;
        }

        if (playerActor == null)
        {
            LogRejectedAction(
                "Take ball",
                "no PlayerActor is available");

            return;
        }

        if (HasBall)
        {
            LogRejectedAction(
                "Take ball",
                "actor already controls the ball");

            return;
        }

        if (!IsBallClose(
                out float colliderDistance))
        {
            LogRejectedAction(
                "Take ball",
                $"ball is out of range " +
                $"({colliderDistance:F2} > {kickRange:F2})");

            return;
        }

        GameObject previousController =
            ball.CurrentController;

        bool succeeded =
            ball.SetController(
                gameObject);

        if (!succeeded)
            return;

        if (gameState != null
            && gameState.HasActivePass)
        {
            gameState.ClearActivePass();
        }
        LogSuccessfulAction(
            $"Take ball succeeded. " +
            $"Previous owner=" +
            $"{previousController?.name ?? "None"}.");
    }

    /// <summary>
    /// Finds the nearest configured valid teammate.
    /// </summary>
    /// <returns>
    /// The nearest valid teammate, or null when none is available.
    /// </returns>
    public PlayerActor GetNearestTeammate()
    {
        if (teammates == null)
            return null;

        PlayerActor nearestTeammate =
            null;

        float shortestSquaredDistance =
            float.PositiveInfinity;

        foreach (PlayerActor teammate in teammates)
        {
            if (!IsValidTeammate(
                    teammate))
            {
                continue;
            }

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

            nearestTeammate =
                teammate;
        }

        return nearestTeammate;
    }

    /// <summary>
    /// Finds a configured teammate using an actor identifier.
    /// </summary>
    /// <param name="targetActorId">
    /// The requested teammate identifier.
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
            if (!IsValidTeammate(
                    teammate))
            {
                continue;
            }

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
    /// True when the actor is an active teammate other than this actor.
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
    /// Checks whether the supplied actor may currently kick the ball.
    /// </summary>
    /// <param name="sender">
    /// The actor requesting the kick.
    /// </param>
    /// <param name="rejectionReason">
    /// The reason the kick is unavailable.
    /// </param>
    /// <returns>
    /// True when the sender owns and is close enough to the ball.
    /// </returns>
    private bool CanKick(
        GameObject sender,
        out string rejectionReason)
    {
        rejectionReason =
            string.Empty;

        if (ball == null)
        {
            rejectionReason =
                "no ball is assigned";

            return false;
        }

        if (sender == null)
        {
            rejectionReason =
                "sender is null";

            return false;
        }

        if (sender != gameObject)
        {
            rejectionReason =
                $"sender {sender.name} does not match {name}";

            return false;
        }

        if (ball.CurrentController
            != sender)
        {
            rejectionReason =
                $"sender does not control the ball; " +
                $"owner=" +
                $"{ball.CurrentController?.name ?? "None"}";

            return false;
        }

        if (!IsBallClose(
                out float colliderDistance))
        {
            rejectionReason =
                $"ball is outside kick range " +
                $"({colliderDistance:F2} > {kickRange:F2})";

            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the ball is within interaction range using collider
    /// separation.
    /// </summary>
    /// <param name="distance">
    /// The calculated collider or transform distance.
    /// </param>
    /// <returns>
    /// True when the ball is within the configured interaction range.
    /// </returns>
    private bool IsBallClose(
        out float distance)
    {
        distance =
            float.PositiveInfinity;

        if (ball == null)
            return false;

        if (playerCollider != null
            && cachedBallCollider != null)
        {
            ColliderDistance2D colliderDistance =
                playerCollider.Distance(
                    cachedBallCollider);

            distance =
                Mathf.Max(
                    0f,
                    colliderDistance.distance);

            return distance <= kickRange;
        }

        distance =
            Vector2.Distance(
                transform.position,
                ball.transform.position);

        return distance <= kickRange;
    }

    /// <summary>
    /// Logs a successful action when action debugging is enabled.
    /// </summary>
    private void LogSuccessfulAction(
        string message)
    {
        if (!logSuccessfulActions)
            return;

        Debug.Log(
            $"{name}: {message}",
            this);
    }

    /// <summary>
    /// Logs a rejected action when rejection debugging is enabled.
    /// </summary>
    private void LogRejectedAction(
        string actionName,
        string reason)
    {
        if (!logRejectedActions)
            return;

        Debug.Log(
            $"{name}: {actionName} rejected. " +
            $"Reason={reason}.",
            this);
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

        ValidatePowerRange(
            ref minimumPassPower,
            ref maximumPassPower,
            ref distanceForMaximumPassPower);

        ValidatePowerRange(
            ref minimumShootPower,
            ref maximumShootPower,
            ref distanceForMaximumShootPower);

        ValidatePowerRange(
            ref minimumClearancePower,
            ref maximumClearancePower,
            ref distanceForMaximumClearancePower);

        ValidatePowerRange(
            ref minimumThrowPower,
            ref maximumThrowPower,
            ref distanceForMaximumThrowPower);
    }

    /// <summary>
    /// Restricts one distance-based power configuration to valid values.
    /// </summary>
    private void ValidatePowerRange(
        ref float minimumPower,
        ref float maximumPower,
        ref float maximumPowerDistance)
    {
        minimumPower =
            Mathf.Max(
                0f,
                minimumPower);

        maximumPower =
            Mathf.Max(
                minimumPower,
                maximumPower);

        maximumPowerDistance =
            Mathf.Max(
                0.01f,
                maximumPowerDistance);
    }
#endif
}