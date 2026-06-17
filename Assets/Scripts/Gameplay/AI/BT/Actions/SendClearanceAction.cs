using UnityEngine;

/// <summary>
/// Creates a pass assignment toward the selected clearance position.
/// </summary>
[CreateAssetMenu(
    fileName = "Send Clearance",
    menuName = "Soccer AI/Nodes/Actions/Send Clearance")]
public sealed class SendClearanceAction :
    AIActionNode
{
    [SerializeField]
    private int priority;

    /// <summary>
    /// Creates a clearance assignment toward the selected position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when the clearance assignment is created.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || !context.HasSelectedPosition)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Pass,
                context.SelectedPosition,
                string.Empty,
                priority,
                nameof(SendClearanceAction));

        return SetAssignment(
            context,
            assignment);
    }
}