using UnityEngine;

/// <summary>
/// Creates a clearance assignment toward the selected field position.
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
                EAIActionType.Clear,
                context.SelectedPosition,
                string.Empty,
                nameof(SendClearanceAction));

        return SetAssignment(
            context,
            assignment);
    }
}