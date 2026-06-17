using UnityEngine;

/// <summary>
/// Moves an actor toward the current ball position.
/// </summary>
[CreateAssetMenu(
    fileName = "Move To Ball",
    menuName = "Soccer AI/Nodes/Actions/Move To Ball")]
public sealed class MoveToBallAction :
    AIActionNode
{
    /// <summary>
    /// Creates an assignment that moves the actor toward the ball.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a movement assignment is created.
    /// </returns>
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
            GetBallApproachPosition(
                context);

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Move,
                targetPosition,
                behaviorName: DisplayName);

        return SetAssignment(
            context,
            assignment);
    }

    /// <summary>
    /// Gets a position from which the actor can approach and steal the ball.
    /// </summary>
    private Vector2 GetBallApproachPosition(
        AIBehaviorContext context)
    {
        Vector2 ballPosition =
            context.GameState.BallPosition;

        IAIActor ballOwner =
            context.GameState.BallOwner;

        if (ballOwner == null)
            return ballPosition;

        Vector2 ownerToBall =
            ballPosition
            - ballOwner.Position;

        if (ownerToBall.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return ballPosition;
        }

        const float approachOffset = 0.35f;

        return ballPosition
               + ownerToBall.normalized
               * approachOffset;
    }
}