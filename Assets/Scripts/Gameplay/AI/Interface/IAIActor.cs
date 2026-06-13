using UnityEngine;

/// <summary>
/// Represents a match participant that can be observed by the AI system.
/// </summary>
public interface IAIActor
{
    string ActorId { get; }

    ETeamId ETeamId { get; }

    Vector2 Position { get; }

    Vector2 Velocity { get; }

    bool IsActive { get; }

    bool IsAIControlled { get; }

    bool IsGoalkeeper { get; }

    bool HasBall { get; }
}