using UnityEngine;

/// <summary>
/// Checks whether the goalkeeper may legally handle the current ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Goalkeeper Can Handle Ball",
    menuName =
        "Soccer AI/Nodes/Conditions/Goalkeeper Can Handle Ball")]
public sealed class GoalkeeperCanHandleBallCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the ball is inside the goalkeeper's box and was not
    /// last touched by a teammate.
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

        if (!insideOwnBox)
            return false;

        return context.GameState.LastTouchTeam
               != context.Actor.TeamId;
    }
}