using UnityEngine;

/// <summary>
/// Creates a pass assignment toward the selected teammate.
/// </summary>
[CreateAssetMenu(
    fileName = "Send Pass",
    menuName = "Soccer AI/Nodes/Actions/Send Pass")]
public sealed class SendPassAction :
    AIActionNode
{
    /// <summary>
    /// Creates a pass assignment using the selected actor and position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a valid pass assignment is created.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.SelectedActor == null
            || !context.HasSelectedPosition)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Pass,
                context.SelectedPosition,
                context.SelectedActor.ActorId,
                nameof(SendPassAction));

        return SetAssignment(
            context,
            assignment);
    }
}