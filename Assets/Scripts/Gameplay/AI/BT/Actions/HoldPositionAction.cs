using UnityEngine;

/// <summary>
/// Stops the actor and leaves it at its current position.
/// </summary>
[CreateAssetMenu(
    fileName = "Hold Position",
    menuName = "Soccer AI/Nodes/Actions/Hold Position")]
public sealed class HoldPositionAction :
    AIBehaviorTreeNode
{
    /// <summary>
    /// Stops the actor's movement.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>Success after requesting no movement.</returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.Actor.ActionOutput == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        context.Actor.ActionOutput.RequestStop(context.Actor.ActorId);

        return EBehaviorTreeResult.Success;
    }
}