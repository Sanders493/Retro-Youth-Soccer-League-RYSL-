using UnityEngine;

/// <summary>
/// Creates a shot assignment toward the selected goal position.
/// </summary>
[CreateAssetMenu(
    fileName = "Send Shot",
    menuName = "Soccer AI/Nodes/Actions/Send Shot")]
public sealed class SendShotAction :
    AIActionNode
{
    [SerializeField]
    private int priority;

    /// <summary>
    /// Creates a shot assignment toward the selected position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a valid shot assignment is created.
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
                EAIActionType.Shoot,
                context.SelectedPosition,
                string.Empty,
                priority,
                nameof(SendShotAction));

        return SetAssignment(
            context,
            assignment);
    }
}