using UnityEngine;

/// <summary>
/// Checks whether the evaluating actor currently controls the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Actor Has Ball",
    menuName = "Soccer AI/Nodes/Conditions/Actor Has Ball")]
public sealed class ActorHasBallCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the actor is the current ball owner.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the actor currently owns the ball.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        IAIActor ballOwner =
            context.GameState.BallOwner;

        return ballOwner != null
               && ballOwner.ActorId
               == context.Actor.ActorId;
    }
}