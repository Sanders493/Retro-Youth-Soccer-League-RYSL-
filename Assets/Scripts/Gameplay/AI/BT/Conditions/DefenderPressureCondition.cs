using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether enough opposing defenders are within pressure distance of
/// the actor currently controlling the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Defender Pressure",
    menuName = "Soccer AI/Nodes/Conditions/Defender Pressure")]
public sealed class DefenderPressureCondition :
    AIConditionNode
{
    [Tooltip(
        "Maximum world-space distance at which a defender applies pressure.")]
    [SerializeField, Min(0f)]
    private float pressureDistance = 3f;

    [Tooltip(
        "The minimum number of nearby defenders required.")]
    [SerializeField, Min(1)]
    private int requiredDefenderCount = 1;

    /// <summary>
    /// Checks whether the ball-owning actor is under defensive pressure.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the required number of opposing defenders are within the
    /// configured pressure distance.
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

        if (context.GameState.BallOwner == null
            || context.GameState.BallOwner.ActorId
                != context.Actor.ActorId)
        {
            return false;
        }

        ETeamId opposingTeam =
            GetOpposingTeam(
                context.Actor.TeamId);

        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                opposingTeam);

        if (opponents == null)
            return false;

        int nearbyDefenderCount = 0;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive
                || opponent.PlayerRole
                    != EPlayerRole.Defender)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    context.Actor.Position,
                    opponent.Position);

            if (distance > pressureDistance)
                continue;

            nearbyDefenderCount++;

            if (nearbyDefenderCount
                >= requiredDefenderCount)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the team opposing the supplied team.
    /// </summary>
    /// <param name="team">
    /// The actor's team.
    /// </param>
    /// <returns>The opposing team.</returns>
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
        pressureDistance =
            Mathf.Max(
                0f,
                pressureDistance);

        requiredDefenderCount =
            Mathf.Max(
                1,
                requiredDefenderCount);
    }
#endif
}