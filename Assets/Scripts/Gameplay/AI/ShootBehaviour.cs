using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chooses whether an actor possessing the ball should shoot, pass, cross,
/// clear the ball forward, or continue holding its formation position.
/// </summary>
public sealed class ShootBehavior : IAIBehavior
{
    private readonly float maximumShootingDistance;
    private readonly float minimumPassDistance;
    private readonly float maximumPassDistance;
    private readonly float passLaneWidth;
    private readonly float crossingWidthThreshold;
    private readonly float defenderDangerDepth;
    private readonly int priority;

    /// <summary>
    /// Creates a ball-distribution behavior.
    /// </summary>
    /// <param name="maximumShootingDistance">
    /// The maximum world-space distance from goal at which a shot is allowed.
    /// </param>
    /// <param name="minimumPassDistance">
    /// The minimum distance required between the passer and receiver.
    /// </param>
    /// <param name="maximumPassDistance">
    /// The maximum distance allowed for a normal pass.
    /// </param>
    /// <param name="passLaneWidth">
    /// The distance from a pass line within which an opponent blocks the pass.
    /// </param>
    /// <param name="crossingWidthThreshold">
    /// The minimum team-relative horizontal separation required for a pass
    /// to be considered a cross.
    /// </param>
    /// <param name="defenderDangerDepth">
    /// The maximum team-relative depth at which a defender may clear the ball
    /// when no safe pass exists.
    /// </param>
    /// <param name="priority">
    /// The behavior priority relative to other behaviors.
    /// </param>
    public ShootBehavior(
        float maximumShootingDistance = 8f,
        float minimumPassDistance = 1.5f,
        float maximumPassDistance = 10f,
        float passLaneWidth = 0.75f,
        float crossingWidthThreshold = 0.75f,
        float defenderDangerDepth = 0.35f,
        int priority = 100)
    {
        this.maximumShootingDistance =
            Mathf.Max(0f, maximumShootingDistance);

        this.minimumPassDistance =
            Mathf.Max(0f, minimumPassDistance);

        this.maximumPassDistance =
            Mathf.Max(this.minimumPassDistance, maximumPassDistance);

        this.passLaneWidth =
            Mathf.Max(0f, passLaneWidth);

        this.crossingWidthThreshold =
            Mathf.Clamp(crossingWidthThreshold, 0f, 2f);

        this.defenderDangerDepth =
            Mathf.Clamp01(defenderDangerDepth);

        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor can make a ball-distribution decision.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when the active AI actor possesses the ball; otherwise, false.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        return context != null
            && context.Actor != null
            && context.GameState != null
            && context.GameState.IsMatchActive
            && context.Actor.IsActive
            && context.Actor.IsAIControlled
            && context.Actor.HasBall
            && context.Actor.PlayerRole != EPlayerRole.Goalkeeper;
    }

    /// <summary>
    /// Returns the priority of this behavior.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The configured behavior priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Chooses between shooting, passing, crossing, clearing, or holding.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting actor assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        if (ShouldShoot(context))
            return CreateShotAssignment(context);

        IAIActor bestReceiver = FindBestPassReceiver(context);

        if (bestReceiver != null)
        {
            if (IsCross(context, bestReceiver))
                return CreateCrossAssignment(context, bestReceiver);

            return CreatePassAssignment(context, bestReceiver);
        }

        if (ShouldClearBall(context))
            return CreateClearanceAssignment(context);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.HoldPosition,
            context.Actor.Position,
            priority: priority);
    }

    /// <summary>
    /// Determines whether the actor should shoot at the opposing goal.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>True when the actor is within shooting range.</returns>
    private bool ShouldShoot(AIBehaviorContext context)
    {
        Vector2 goalPosition =
            context.GameState.GetAttackingGoalPosition(
                context.Actor.TeamId);

        float maximumDistanceSquared =
            maximumShootingDistance * maximumShootingDistance;

        return (goalPosition - context.Actor.Position).sqrMagnitude
            <= maximumDistanceSquared;
    }

    /// <summary>
    /// Creates an assignment that shoots toward the opposing goal.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting shooting assignment.</returns>
    private ActorAssignment CreateShotAssignment(
        AIBehaviorContext context)
    {
        Vector2 goalPosition =
            context.GameState.GetAttackingGoalPosition(
                context.Actor.TeamId);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Shoot,
            goalPosition,
            priority: priority);
    }

    /// <summary>
    /// Finds the safest teammate who also advances the ball toward goal.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// The best available receiver, or null when no safe receiver exists.
    /// </returns>
    private IAIActor FindBestPassReceiver(
        AIBehaviorContext context)
    {
        IReadOnlyList<IAIActor> teammates =
            context.GameState.GetTeamActors(
                context.Actor.TeamId);

        IAIActor bestReceiver = null;
        float bestScore = float.MinValue;

        foreach (IAIActor teammate in teammates)
        {
            if (!IsValidReceiver(context, teammate))
                continue;

            if (IsPassLaneBlocked(
                    context,
                    context.Actor.Position,
                    teammate.Position))
            {
                continue;
            }

            float score =
                ScoreReceiver(context, teammate);

            if (score <= bestScore)
                continue;

            bestScore = score;
            bestReceiver = teammate;
        }

        return bestReceiver;
    }

    /// <summary>
    /// Determines whether a teammate can be considered as a pass target.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="teammate">The potential receiver.</param>
    /// <returns>True when the teammate is a valid target.</returns>
    private bool IsValidReceiver(
        AIBehaviorContext context,
        IAIActor teammate)
    {
        if (teammate == null
            || !teammate.IsActive
            || teammate.ActorId == context.Actor.ActorId)
        {
            return false;
        }

        float distance =
            Vector2.Distance(
                context.Actor.Position,
                teammate.Position);

        return distance >= minimumPassDistance
            && distance <= maximumPassDistance;
    }

    /// <summary>
    /// Scores a potential receiver based on forward progress, distance,
    /// and tactical role.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="receiver">The potential receiver.</param>
    /// <returns>A larger value for a more desirable receiver.</returns>
    private float ScoreReceiver(
        AIBehaviorContext context,
        IAIActor receiver)
    {
        Vector2 passerFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        Vector2 receiverFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                receiver.Position);

        float forwardProgress =
            receiverFieldPosition.y - passerFieldPosition.y;

        float distance =
            Vector2.Distance(
                context.Actor.Position,
                receiver.Position);

        float roleBonus =
            GetReceiverRoleBonus(receiver.PlayerRole);

        float crossBonus =
            IsCross(context, receiver) ? 0.5f : 0f;

        return forwardProgress * 4f
            - distance * 0.15f
            + roleBonus
            + crossBonus;
    }

    /// <summary>
    /// Returns the tactical preference associated with a receiver's role.
    /// </summary>
    /// <param name="role">The receiver's role.</param>
    /// <returns>The role score bonus.</returns>
    private float GetReceiverRoleBonus(EPlayerRole role)
    {
        switch (role)
        {
            case EPlayerRole.Forward:
                return 2f;

            case EPlayerRole.Midfielder:
                return 1f;

            case EPlayerRole.Defender:
                return 0.25f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Determines whether a pass lane is obstructed by an opponent.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="startPosition">The pass origin.</param>
    /// <param name="endPosition">The intended pass destination.</param>
    /// <returns>True when an opponent blocks the passing lane.</returns>
    private bool IsPassLaneBlocked(
        AIBehaviorContext context,
        Vector2 startPosition,
        Vector2 endPosition)
    {
        ETeamId opposingTeam =
            GetOpposingTeam(context.Actor.TeamId);

        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(opposingTeam);

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null || !opponent.IsActive)
                continue;

            float distanceToLane =
                DistanceFromPointToSegment(
                    opponent.Position,
                    startPosition,
                    endPosition);

            if (distanceToLane <= passLaneWidth)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a pass travels across the center of the field.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="receiver">The intended receiver.</param>
    /// <returns>True when the pass qualifies as a cross.</returns>
    private bool IsCross(
        AIBehaviorContext context,
        IAIActor receiver)
    {
        Vector2 passerFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        Vector2 receiverFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                receiver.Position);

        bool crossesCenter =
            Mathf.Sign(passerFieldPosition.x)
            != Mathf.Sign(receiverFieldPosition.x);

        float horizontalDistance =
            Mathf.Abs(
                receiverFieldPosition.x
                - passerFieldPosition.x);

        return crossesCenter
            && horizontalDistance >= crossingWidthThreshold;
    }

    /// <summary>
    /// Creates a normal pass assignment.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="receiver">The intended receiver.</param>
    /// <returns>The resulting pass assignment.</returns>
    private ActorAssignment CreatePassAssignment(
        AIBehaviorContext context,
        IAIActor receiver)
    {
        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Pass,
            receiver.Position,
            receiver.ActorId,
            priority);
    }

    /// <summary>
    /// Creates a pass assignment that crosses the field center.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="receiver">The intended receiver.</param>
    /// <returns>The resulting crossing assignment.</returns>
    private ActorAssignment CreateCrossAssignment(
        AIBehaviorContext context,
        IAIActor receiver)
    {
        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Pass,
            receiver.Position,
            receiver.ActorId,
            priority);
    }

    /// <summary>
    /// Determines whether a defender should clear the ball forward.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when the actor is a defender in its defensive area and no safe
    /// pass has been found.
    /// </returns>
    private bool ShouldClearBall(
        AIBehaviorContext context)
    {
        if (context.Actor.PlayerRole != EPlayerRole.Defender)
            return false;

        Vector2 actorFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        bool insidePenaltyBox =
            context.GameState.IsInsideDefendingPenaltyBox(
                context.Actor.TeamId,
                context.Actor.Position);

        return insidePenaltyBox
            || actorFieldPosition.y <= defenderDangerDepth;
    }

    /// <summary>
    /// Creates a forward clearance assignment for a defender.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting clearance assignment.</returns>
    private ActorAssignment CreateClearanceAssignment(
        AIBehaviorContext context)
    {
        Vector2 actorFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        float targetHorizontalPosition =
            actorFieldPosition.x * 0.5f;

        Vector2 clearanceFieldPosition = new Vector2(
            targetHorizontalPosition,
            0.8f);

        Vector2 clearanceWorldPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                clearanceFieldPosition);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Shoot,
            clearanceWorldPosition,
            priority: priority);
    }

    /// <summary>
    /// Returns the team opposing the specified team.
    /// </summary>
    /// <param name="teamId">The team being evaluated.</param>
    /// <returns>The opposing team identifier.</returns>
    private ETeamId GetOpposingTeam(ETeamId teamId)
    {
        return teamId == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }

    /// <summary>
    /// Calculates the shortest distance from a point to a line segment.
    /// </summary>
    /// <param name="point">The point being measured.</param>
    /// <param name="segmentStart">The beginning of the line segment.</param>
    /// <param name="segmentEnd">The end of the line segment.</param>
    /// <returns>The shortest distance from the point to the segment.</returns>
    private float DistanceFromPointToSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;

        if (segment.sqrMagnitude <= Mathf.Epsilon)
            return Vector2.Distance(point, segmentStart);

        float projection =
            Vector2.Dot(point - segmentStart, segment)
            / segment.sqrMagnitude;

        projection = Mathf.Clamp01(projection);

        Vector2 closestPoint =
            segmentStart + segment * projection;

        return Vector2.Distance(point, closestPoint);
    }
}