using UnityEngine;

/// <summary>
/// Provides shared runtime information while evaluating an actor's behavior
/// tree.
/// </summary>
public sealed class AIBehaviorContext
{
    public IAIActor Actor { get; }

    public IGameState GameState { get; }

    public EFormationPosition FormationPosition { get; }

    public ETeamAIState TeamState { get; }

    public bool IsPrimaryBallChaser { get; }

    public bool IsPrimaryDefender { get; }
    public ActorAssignment Assignment { get; set; }

    public IAIActor SelectedActor { get; set; }

    public Vector2 SelectedPosition { get; set; }
    
    public bool HasSelectedPosition
    {
        get;
        set;
    }

    /// <summary>
    /// Creates a behavior-tree context for one actor.
    /// </summary>
    /// <param name="actor">The actor being evaluated.</param>
    /// <param name="gameState">The current read-only match state.</param>
    /// <param name="formationPosition">
    /// The formation position assigned to the actor.
    /// </param>
    /// <param name="teamState">The team's current tactical state.</param>
    /// <param name="isPrimaryBallChaser">
    /// Whether this actor was selected to pursue the loose ball.
    /// </param>
    /// <param name="isPrimaryDefender">d</param>
    public AIBehaviorContext(
        IAIActor actor,
        GameState gameState,
        EFormationPosition formationPosition,
        ETeamAIState teamState,
        bool isPrimaryBallChaser,
        bool isPrimaryDefender)
    {
        Actor = actor;
        GameState = gameState;
        FormationPosition = formationPosition;
        TeamState = teamState;
        IsPrimaryBallChaser = isPrimaryBallChaser;
        IsPrimaryDefender = isPrimaryDefender;
    }

    /// <summary>
    /// Clears transient values produced during the previous tree evaluation.
    /// </summary>
    public void ResetResults()
    {
        Assignment = null;
        SelectedActor = null;
        SelectedPosition = default;
        HasSelectedPosition = false;
    }
}