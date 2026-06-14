/// <summary>
/// Moves the selected AI-controlled actor toward the ball.
/// </summary>
public sealed class ChaseBallBehavior : IAIBehavior
{
    private readonly int priority;

    /// <summary>
    /// Creates a ball-chasing behavior.
    /// </summary>
    /// <param name="priority">
    /// The priority of chasing the ball relative to other behaviors.
    /// </param>
    public ChaseBallBehavior(int priority = 50)
    {
        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor should currently chase the ball.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>
    /// True when the actor is active, selected as the primary chaser,
    /// and does not already possess the ball.
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
    /// Returns the priority of the chase behavior.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The configured chase priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates an assignment that moves the actor toward the ball.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The resulting movement assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            context.GameState.BallPosition,
            priority: priority);
    }
}