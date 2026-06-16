/// <summary>
/// Provides the information required to evaluate an AI behavior for one actor.
/// </summary>
public sealed class AIBehaviorContext
{
    public IAIActor Actor { get; }

    public IGameState GameState { get; }

    public EFormationPosition FormationPosition { get; }

    public bool IsPrimaryBallChaser { get; }

    /// <summary>
    /// Creates a behavior evaluation context.
    /// </summary>
    /// <param name="actor">The actor being evaluated.</param>
    /// <param name="gameState">The current read-only game state.</param>
    /// <param name="formationPosition">
    /// The formation position assigned to the actor.
    /// </param>
    /// <param name="isPrimaryBallChaser">
    /// Whether this actor has been selected to chase the ball.
    /// </param>
    public AIBehaviorContext(
        IAIActor actor,
        IGameState gameState,
        EFormationPosition formationPosition,
        bool isPrimaryBallChaser)
    {
        Actor = actor;
        GameState = gameState;
        FormationPosition = formationPosition;
        IsPrimaryBallChaser = isPrimaryBallChaser;
    }
}