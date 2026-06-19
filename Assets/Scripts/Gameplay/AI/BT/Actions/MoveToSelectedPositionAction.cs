using UnityEngine;

/// <summary>
/// Moves an actor toward a position selected by an earlier behavior-tree node.
/// </summary>
[CreateAssetMenu(
    fileName = "Move To Selected Position",
    menuName = "Soccer AI/Nodes/Actions/Move To Selected Position")]
public sealed class MoveToSelectedPositionAction :
    AIActionNode
{
    [SerializeField] private float arrivalTolerance = 0.25f;

    /// <summary>
    /// Creates a movement assignment using the selected context position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a movement or hold-position assignment is created.
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

        float distanceSquared =
            (context.SelectedPosition
             - context.Actor.Position).sqrMagnitude;

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
                context.SelectedPosition);

        return SetAssignment(
            context,
            assignment);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the arrival tolerance to a valid value.
    /// </summary>
    private void OnValidate()
    {
        arrivalTolerance =
            Mathf.Max(0f, arrivalTolerance);
    }
#endif
}