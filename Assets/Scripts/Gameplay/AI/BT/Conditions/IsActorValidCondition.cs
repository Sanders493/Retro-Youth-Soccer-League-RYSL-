using UnityEngine;

/// <summary>
/// Checks whether the actor can currently receive an AI assignment.
/// </summary>
[CreateAssetMenu(
    fileName = "Is Actor Valid",
    menuName = "Soccer AI/Nodes/Conditions/Is Actor Valid")]
public sealed class IsActorValidCondition : AIConditionNode
{
    /// <summary>
    /// Checks whether the actor is active and AI-controlled during a match.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>True when the actor can be controlled.</returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        return context.Actor != null
               && context.GameState != null
               && context.GameState.IsMatchActive
               && context.Actor.IsActive
               && context.Actor.IsAIControlled;
    }
}