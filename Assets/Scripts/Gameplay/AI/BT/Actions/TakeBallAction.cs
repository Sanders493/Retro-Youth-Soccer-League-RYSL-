using UnityEngine;

/// <summary>
/// Requests that an actor take possession of an available or
/// opponent-controlled ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Take Ball",
    menuName = "Soccer AI/Nodes/Actions/Take Ball")]
public sealed class TakeBallAction :
    AIActionNode
{
    /// <summary>
    /// Creates a take-ball assignment when the ball is loose or controlled
    /// by an opponent.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a take-ball assignment is created.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || context.Actor.HasBall)
        {
            return EBehaviorTreeResult.Failure;
        }

        IAIActor ballOwner =
            context.GameState.BallOwner;

        if (ballOwner != null
            && ballOwner.TeamId
            == context.Actor.TeamId)
        {
            return EBehaviorTreeResult.Failure;
        }

        ActorAssignment assignment =
            new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.TakeBall,
                context.GameState.BallPosition,
                behaviorName: DisplayName);

        return SetAssignment(
            context,
            assignment);
    }
}