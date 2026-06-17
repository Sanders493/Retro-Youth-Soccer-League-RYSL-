using UnityEngine;

/// <summary>
/// Checks whether the goalkeeper must control the ball with their feet
/// instead of handling it.
/// </summary>
[CreateAssetMenu(
    fileName = "Goalkeeper Must Use Feet",
    menuName =
        "Soccer AI/Nodes/Conditions/Goalkeeper Must Use Feet")]
public sealed class GoalkeeperMustUseFeetCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the ball is outside the box or was last touched by a
    /// teammate.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.Actor.IsGoalkeeper)
        {
            return false;
        }

        bool insideOwnBox =
            context.GameState.IsInsideDefendingPenaltyBox(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        bool teammateTouchedLast =
            context.GameState.LastTouchTeam
            == context.Actor.TeamId;

        return !insideOwnBox
               || teammateTouchedLast;
    }
}