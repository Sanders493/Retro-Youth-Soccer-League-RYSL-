using System.Collections.Generic;

/// <summary>
/// Coordinates the AI-controlled actors belonging to one team.
/// </summary>
public sealed class TeamAIController
{
    private readonly ETeamId teamId;
    private readonly IGameState gameState;
    private readonly Dictionary<string, AIActorController> actorControllers = new();

    public ETeamAIState CurrentState { get; private set; }

    public TeamDecision CurrentDecision { get; private set; }

    /// <summary>
    /// Creates a controller for the specified team.
    /// </summary>
    /// <param name="teamId">The team controlled by this controller.</param>
    /// <param name="gameState">The read-only source of match information.</param>
    /// <param name="actionOutput">
    /// The interface used to send requests to external gameplay systems.
    /// </param>
    public TeamAIController(
        ETeamId teamId,
        IGameState gameState,
        IAIActionOutput actionOutput)
    {
        this.teamId = teamId;
        this.gameState = gameState;

        IReadOnlyList<IAIActor> actors = gameState.GetTeamActors(teamId);

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsAIControlled)
                continue;

            actorControllers.Add(
                actor.ActorId,
                new AIActorController(actor.ActorId, actionOutput));
        }
    }

    /// <summary>
    /// Evaluates the current match information and updates the team.
    /// </summary>
    public void UpdateTeam()
    {
        CurrentState = DetermineTeamState();
        CurrentDecision = CreateTeamDecision(CurrentState);

        ApplyDecision(CurrentDecision);
        ExecuteAssignments();
    }

    /// <summary>
    /// Determines the tactical state of the controlled team.
    /// </summary>
    /// <returns>The team's current tactical state.</returns>
    private ETeamAIState DetermineTeamState()
    {
        if (!gameState.IsMatchActive)
            return ETeamAIState.Stopped;

        if (gameState.HasPossession(teamId))
            return ETeamAIState.Attacking;

        if (gameState.TeamInPossession == ETeamId.None)
            return ETeamAIState.Transitioning;

        return ETeamAIState.Defending;
    }

    /// <summary>
    /// Creates actor assignments based on the team's tactical state.
    /// </summary>
    /// <param name="teamState">The tactical state being evaluated.</param>
    /// <returns>The resulting team decision.</returns>
    private TeamDecision CreateTeamDecision(ETeamAIState teamState)
    {
        TeamDecision decision = new(teamState);
        IReadOnlyList<IAIActor> actors = gameState.GetTeamActors(teamId);

        IAIActor closestActor = FindClosestAIActorToBall(actors);

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsActive || !actor.IsAIControlled)
                continue;

            ActorAssignment assignment = CreateAssignment(
                actor,
                closestActor,
                teamState);

            decision.AddAssignment(assignment);
        }

        return decision;
    }

    /// <summary>
    /// Creates an assignment for one actor.
    /// </summary>
    /// <param name="actor">The actor receiving the assignment.</param>
    /// <param name="closestActor">The actor closest to the ball.</param>
    /// <param name="teamState">The team's current tactical state.</param>
    /// <returns>The assignment created for the actor.</returns>
    private ActorAssignment CreateAssignment(
        IAIActor actor,
        IAIActor closestActor,
        ETeamAIState teamState)
    {
        if (teamState == ETeamAIState.Stopped)
        {
            return new ActorAssignment(
                actor.ActorId,
                EAIActionType.HoldPosition,
                actor.Position);
        }

        if (actor.HasBall)
        {
            return new ActorAssignment(
                actor.ActorId,
                EAIActionType.Shoot,
                gameState.GetAttackingGoalPosition(teamId),
                priority: 100);
        }

        if (closestActor != null && actor.ActorId == closestActor.ActorId)
        {
            return new ActorAssignment(
                actor.ActorId,
                EAIActionType.Move,
                gameState.BallPosition,
                priority: 50);
        }

        return new ActorAssignment(
            actor.ActorId,
            EAIActionType.HoldPosition,
            actor.Position);
    }

    /// <summary>
    /// Finds the active AI-controlled actor closest to the ball.
    /// </summary>
    /// <param name="actors">The actors available for selection.</param>
    /// <returns>The closest valid actor, or null when none is available.</returns>
    private IAIActor FindClosestAIActorToBall(
        IReadOnlyList<IAIActor> actors)
    {
        IAIActor closestActor = null;
        float closestDistanceSquared = float.MaxValue;

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsActive ||
                !actor.IsAIControlled ||
                actor.IsGoalkeeper)
            {
                continue;
            }

            float distanceSquared =
                (gameState.BallPosition - actor.Position).sqrMagnitude;

            if (distanceSquared >= closestDistanceSquared)
                continue;

            closestDistanceSquared = distanceSquared;
            closestActor = actor;
        }

        return closestActor;
    }

    /// <summary>
    /// Sends each team assignment to its corresponding actor controller.
    /// </summary>
    /// <param name="decision">The team decision being applied.</param>
    private void ApplyDecision(TeamDecision decision)
    {
        foreach (ActorAssignment assignment in decision.Assignments)
        {
            if (!actorControllers.TryGetValue(
                    assignment.ActorId,
                    out AIActorController controller))
            {
                continue;
            }

            controller.SetAssignment(assignment);
        }
    }

    /// <summary>
    /// Executes the current assignment for every registered AI actor.
    /// </summary>
    private void ExecuteAssignments()
    {
        foreach (AIActorController controller in actorControllers.Values)
            controller.ExecuteAssignment();
    }
}