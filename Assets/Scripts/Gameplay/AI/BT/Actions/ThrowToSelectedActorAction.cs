using UnityEngine;

/// <summary>
/// Creates a goalkeeper throw assignment toward the selected teammate.
/// </summary>
[CreateAssetMenu(
    fileName = "Throw To Selected Actor",
    menuName =
        "Soccer AI/Nodes/Actions/Throw To Selected Actor")]
public sealed class ThrowToSelectedActorAction :
    AIActionNode
{
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || !context.Actor.IsGoalkeeper
            || !context.Actor.HasBall
            || context.SelectedActor == null
            || !context.HasSelectedPosition)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Throw,
                context.SelectedPosition,
                context.SelectedActor.ActorId);

        return SetAssignment(
            context,
            assignment);
    }
}