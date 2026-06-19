using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether opponents are blocking the actor's path to goal.
/// </summary>
[CreateAssetMenu(
    fileName = "Shot Lane Is Clear",
    menuName = "Soccer AI/Nodes/Conditions/Shot Lane Is Clear")]
public sealed class ShotLaneIsClearCondition :
    AIConditionNode
{
    [SerializeField]
    private float laneBlockRadius = 0.75f;

    /// <summary>
    /// Checks whether the direct shot lane to goal is clear.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when no active opponent blocks the shot lane.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        Vector2 start =
            context.Actor.Position;

        Vector2 end =
            context.GameState.GetAttackingGoalPosition(
                context.Actor.TeamId);

        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                GetOpposingTeam(
                    context.Actor.TeamId));

        if (opponents == null)
            return true;

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
                return false;
            }
        }

        return true;
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
        laneBlockRadius =
            Mathf.Max(
                0f,
                laneBlockRadius);
    }
#endif
}