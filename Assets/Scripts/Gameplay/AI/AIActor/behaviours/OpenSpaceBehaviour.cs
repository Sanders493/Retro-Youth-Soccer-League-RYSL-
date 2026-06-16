using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Positions attacking AI actors based on their role, formation lane,
/// the ball's progress, and whether they should support a pass or cross.
/// </summary>
public sealed class OpenSpaceBehavior : IAIBehavior
{
    private readonly float horizontalSupportOffset;
    private readonly float verticalSupportOffset;
    private readonly float opponentAvoidanceDistance;
    private readonly float teammateSpacingDistance;
    private readonly float wideBallThreshold;
    private readonly float crossingDepthThreshold;
    private readonly float passLaneWidth;
    private readonly int priority;

    /// <summary>
    /// Creates an attacking open-space behavior.
    /// </summary>
    /// <param name="horizontalSupportOffset">
    /// The normalized horizontal distance used to create passing angles.
    /// </param>
    /// <param name="verticalSupportOffset">
    /// The normalized vertical distance used to position ahead of or behind
    /// the ball.
    /// </param>
    /// <param name="opponentAvoidanceDistance">
    /// The preferred world-space distance from opponents.
    /// </param>
    /// <param name="teammateSpacingDistance">
    /// The preferred world-space distance from teammates.
    /// </param>
    /// <param name="wideBallThreshold">
    /// The normalized horizontal position at which the ball is considered
    /// wide enough to support a cross.
    /// </param>
    /// <param name="crossingDepthThreshold">
    /// The normalized attacking depth at which crossing positions become
    /// appropriate.
    /// </param>
    /// <param name="passLaneWidth">
    /// The distance from a passing line within which an opponent is
    /// considered to block that lane.
    /// </param>
    /// <param name="priority">
    /// The priority of open-space movement relative to other behaviors.
    /// </param>
    public OpenSpaceBehavior(
        float horizontalSupportOffset = 0.25f,
        float verticalSupportOffset = 0.15f,
        float opponentAvoidanceDistance = 2f,
        float teammateSpacingDistance = 1.5f,
        float wideBallThreshold = 0.55f,
        float crossingDepthThreshold = 0.65f,
        float passLaneWidth = 0.75f,
        int priority = 20)
    {
        this.horizontalSupportOffset =
            Mathf.Max(0f, horizontalSupportOffset);

        this.verticalSupportOffset =
            Mathf.Max(0f, verticalSupportOffset);

        this.opponentAvoidanceDistance =
            Mathf.Max(0f, opponentAvoidanceDistance);

        this.teammateSpacingDistance =
            Mathf.Max(0f, teammateSpacingDistance);

        this.wideBallThreshold =
            Mathf.Clamp01(wideBallThreshold);

        this.crossingDepthThreshold =
            Mathf.Clamp01(crossingDepthThreshold);

        this.passLaneWidth =
            Mathf.Max(0f, passLaneWidth);

        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor should move into attacking space.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when the actor's team possesses the ball and the actor is an
    /// eligible outfield player without the ball.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        if (!context.GameState.IsMatchActive
            || !context.Actor.IsActive
            || !context.Actor.IsAIControlled
            || context.Actor.HasBall
            || context.Actor.PlayerRole == EPlayerRole.Goalkeeper)
        {
            return false;
        }

        return context.GameState.HasPossession(
            context.Actor.TeamId);
    }

    /// <summary>
    /// Returns the priority of open-space movement.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The configured behavior priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates an assignment that moves the actor into the best available
    /// attacking position.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting movement assignment.</returns>
    public ActorAssignment CreateAssignment(
        AIBehaviorContext context)
    {
        Vector2 targetPosition =
            FindBestOpenPosition(context);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            targetPosition,
            priority: priority);
    }

    /// <summary>
    /// Finds the highest-scoring attacking position for the actor.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The selected world-space destination.</returns>
    private Vector2 FindBestOpenPosition(
        AIBehaviorContext context)
    {
        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        Vector2 roleBasePosition =
            GetRoleBasePosition(
                context,
                ballFieldPosition);

        Vector2[] candidates =
            CreateCandidates(
                context,
                roleBasePosition,
                ballFieldPosition);

        Vector2 bestPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                ClampFieldPosition(roleBasePosition));

        float bestScore = float.MinValue;

        foreach (Vector2 candidate in candidates)
        {
            Vector2 clampedCandidate =
                ClampFieldPosition(candidate);

            Vector2 worldCandidate =
                context.GameState.GetWorldPositionFromTeamRelative(
                    context.Actor.TeamId,
                    clampedCandidate);

            float score = ScorePosition(
                context,
                worldCandidate,
                clampedCandidate,
                ballFieldPosition);

            if (score <= bestScore)
                continue;

            bestScore = score;
            bestPosition = worldCandidate;
        }

        return bestPosition;
    }

    /// <summary>
    /// Calculates the actor's starting attacking position based on role and
    /// the ball's progress up the field.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="ballFieldPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>The actor's normalized base position.</returns>
    private Vector2 GetRoleBasePosition(
        AIBehaviorContext context,
        Vector2 ballFieldPosition)
    {
        float formationLane =
            GetFormationLane(context.FormationPosition);

        float targetDepth;

        switch (context.Actor.PlayerRole)
        {
            case EPlayerRole.Defender:
                targetDepth = Mathf.Clamp(
                    ballFieldPosition.y - 0.2f,
                    0.15f,
                    0.55f);
                break;

            case EPlayerRole.Midfielder:
                targetDepth = Mathf.Clamp(
                    ballFieldPosition.y
                    + GetMidfielderDepthOffset(ballFieldPosition.y),
                    0.3f,
                    0.8f);
                break;

            case EPlayerRole.Forward:
                targetDepth = Mathf.Clamp(
                    ballFieldPosition.y
                    + GetForwardDepthOffset(ballFieldPosition.y),
                    0.5f,
                    0.95f);
                break;

            default:
                targetDepth = ballFieldPosition.y;
                break;
        }

        float ballLaneInfluence =
            GetBallLaneInfluence(
                context.Actor.PlayerRole,
                ballFieldPosition.y);

        float targetLane = Mathf.Lerp(
            formationLane,
            ballFieldPosition.x,
            ballLaneInfluence);

        return new Vector2(
            targetLane,
            targetDepth);
    }

    /// <summary>
    /// Returns how far a midfielder should position relative to the ball.
    /// </summary>
    /// <param name="ballDepth">
    /// The ball's normalized attacking depth.
    /// </param>
    /// <returns>The midfielder's vertical offset.</returns>
    private float GetMidfielderDepthOffset(float ballDepth)
    {
        if (ballDepth < 0.35f)
            return 0.1f;

        if (ballDepth < 0.7f)
            return 0.05f;

        return -0.05f;
    }

    /// <summary>
    /// Returns how far a forward should position relative to the ball.
    /// </summary>
    /// <param name="ballDepth">
    /// The ball's normalized attacking depth.
    /// </param>
    /// <returns>The forward's vertical offset.</returns>
    private float GetForwardDepthOffset(float ballDepth)
    {
        if (ballDepth < 0.35f)
            return 0.25f;

        if (ballDepth < 0.7f)
            return 0.18f;

        return 0.08f;
    }

    /// <summary>
    /// Returns how strongly the actor should shift toward the ball's lane.
    /// </summary>
    /// <param name="role">The actor's role.</param>
    /// <param name="ballDepth">
    /// The ball's normalized attacking depth.
    /// </param>
    /// <returns>A lane influence from zero to one.</returns>
    private float GetBallLaneInfluence(
        EPlayerRole role,
        float ballDepth)
    {
        switch (role)
        {
            case EPlayerRole.Defender:
                return 0.25f;

            case EPlayerRole.Midfielder:
                return ballDepth > 0.65f
                    ? 0.45f
                    : 0.35f;

            case EPlayerRole.Forward:
                return ballDepth > 0.65f
                    ? 0.3f
                    : 0.45f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Creates potential support, pass, and crossing positions.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="basePosition">
    /// The actor's role-based normalized position.
    /// </param>
    /// <param name="ballPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>The generated candidate positions.</returns>
    private Vector2[] CreateCandidates(
        AIBehaviorContext context,
        Vector2 basePosition,
        Vector2 ballPosition)
    {
        bool shouldPositionForCross =
            ShouldPositionForCross(
                context,
                ballPosition);

        if (shouldPositionForCross)
        {
            return CreateCrossingCandidates(
                context,
                basePosition,
                ballPosition);
        }

        return CreatePassingCandidates(
            context,
            basePosition,
            ballPosition);
    }

    /// <summary>
    /// Determines whether the actor should position itself to receive a
    /// potential cross.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="ballPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>
    /// True when the ball is wide and sufficiently advanced.
    /// </returns>
    private bool ShouldPositionForCross(
        AIBehaviorContext context,
        Vector2 ballPosition)
    {
        if (context.Actor.PlayerRole != EPlayerRole.Forward
            && context.Actor.PlayerRole != EPlayerRole.Midfielder)
        {
            return false;
        }

        return Mathf.Abs(ballPosition.x) >= wideBallThreshold
            && ballPosition.y >= crossingDepthThreshold;
    }

    /// <summary>
    /// Creates positions that provide normal passing options around the
    /// ball carrier.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="basePosition">
    /// The actor's role-based normalized position.
    /// </param>
    /// <param name="ballPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>The passing-support candidates.</returns>
    private Vector2[] CreatePassingCandidates(
        AIBehaviorContext context,
        Vector2 basePosition,
        Vector2 ballPosition)
    {
        float actorLane =
            GetFormationLane(context.FormationPosition);

        float sideDirection =
            actorLane == 0f
                ? GetOpenSideDirection(ballPosition.x)
                : Mathf.Sign(actorLane);

        return new[]
        {
            basePosition,

            new Vector2(
                basePosition.x
                    + horizontalSupportOffset * sideDirection,
                basePosition.y),

            new Vector2(
                basePosition.x
                    - horizontalSupportOffset * sideDirection,
                basePosition.y),

            new Vector2(
                basePosition.x,
                basePosition.y + verticalSupportOffset),

            new Vector2(
                basePosition.x,
                basePosition.y - verticalSupportOffset),

            new Vector2(
                ballPosition.x
                    + horizontalSupportOffset * sideDirection,
                ballPosition.y + verticalSupportOffset),

            new Vector2(
                ballPosition.x
                    - horizontalSupportOffset * sideDirection,
                ballPosition.y - verticalSupportOffset)
        };
    }

    /// <summary>
    /// Creates central, near-side, and far-side positions for a potential
    /// cross.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="basePosition">
    /// The actor's role-based normalized position.
    /// </param>
    /// <param name="ballPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>The crossing-support candidates.</returns>
    private Vector2[] CreateCrossingCandidates(
        AIBehaviorContext context,
        Vector2 basePosition,
        Vector2 ballPosition)
    {
        float ballSide = Mathf.Sign(ballPosition.x);

        float actorLane =
            GetFormationLane(context.FormationPosition);

        float centralDepth =
            context.Actor.PlayerRole == EPlayerRole.Forward
                ? 0.88f
                : 0.75f;

        float preferredHorizontalPosition;

        if (actorLane == -ballSide)
        {
            preferredHorizontalPosition =
                -ballSide * 0.35f;
        }
        else if (actorLane == 0f)
        {
            preferredHorizontalPosition = 0f;
        }
        else
        {
            preferredHorizontalPosition =
                ballSide * 0.2f;
        }

        return new[]
        {
            new Vector2(
                preferredHorizontalPosition,
                centralDepth),

            new Vector2(
                0f,
                centralDepth),

            new Vector2(
                -ballSide * 0.35f,
                centralDepth),

            new Vector2(
                ballSide * 0.2f,
                centralDepth - 0.08f),

            basePosition
        };
    }

    /// <summary>
    /// Scores a potential attacking position.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="worldPosition">The candidate world position.</param>
    /// <param name="fieldPosition">
    /// The candidate team-relative field position.
    /// </param>
    /// <param name="ballFieldPosition">
    /// The ball's team-relative field position.
    /// </param>
    /// <returns>The candidate score.</returns>
    private float ScorePosition(
        AIBehaviorContext context,
        Vector2 worldPosition,
        Vector2 fieldPosition,
        Vector2 ballFieldPosition)
    {
        float nearestOpponentDistance =
            GetNearestOpponentDistance(
                context,
                worldPosition);

        float nearestTeammateDistance =
            GetNearestTeammateDistance(
                context,
                worldPosition);

        float opponentSpaceScore =
            Mathf.Min(
                nearestOpponentDistance,
                opponentAvoidanceDistance);

        float teammateSpacingScore =
            Mathf.Min(
                nearestTeammateDistance,
                teammateSpacingDistance);

        float rolePositionScore =
            ScoreRolePosition(
                context.Actor.PlayerRole,
                fieldPosition,
                ballFieldPosition);

        float formationLaneScore =
            ScoreFormationLane(
                context.FormationPosition,
                fieldPosition.x);

        float passLaneScore =
            IsPassingLaneBlocked(
                context,
                context.GameState.BallPosition,
                worldPosition)
                ? -4f
                : 3f;

        float crossPositionScore =
            ScoreCrossPosition(
                context,
                fieldPosition,
                ballFieldPosition);

        return opponentSpaceScore * 2f
            + teammateSpacingScore
            + rolePositionScore
            + formationLaneScore
            + passLaneScore
            + crossPositionScore;
    }

    /// <summary>
    /// Scores whether a candidate matches the actor's role relative to the
    /// ball.
    /// </summary>
    /// <param name="role">The actor's role.</param>
    /// <param name="candidate">
    /// The candidate team-relative position.
    /// </param>
    /// <param name="ballPosition">
    /// The ball's team-relative position.
    /// </param>
    /// <returns>The role-position score.</returns>
    private float ScoreRolePosition(
        EPlayerRole role,
        Vector2 candidate,
        Vector2 ballPosition)
    {
        float depthDifference =
            candidate.y - ballPosition.y;

        switch (role)
        {
            case EPlayerRole.Defender:
                return depthDifference <= 0f
                    ? 2f
                    : -2f;

            case EPlayerRole.Midfielder:
                return Mathf.Abs(depthDifference) <= 0.2f
                    ? 2f
                    : 0f;

            case EPlayerRole.Forward:
                return depthDifference >= 0f
                    ? 3f
                    : -1f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Scores how closely the candidate respects the actor's formation lane.
    /// </summary>
    /// <param name="formationPosition">
    /// The actor's formation position.
    /// </param>
    /// <param name="candidateHorizontalPosition">
    /// The candidate's normalized horizontal position.
    /// </param>
    /// <returns>The formation-lane score.</returns>
    private float ScoreFormationLane(
        EFormationPosition formationPosition,
        float candidateHorizontalPosition)
    {
        float expectedLane =
            GetFormationLane(formationPosition);

        float laneDifference =
            Mathf.Abs(
                expectedLane
                - candidateHorizontalPosition);

        return 1f - Mathf.Clamp01(laneDifference);
    }

    /// <summary>
    /// Scores a candidate's suitability for receiving a potential cross.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="candidate">
    /// The candidate team-relative position.
    /// </param>
    /// <param name="ballPosition">
    /// The ball's team-relative position.
    /// </param>
    /// <returns>The crossing-position score.</returns>
    private float ScoreCrossPosition(
        AIBehaviorContext context,
        Vector2 candidate,
        Vector2 ballPosition)
    {
        if (!ShouldPositionForCross(context, ballPosition))
            return 0f;

        bool crossesCenter =
            Mathf.Sign(candidate.x)
            != Mathf.Sign(ballPosition.x);

        float centrality =
            1f - Mathf.Abs(candidate.x);

        float attackingDepth =
            candidate.y;

        float score =
            centrality * 2f
            + attackingDepth * 2f;

        if (crossesCenter)
            score += 2f;

        return score;
    }

    /// <summary>
    /// Determines whether an opponent blocks the passing lane between two
    /// positions.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <param name="startPosition">The pass origin.</param>
    /// <param name="endPosition">The possible pass destination.</param>
    /// <returns>True when an opponent blocks the lane.</returns>
    private bool IsPassingLaneBlocked(
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
    /// Returns the distance from a position to the nearest opponent.
    /// </summary>
    private float GetNearestOpponentDistance(
        AIBehaviorContext context,
        Vector2 position)
    {
        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                GetOpposingTeam(context.Actor.TeamId));

        float nearestDistance = float.MaxValue;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null || !opponent.IsActive)
                continue;

            float distance =
                Vector2.Distance(
                    position,
                    opponent.Position);

            if (distance < nearestDistance)
                nearestDistance = distance;
        }

        return nearestDistance;
    }

    /// <summary>
    /// Returns the distance from a position to the nearest teammate.
    /// </summary>
    private float GetNearestTeammateDistance(
        AIBehaviorContext context,
        Vector2 position)
    {
        IReadOnlyList<IAIActor> teammates =
            context.GameState.GetTeamActors(
                context.Actor.TeamId);

        float nearestDistance = float.MaxValue;

        foreach (IAIActor teammate in teammates)
        {
            if (teammate == null
                || !teammate.IsActive
                || teammate.ActorId == context.Actor.ActorId)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    position,
                    teammate.Position);

            if (distance < nearestDistance)
                nearestDistance = distance;
        }

        return nearestDistance;
    }

    /// <summary>
    /// Returns the horizontal lane represented by a formation position.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position being evaluated.
    /// </param>
    /// <returns>
    /// Negative one for left, zero for center, and one for right.
    /// </returns>
    private float GetFormationLane(
        EFormationPosition formationPosition)
    {
        switch (formationPosition)
        {
            case EFormationPosition.LeftDefender:
            case EFormationPosition.LeftMidfielder:
            case EFormationPosition.LeftForward:
                return -1f;

            case EFormationPosition.RightDefender:
            case EFormationPosition.RightMidfielder:
            case EFormationPosition.RightForward:
                return 1f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Returns the side offering more width away from the ball.
    /// </summary>
    /// <param name="ballHorizontalPosition">
    /// The ball's normalized horizontal position.
    /// </param>
    /// <returns>Negative one for left or one for right.</returns>
    private float GetOpenSideDirection(
        float ballHorizontalPosition)
    {
        return ballHorizontalPosition >= 0f
            ? -1f
            : 1f;
    }

    /// <summary>
    /// Restricts a normalized field position to the playable field.
    /// </summary>
    /// <param name="position">
    /// The team-relative position being restricted.
    /// </param>
    /// <returns>The clamped team-relative position.</returns>
    private Vector2 ClampFieldPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, -1f, 1f),
            Mathf.Clamp01(position.y));
    }

    /// <summary>
    /// Returns the opposing team.
    /// </summary>
    /// <param name="teamId">The current team.</param>
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
    /// <param name="segmentStart">The segment's starting position.</param>
    /// <param name="segmentEnd">The segment's ending position.</param>
    /// <returns>The shortest distance from the point to the segment.</returns>
    private float DistanceFromPointToSegment(
        Vector2 point,
        Vector2 segmentStart,
        Vector2 segmentEnd)
    {
        Vector2 segment =
            segmentEnd - segmentStart;

        if (segment.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector2.Distance(
                point,
                segmentStart);
        }

        float projection =
            Vector2.Dot(
                point - segmentStart,
                segment)
            / segment.sqrMagnitude;

        projection = Mathf.Clamp01(projection);

        Vector2 closestPoint =
            segmentStart
            + segment * projection;

        return Vector2.Distance(
            point,
            closestPoint);
    }
}