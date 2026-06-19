using UnityEngine;

/// <summary>
/// Moves an actor toward its assigned formation position.
/// </summary>
[CreateAssetMenu(
    fileName = "Maintain Formation",
    menuName = "Soccer AI/Nodes/Actions/Maintain Formation")]
public sealed class MaintainFormationAction : AIActionNode
{
    [SerializeField] private float arrivalTolerance = 0.25f;

    /// <summary>
    /// Creates a movement or hold-position assignment for the actor's
    /// formation location.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>Success when a formation assignment is produced.</returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 targetPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        float toleranceSquared =
            arrivalTolerance * arrivalTolerance;

        EAIActionType actionType =
            (targetPosition - context.Actor.Position).sqrMagnitude
            <= toleranceSquared
                ? EAIActionType.HoldPosition
                : EAIActionType.Move;

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                actionType,
                targetPosition);

        return SetAssignment(context, assignment);
    }
}