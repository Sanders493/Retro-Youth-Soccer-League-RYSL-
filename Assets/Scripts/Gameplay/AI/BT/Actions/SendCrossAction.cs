using UnityEngine;

/// <summary>
/// Creates a pass assignment toward the selected cross receiver.
/// </summary>
[CreateAssetMenu(
    fileName = "Send Cross",
    menuName = "Soccer AI/Nodes/Actions/Send Cross")]
public sealed class SendCrossAction :
    AIActionNode
{
    [SerializeField]
    private int priority;

    /// <summary>
    /// Creates a cross assignment toward the selected actor and position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a cross assignment is created.
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
                nameof(SendCrossAction));

        return SetAssignment(
            context,
            assignment);
    }
}