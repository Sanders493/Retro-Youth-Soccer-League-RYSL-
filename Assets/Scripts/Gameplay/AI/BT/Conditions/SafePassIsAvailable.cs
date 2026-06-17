using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether the actor currently has at least one safe teammate to
/// whom the ball can be passed.
/// </summary>
[CreateAssetMenu(
    fileName = "Safe Pass Is Available",
    menuName = "Soccer AI/Nodes/Conditions/Safe Pass Is Available")]
public sealed class SafePassIsAvailableCondition :
    AIConditionNode
{
    [Header("Pass Range")]
    [SerializeField]
    private float minimumPassDistance = 1.5f;

    [SerializeField]
    private float maximumPassDistance = 10f;

    [Header("Passing Lane")]
    [SerializeField]
    private float laneBlockRadius = 0.75f;

    /// <summary>
    /// Checks whether any active teammate is within range and has an
    /// unobstructed passing lane.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.Actor.HasBall)
        {
            return false;
        }

        IReadOnlyList<IAIActor> teammates =
            context.GameState.GetTeamActors(
                context.Actor.TeamId);

        if (teammates == null)
            return false;

        foreach (IAIActor teammate in teammates)
        {
            if (!IsValidTeammate(
                    context,
                    teammate))
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    context.Actor.Position,
                    teammate.Position);

            if (distance < minimumPassDistance
                || distance > maximumPassDistance)
            {
                continue;
            }

            if (!IsPassingLaneBlocked(
                    context,
                    context.Actor.Position,
                    teammate.Position))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether an actor is eligible to receive a pass.
    /// </summary>
    private bool IsValidTeammate(
        AIBehaviorContext context,
        IAIActor teammate)
    {
        return teammate != null
            && teammate.IsActive
            && teammate.TeamId
                == context.Actor.TeamId
            && teammate.ActorId
                != context.Actor.ActorId;
    }

    /// <summary>
    /// Checks whether an opponent obstructs the passing segment.
    /// </summary>
    private bool IsPassingLaneBlocked(
        AIBehaviorContext context,
        Vector2 start,
        Vector2 end)
    {
        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                GetOpposingTeam(
                    context.Actor.TeamId));

        if (opponents == null)
            return false;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive)
            {
                continue;
            }

            float distanceToLane =
                GetDistanceToSegment(
                    opponent.Position,
                    start,
                    end);

            if (distanceToLane <= laneBlockRadius)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the shortest distance between a point and a line segment.
    /// </summary>
    private float GetDistanceToSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment =
            end - start;

        float segmentLengthSquared =
            segment.sqrMagnitude;

        if (segmentLengthSquared
            <= Mathf.Epsilon)
        {
            return Vector2.Distance(
                point,
                start);
        }

        float interpolation =
            Vector2.Dot(
                point - start,
                segment)
            / segmentLengthSquared;

        interpolation =
            Mathf.Clamp01(
                interpolation);

        Vector2 closestPoint =
            start
            + segment * interpolation;

        return Vector2.Distance(
            point,
            closestPoint);
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    private ETeamId GetOpposingTeam(
        ETeamId currentTeam)
    {
        return currentTeam == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts pass-evaluation values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        minimumPassDistance =
            Mathf.Max(
                0f,
                minimumPassDistance);

        maximumPassDistance =
            Mathf.Max(
                minimumPassDistance,
                maximumPassDistance);

        laneBlockRadius =
            Mathf.Max(
                0f,
                laneBlockRadius);
    }
#endif
}