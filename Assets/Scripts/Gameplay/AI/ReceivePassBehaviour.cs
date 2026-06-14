using UnityEngine;

/// <summary>
/// Moves an intended pass receiver toward the pass destination.
/// </summary>
public sealed class ReceivePassBehavior : IAIBehavior
{
    private readonly float arrivalTolerance;
    private readonly int priority;

    /// <summary>
    /// Creates a pass-receiving behavior.
    /// </summary>
    /// <param name="arrivalTolerance">
    /// The distance at which the actor is considered to have reached the
    /// pass destination.
    /// </param>
    /// <param name="priority">
    /// The priority of receiving a pass.
    /// </param>
    public ReceivePassBehavior(
        float arrivalTolerance = 0.25f,
        int priority = 80)
    {
        this.arrivalTolerance =
            Mathf.Max(0f, arrivalTolerance);

        this.priority = priority;
    }

    /// <summary>
    /// Determines whether this actor is the intended receiver of an active
    /// pass.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when an active pass targets this actor.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        return context.GameState.IsMatchActive
            && context.Actor.IsActive
            && context.Actor.IsAIControlled
            && !context.Actor.HasBall
            && context.GameState.HasActivePass
            && context.GameState.IntendedPassReceiverId
                == context.Actor.ActorId;
    }

    /// <summary>
    /// Returns the priority of receiving a pass.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The configured behavior priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates an assignment that moves the intended receiver toward the
    /// pass destination.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting movement assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        Vector2 targetPosition =
            context.GameState.IntendedPassTargetPosition;

        float distanceSquared =
            (targetPosition - context.Actor.Position)
            .sqrMagnitude;

        float toleranceSquared =
            arrivalTolerance * arrivalTolerance;

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