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

    public ActorAssignment Assignment { get; set; }

    public IAIActor SelectedActor { get; set; }

    public Vector2 SelectedPosition { get; set; }

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
    public AIBehaviorContext(
        IAIActor actor,
        IGameState gameState,
        EFormationPosition formationPosition,
        ETeamAIState teamState,
        bool isPrimaryBallChaser)
    {
        Actor = actor;
        GameState = gameState;
        FormationPosition = formationPosition;
        TeamState = teamState;
        IsPrimaryBallChaser = isPrimaryBallChaser;
    }

    /// <summary>
    /// Clears transient results before evaluating the tree.
    /// </summary>
    public void ResetResults()
    {
        Assignment = null;
        SelectedActor = null;
        SelectedPosition = default;
    }
}