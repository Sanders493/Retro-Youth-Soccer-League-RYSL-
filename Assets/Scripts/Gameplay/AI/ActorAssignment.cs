using UnityEngine;

/// <summary>
/// Describes the action currently assigned to one AI-controlled actor.
/// </summary>
public sealed class ActorAssignment
{
    public string ActorId { get; }

    public EAIActionType ActionType { get; }

    public Vector2 TargetPosition { get; }

    public string TargetActorId { get; }

    public int Priority { get; }

    /// <summary>
    /// Creates an assignment for an AI-controlled actor.
    /// </summary>
    /// <param name="actorId">The identifier of the assigned actor.</param>
    /// <param name="actionType">The action the actor should perform.</param>
    /// <param name="targetPosition">The target world position for the action.</param>
    /// <param name="targetActorId">The optional identifier of a targeted actor.</param>
    /// <param name="priority">The assignment priority.</param>
    public ActorAssignment(
        string actorId,
        EAIActionType actionType,
        Vector2 targetPosition,
        string targetActorId = null,
        int priority = 0)
    {
        ActorId = actorId;
        ActionType = actionType;
        TargetPosition = targetPosition;
        TargetActorId = targetActorId;
        Priority = priority;
    }
}