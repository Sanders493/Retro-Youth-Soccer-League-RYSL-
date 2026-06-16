using UnityEngine;

/// <summary>
/// Represents a match participant that can be observed by the AI system.
/// </summary>
public interface IAIActor
{
    string ActorId { get; }

    ETeamId TeamId { get; }

    Vector2 Position { get; set;}


    bool IsActive { get; }

    bool IsAIControlled { get; }

    bool IsGoalkeeper { get; }

    bool HasBall { get; }
    
    EPlayerRole PlayerRole { get; }
    EFormationPosition FormationPosition { get; }
    
    /// <summary>
    /// Gets the gameplay action output used to control this actor.
    /// </summary>
    IAIActionOutput ActionOutput { get; }
}