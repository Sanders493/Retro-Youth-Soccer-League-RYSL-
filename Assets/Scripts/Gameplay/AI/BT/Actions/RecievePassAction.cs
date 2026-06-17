using UnityEngine;

/// <summary>
/// Moves an intended pass receiver toward the pass destination.
/// </summary>
[CreateAssetMenu(
    fileName = "Receive Pass",
    menuName = "Soccer AI/Nodes/Actions/Receive Pass")]
public sealed class ReceivePassAction : AIActionNode
{
    [SerializeField] private float arrivalTolerance = 0.25f;

    /// <summary>
    /// Creates an assignment that moves the actor toward the active pass
    /// destination.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>Success when a receiving assignment is created.</returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.GameState.HasActivePass)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 targetPosition =
            context.GameState.IntendedPassTargetPosition;

        float distanceSquared =
            (targetPosition - context.Actor.Position)
            .sqrMagnitude;

        float toleranceSquared =
            arrivalTolerance * arrivalTolerance;

        EAIActionType actionType =
            distanceSquared <= toleranceSquared
                ? EAIActionType.HoldPosition
                : EAIActionType.Move;

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                actionType,
                targetPosition);

        return SetAssignment(
            context,
            assignment);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the arrival tolerance to a non-negative value.
    /// </summary>
    private void OnValidate()
    {
        arrivalTolerance =
            Mathf.Max(0f, arrivalTolerance);
    }
#endif
}