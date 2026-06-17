using UnityEngine;

/// <summary>
/// Checks whether an opposing actor currently controls the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Opponent Has Ball",
    menuName = "Soccer AI/Nodes/Conditions/Opponent Has Ball")]
public sealed class OpponentHasBallCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the ball owner belongs to the opposing team.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when an opponent currently owns the ball.
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

        IAIActor ballOwner =
            context.GameState.BallOwner;

        return ballOwner != null
               && ballOwner.TeamId
               != context.Actor.TeamId;
    }
}