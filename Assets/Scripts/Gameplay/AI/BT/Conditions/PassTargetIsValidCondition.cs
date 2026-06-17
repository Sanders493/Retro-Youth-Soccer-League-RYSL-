using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether the currently selected pass target remains valid.
/// </summary>
[CreateAssetMenu(
    fileName = "Pass Target Is Valid",
    menuName = "Soccer AI/Nodes/Conditions/Pass Target Is Valid")]
public sealed class PassTargetIsValidCondition :
    AIConditionNode
{
    [SerializeField]
    private float minimumPassDistance = 1.5f;

    [SerializeField]
    private float maximumPassDistance = 10f;

    [SerializeField]
    private float laneBlockRadius = 0.75f;

    /// <summary>
    /// Checks whether the selected teammate remains eligible to receive the
    /// pass.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the selected pass target remains valid.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || context.SelectedActor == null)
        {
            return false;
        }

        IAIActor target =
            context.SelectedActor;

        if (!target.IsActive
            || target.TeamId
                != context.Actor.TeamId
            || target.ActorId
                == context.Actor.ActorId)
        {
            return false;
        }

        float distance =
            Vector2.Distance(
                context.Actor.Position,
                target.Position);

        if (distance < minimumPassDistance
            || distance > maximumPassDistance)
        {
            return false;
        }

        if (IsPassingLaneBlocked(
                context,
                context.Actor.Position,
                target.Position))
        {
            return false;
        }

        context.SelectedPosition =
            target.Position;

        context.HasSelectedPosition =
            true;

        return true;
    }

    /// <summary>
    /// Checks whether an opponent blocks the selected passing lane.
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

            if (GetDistanceToSegment(
                    opponent.Position,
                    start,
                    end)
                <= laneBlockRadius)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the distance between a point and a line segment.
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
        ETeamId team)
    {
        return team == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured values to valid ranges.
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