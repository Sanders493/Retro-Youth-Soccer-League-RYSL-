using UnityEngine;

/// <summary>
/// Moves an AI-controlled actor toward its assigned formation position.
/// </summary>
public sealed class FormationBehavior : IAIBehavior
{
    private readonly float positionTolerance;
    private readonly int priority;

    /// <summary>
    /// Creates a formation-following behavior.
    /// </summary>
    /// <param name="positionTolerance">
    /// The distance at which the actor is considered to have reached its
    /// formation position.
    /// </param>
    /// <param name="priority">
    /// The priority returned while the behavior is available.
    /// </param>
    public FormationBehavior(
        float positionTolerance = 0.1f,
        int priority = 0)
    {
        this.positionTolerance = Mathf.Max(0f, positionTolerance);
        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor can follow its formation position.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>
    /// True when the match is active and the actor is active; otherwise,
    /// false.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        return context != null
            && context.Actor != null
            && context.GameState != null
            && context.Actor.IsActive
            && context.GameState.IsMatchActive;
    }

    /// <summary>
    /// Returns the priority of formation following.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The configured formation behavior priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates an assignment that moves the actor toward its formation
    /// position or holds it in place when it has arrived.
    /// </summary>
    /// <param name="context">The current behavior context.</param>
    /// <returns>The resulting formation assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        Vector2 targetPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        float distanceSquared =
            (targetPosition - context.Actor.Position).sqrMagnitude;

        float toleranceSquared =
            positionTolerance * positionTolerance;

        if (distanceSquared <= toleranceSquared)
        {
            return new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.HoldPosition,
                targetPosition,
                priority: priority);
        }

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            targetPosition,
            priority: priority);
    }
}
