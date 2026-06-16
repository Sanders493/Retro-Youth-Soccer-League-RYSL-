using UnityEngine;

/// <summary>
/// Moves the selected AI actor toward the ball and attempts to take
/// possession when close enough.
/// </summary>
public sealed class ChaseBallBehavior : IAIBehavior
{
    private readonly float takeBallDistance;
    private readonly int priority;

    /// <summary>
    /// Creates a ball-chasing behavior.
    /// </summary>
    /// <param name="takeBallDistance">
    /// The maximum distance at which the actor attempts to take the ball.
    /// </param>
    /// <param name="priority">
    /// The priority of chasing the ball relative to other behaviors.
    /// </param>
    public ChaseBallBehavior(
        float takeBallDistance = 0.75f,
        int priority = 50)
    {
        this.takeBallDistance =
            Mathf.Max(0f, takeBallDistance);

        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor should currently chase the ball.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>
    /// True when the actor is the primary ball chaser and does not possess
    /// the ball.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        return context != null
            && context.Actor != null
            && context.GameState != null
            && context.GameState.IsMatchActive
            && context.Actor.IsActive
            && context.Actor.IsAIControlled
            && !context.Actor.HasBall
            && context.IsPrimaryBallChaser;
    }

    /// <summary>
    /// Returns the configured behavior priority.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The chase-ball priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Moves toward the ball or attempts to take it when in range.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The resulting assignment.</returns>
    public ActorAssignment CreateAssignment(
        AIBehaviorContext context)
    {
        float distanceSquared =
            (context.GameState.BallPosition -
             context.Actor.Position).sqrMagnitude;

        float takeDistanceSquared =
            takeBallDistance * takeBallDistance;

        if (distanceSquared <= takeDistanceSquared)
        {
            return new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.TakeBall,
                context.GameState.BallPosition,
                priority: priority);
        }

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            context.GameState.BallPosition,
            priority: priority);
    }
}