using UnityEngine;

/// <summary>
/// Creates a goalkeeper dive assignment toward the selected position.
/// </summary>
[CreateAssetMenu(
    fileName = "Dive Action",
    menuName =
        "Soccer AI/Nodes/Actions/Dive Action")]
public sealed class DiveAction :
    AIActionNode
{
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || !context.Actor.IsGoalkeeper
            || !context.HasSelectedPosition)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Dive,
                context.SelectedPosition);

        return SetAssignment(
            context,
            assignment);
    }
}