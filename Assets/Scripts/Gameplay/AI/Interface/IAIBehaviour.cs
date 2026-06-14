/// <summary>
/// Defines a behavior that can produce an assignment for an AI-controlled actor.
/// </summary>
public interface IAIBehavior
{
    /// <summary>
    /// Determines whether the behavior can currently execute.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>
    /// True when the behavior can execute; otherwise, false.
    /// </returns>
    bool CanExecute(AIBehaviorContext context);

    /// <summary>
    /// Returns the priority of the behavior in the current context.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The behavior priority.</returns>
    int GetPriority(AIBehaviorContext context);

    /// <summary>
    /// Creates the actor assignment produced by the behavior.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The resulting actor assignment.</returns>
    ActorAssignment CreateAssignment(AIBehaviorContext context);
}