using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether the ball carrier has sufficient open space to advance.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball Carrier Can Advance",
    menuName = "Soccer AI/Nodes/Conditions/Ball Carrier Can Advance")]
public sealed class BallCarrierCanAdvanceCondition :
    AIConditionNode
{
    [Tooltip(
        "How far ahead of the actor opponents are checked.")]
    [SerializeField]
    private float forwardCheckDistance = 3f;

    [Tooltip(
        "The half-width of the forward movement corridor.")]
    [SerializeField]
    private float corridorHalfWidth = 1.25f;

    /// <summary>
    /// Checks whether no opponent occupies the actor's immediate forward
    /// movement corridor.
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

        Vector2 startFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        ETeamId opposingTeam =
            context.Actor.TeamId == ETeamId.PlayerTeam
                ? ETeamId.EnemyTeam
                : ETeamId.PlayerTeam;

        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                opposingTeam);

        if (opponents == null)
            return true;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive)
            {
                continue;
            }

            Vector2 opponentFieldPosition =
                context.GameState.GetTeamRelativeFieldPosition(
                    context.Actor.TeamId,
                    opponent.Position);

            float forwardDistance =
                opponentFieldPosition.x
                - startFieldPosition.x;

            if (forwardDistance <= 0f
                || forwardDistance > forwardCheckDistance)
            {
                continue;
            }

            float lateralDistance =
                Mathf.Abs(
                    opponentFieldPosition.y
                    - startFieldPosition.y);

            if (lateralDistance
                <= corridorHalfWidth)
            {
                return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        forwardCheckDistance =
            Mathf.Max(
                0f,
                forwardCheckDistance);

        corridorHalfWidth =
            Mathf.Max(
                0f,
                corridorHalfWidth);
    }
#endif
}