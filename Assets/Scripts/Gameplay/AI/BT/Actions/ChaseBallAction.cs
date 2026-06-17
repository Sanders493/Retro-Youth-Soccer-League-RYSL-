using UnityEngine;

/// <summary>
/// Moves an actor toward a loose ball and attempts to take possession when
/// close enough.
/// </summary>
[CreateAssetMenu(
    fileName = "Chase Ball",
    menuName = "Soccer AI/Nodes/Actions/Chase Ball")]
public sealed class ChaseBallAction : AIActionNode
{
    [SerializeField] private float takeBallDistance = 0.75f;

    /// <summary>
    /// Creates a movement or take-ball assignment.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>Success when an assignment is created.</returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 ballPosition =
            context.GameState.BallPosition;

        float distanceSquared =
            (ballPosition - context.Actor.Position)
            .sqrMagnitude;

        float takeDistanceSquared =
            takeBallDistance * takeBallDistance;

        EAIActionType actionType =
            distanceSquared <= takeDistanceSquared
                ? EAIActionType.TakeBall
                : EAIActionType.Move;

        return SetAssignment(
            context,
            new ActorAssignment(
                context.Actor.ActorId,
                actionType,
                ballPosition));
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the take-ball distance to a non-negative value.
    /// </summary>
    private void OnValidate()
    {
        takeBallDistance =
            Mathf.Max(0f, takeBallDistance);
    }
#endif
}