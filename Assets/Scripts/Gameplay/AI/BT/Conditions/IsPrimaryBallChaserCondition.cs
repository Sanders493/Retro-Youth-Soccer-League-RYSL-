using UnityEngine;

/// <summary>
/// Checks whether the actor was selected as the team's primary ball chaser.
/// </summary>
[CreateAssetMenu(
    fileName = "Is Primary Ball Chaser",
    menuName = "Soccer AI/Nodes/Conditions/Is Primary Ball Chaser")]
public sealed class IsPrimaryBallChaserCondition : AIConditionNode
{
    /// <summary>
    /// Checks the ball-chaser selection stored in the context.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>True when this actor is the selected chaser.</returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        return context.Actor != null
               && context.Actor.IsActive
               && context.Actor.IsAIControlled
               && !context.Actor.HasBall
               && context.IsPrimaryBallChaser;
    }
}