using UnityEngine;

/// <summary>
/// Checks whether the actor is the team's selected pressure defender.
/// </summary>
[CreateAssetMenu(
    fileName = "Actor Is Primary Defender",
    menuName = "Soccer AI/Nodes/Conditions/Actor Is Primary Defender")]
public sealed class ActorIsPrimaryDefenderCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether this actor should pressure the opposing ball owner.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when this actor is the primary defender.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        return context != null
               && context.Actor != null
               && context.IsPrimaryDefender;
    }
}