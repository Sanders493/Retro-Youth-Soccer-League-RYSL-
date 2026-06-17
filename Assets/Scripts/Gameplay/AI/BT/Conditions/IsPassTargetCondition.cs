using UnityEngine;

/// <summary>
/// Checks whether the actor is the intended receiver of an active pass.
/// </summary>
[CreateAssetMenu(
    fileName = "Is Pass Target",
    menuName = "Soccer AI/Nodes/Conditions/Is Pass Target")]
public sealed class IsPassTargetCondition : AIConditionNode
{
    /// <summary>
    /// Checks whether the current active pass targets this actor.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>True when the actor is the intended pass receiver.</returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.GameState.HasActivePass)
        {
            return false;
        }

        string receiverId =
            context.GameState.IntendedPassReceiverId;

        if (string.IsNullOrWhiteSpace(
                receiverId))
        {
            return false;
        }

        return receiverId
               == context.Actor.ActorId;
    }
}