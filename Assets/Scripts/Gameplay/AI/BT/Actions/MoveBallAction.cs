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
            || context.GameState == null
            || context.GameState.HasBallOwner)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Move,
                context.GameState.BallPosition,
                behaviorName: DisplayName);

        return SetAssignment(
            context,
            assignment);
    }
}