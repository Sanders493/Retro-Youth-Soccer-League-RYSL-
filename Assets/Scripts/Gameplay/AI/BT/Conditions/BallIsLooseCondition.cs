using UnityEngine;

/// <summary>
/// Checks whether the ball currently has no owner.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball Is Loose",
    menuName = "Soccer AI/Nodes/Conditions/Ball Is Loose")]
public sealed class BallIsLooseCondition : AIConditionNode
{
    /// <summary>
    /// Checks whether the match is active and the ball has no owner.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>True when the ball is loose.</returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        return context.GameState != null
               && context.GameState.IsMatchActive
               && !context.GameState.HasBallOwner;
    }
}