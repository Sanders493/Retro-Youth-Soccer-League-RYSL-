using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Coordinates the behaviors of AI-controlled actors belonging to one team.
/// </summary>
public sealed class TeamAIController
{
    private readonly ETeamId teamId;
    private readonly IGameState gameState;

    private readonly float minimumTargetSeparation = 1.25f;
    private readonly float crowdingSearchStep = 0.75f;
    private readonly int crowdingSearchRings = 3;
    
    private readonly FormationBehavior formationBehavior;
    private readonly ChaseBallBehavior chaseBallBehavior;
    private readonly ShootBehavior shootBehavior;
    private readonly DefendBehavior defendBehavior;
    
    private readonly Dictionary<string, AIActorController> actorControllers;
    private readonly IReadOnlyDictionary<string, EFormationPosition>
        formationPositions;

    public ETeamAIState CurrentState { get; private set; }

    public TeamDecision CurrentDecision { get; private set; }

    /// <summary>
    /// Creates a controller for an AI-controlled team.
    /// </summary>
    /// <param name="teamId">The team controlled by this controller.</param>
    /// <param name="gameState">
    /// The read-only source of current match information.
    /// </param>
    /// <param name="actionOutput">
    /// The interface used to send actions to gameplay systems.
    /// </param>
    /// <param name="formationPositions">
    /// The formation position assigned to each actor.
    /// </param>
    public TeamAIController(
        ETeamId teamId,
        IGameState gameState,
        IAIActionOutput actionOutput,
        IReadOnlyDictionary<string, EFormationPosition> formationPositions)
    {
        this.teamId = teamId;
        this.gameState = gameState;
        this.formationPositions = formationPositions;

        formationBehavior = new FormationBehavior();
        chaseBallBehavior = new ChaseBallBehavior();
        shootBehavior = new ShootBehavior();
        defendBehavior = new DefendBehavior();

        actorControllers = new Dictionary<string, AIActorController>();

        RegisterActors(actionOutput);
    }
    /// <summary>
    /// Evaluates, separates, and executes the team's current assignments.
    /// </summary>
    public void UpdateTeam()
    {
        CurrentState = DetermineTeamState();
        CurrentDecision = CreateTeamDecision();
        CurrentDecision = ResolveCrowdedAssignments(CurrentDecision);

        ApplyDecision(CurrentDecision);
        ExecuteAssignments();
    }
    
   /// <summary>
/// Adjusts movement assignments so multiple actors are not sent to nearly
/// identical destinations.
/// </summary>
/// <param name="decision">The original team decision.</param>
/// <returns>A new decision containing separated movement targets.</returns>
private TeamDecision ResolveCrowdedAssignments(
    TeamDecision decision)
{
    TeamDecision resolvedDecision =
        new TeamDecision(decision.TeamState);

    List<ActorAssignment> orderedAssignments =
        new List<ActorAssignment>(decision.Assignments);

    orderedAssignments.Sort(
        CompareAssignmentPriority);

    List<Vector2> reservedTargets =
        new List<Vector2>();

    foreach (ActorAssignment assignment in orderedAssignments)
    {
        ActorAssignment resolvedAssignment =
            ResolveAssignmentTarget(
                assignment,
                reservedTargets);

        resolvedDecision.AddAssignment(
            resolvedAssignment);

        if (UsesMovementTarget(resolvedAssignment))
        {
            reservedTargets.Add(
                resolvedAssignment.TargetPosition);
        }
    }

    return resolvedDecision;
}

/// <summary>
/// Orders assignments from highest priority to lowest priority.
/// </summary>
/// <param name="first">The first assignment.</param>
/// <param name="second">The second assignment.</param>
/// <returns>
/// A negative value when the first assignment has greater priority,
/// a positive value when the second has greater priority, or zero when equal.
/// </returns>
private int CompareAssignmentPriority(
    ActorAssignment first,
    ActorAssignment second)
{
    return second.Priority.CompareTo(first.Priority);
}

/// <summary>
/// Resolves one assignment against targets already reserved by
/// higher-priority assignments.
/// </summary>
/// <param name="assignment">The assignment being resolved.</param>
/// <param name="reservedTargets">
/// Targets already claimed by higher-priority actors.
/// </param>
/// <returns>The original or adjusted assignment.</returns>
private ActorAssignment ResolveAssignmentTarget(
    ActorAssignment assignment,
    IReadOnlyList<Vector2> reservedTargets)
{
    if (!UsesMovementTarget(assignment))
        return assignment;

    if (IsTargetAvailable(
            assignment.TargetPosition,
            reservedTargets))
    {
        return assignment;
    }

    Vector2 separatedTarget =
        FindAvailableTarget(
            assignment.ActorId,
            assignment.TargetPosition,
            reservedTargets);

    return new ActorAssignment(
        assignment.ActorId,
        assignment.ActionType,
        separatedTarget,
        assignment.TargetActorId,
        assignment.Priority);
}

/// <summary>
/// Determines whether an assignment reserves a movement destination.
/// </summary>
/// <param name="assignment">The assignment being checked.</param>
/// <returns>
/// True when the assignment moves or holds an actor at a position.
/// </returns>
private bool UsesMovementTarget(
    ActorAssignment assignment)
{
    return assignment.ActionType == EAIActionType.Move
        || assignment.ActionType == EAIActionType.HoldPosition;
}

/// <summary>
/// Determines whether a target is sufficiently separated from all
/// previously reserved targets.
/// </summary>
/// <param name="target">The proposed target.</param>
/// <param name="reservedTargets">The existing reserved targets.</param>
/// <returns>True when the target has enough space.</returns>
private bool IsTargetAvailable(
    Vector2 target,
    IReadOnlyList<Vector2> reservedTargets)
{
    float minimumDistanceSquared =
        minimumTargetSeparation
        * minimumTargetSeparation;

    foreach (Vector2 reservedTarget in reservedTargets)
    {
        if ((target - reservedTarget).sqrMagnitude
            < minimumDistanceSquared)
        {
            return false;
        }
    }

    return true;
}

/// <summary>
/// Searches around a crowded destination for the nearest available target.
/// </summary>
/// <param name="actorId">The actor requiring a new target.</param>
/// <param name="originalTarget">The actor's original destination.</param>
/// <param name="reservedTargets">Targets already claimed by other actors.</param>
/// <returns>The nearest available valid target.</returns>
private Vector2 FindAvailableTarget(
    string actorId,
    Vector2 originalTarget,
    IReadOnlyList<Vector2> reservedTargets)
{
    IAIActor actor =
        gameState.GetActor(actorId);

    Vector2 preferredDirection =
        GetPreferredSeparationDirection(
            actor,
            originalTarget);

    for (int ring = 1;
         ring <= crowdingSearchRings;
         ring++)
    {
        float distance =
            crowdingSearchStep * ring;

        Vector2[] candidates =
        {
            originalTarget
                + preferredDirection * distance,

            originalTarget
                - preferredDirection * distance,

            originalTarget
                + RotateDirection(
                    preferredDirection,
                    90f) * distance,

            originalTarget
                + RotateDirection(
                    preferredDirection,
                    -90f) * distance,

            originalTarget
                + RotateDirection(
                    preferredDirection,
                    45f) * distance,

            originalTarget
                + RotateDirection(
                    preferredDirection,
                    -45f) * distance
        };

        foreach (Vector2 candidate in candidates)
        {
            Vector2 validCandidate =
                ClampTargetToField(
                    actor,
                    candidate);

            if (IsTargetAvailable(
                    validCandidate,
                    reservedTargets))
            {
                return validCandidate;
            }
        }
    }

    return ClampTargetToField(
        actor,
        originalTarget);
}

/// <summary>
/// Calculates the preferred direction used when separating an actor from
/// a crowded destination.
/// </summary>
/// <param name="actor">The actor being repositioned.</param>
/// <param name="target">The crowded destination.</param>
/// <returns>A normalized separation direction.</returns>
private Vector2 GetPreferredSeparationDirection(
    IAIActor actor,
    Vector2 target)
{
    if (actor == null)
        return Vector2.right;

    Vector2 direction =
        actor.Position - target;

    if (direction.sqrMagnitude
        <= Mathf.Epsilon)
    {
        Vector2 actorFieldPosition =
            gameState.GetTeamRelativeFieldPosition(
                actor.TeamId,
                actor.Position);

        direction = actorFieldPosition.x <= 0f
            ? Vector2.left
            : Vector2.right;
    }

    return direction.normalized;
}

/// <summary>
/// Rotates a two-dimensional direction by the specified number of degrees.
/// </summary>
/// <param name="direction">The direction being rotated.</param>
/// <param name="degrees">The rotation in degrees.</param>
/// <returns>The rotated direction.</returns>
private Vector2 RotateDirection(
    Vector2 direction,
    float degrees)
{
    float radians =
        degrees * Mathf.Deg2Rad;

    float cosine =
        Mathf.Cos(radians);

    float sine =
        Mathf.Sin(radians);

    return new Vector2(
        direction.x * cosine
            - direction.y * sine,
        direction.x * sine
            + direction.y * cosine);
}

/// <summary>
/// Restricts a proposed destination to the playable field.
/// </summary>
/// <param name="actor">The actor receiving the destination.</param>
/// <param name="worldTarget">The proposed world-space destination.</param>
/// <returns>A world-space position inside the playable field.</returns>
private Vector2 ClampTargetToField(
    IAIActor actor,
    Vector2 worldTarget)
{
    if (actor == null)
        return worldTarget;

    Vector2 fieldPosition =
        gameState.GetTeamRelativeFieldPosition(
            actor.TeamId,
            worldTarget);

    fieldPosition.x =
        Mathf.Clamp(fieldPosition.x, -1f, 1f);

    fieldPosition.y =
        Mathf.Clamp01(fieldPosition.y);

    return gameState.GetWorldPositionFromTeamRelative(
        actor.TeamId,
        fieldPosition);
} 
    
    /// <summary>
    /// Registers the team's AI-controlled actors.
    /// </summary>
    /// <param name="actionOutput">
    /// The output used by each actor controller.
    /// </param>
    private void RegisterActors(IAIActionOutput actionOutput)
    {
        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(teamId);

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsAIControlled)
                continue;

            actorControllers.Add(
                actor.ActorId,
                new AIActorController(
                    actor.ActorId,
                    actionOutput));
        }
    }

    /// <summary>
    /// Determines the team's current tactical state.
    /// </summary>
    /// <returns>The current tactical state.</returns>
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
    /// Creates the current assignments for every AI-controlled actor.
    /// </summary>
    /// <returns>The resulting team decision.</returns>
    private TeamDecision CreateTeamDecision()
    {
        TeamDecision decision = new TeamDecision(CurrentState);

        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(teamId);

        IAIActor primaryBallChaser =
            FindPrimaryBallChaser(actors);

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsActive || !actor.IsAIControlled)
                continue;

            if (!formationPositions.TryGetValue(
                    actor.ActorId,
                    out EFormationPosition formationPosition))
            {
                continue;
            }

            bool isPrimaryBallChaser =
                primaryBallChaser != null
                && primaryBallChaser.ActorId == actor.ActorId;

            AIBehaviorContext context = new AIBehaviorContext(
                actor,
                gameState,
                formationPosition,
                isPrimaryBallChaser);

            ActorAssignment assignment =
                SelectAssignment(context);

            if (assignment != null)
                decision.AddAssignment(assignment);
        }

        return decision;
    }

    /// <summary>
    /// Selects the highest-priority valid behavior for an actor.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// The assignment created by the selected behavior, or null when no behavior
    /// can execute.
    /// </returns>
    private ActorAssignment SelectAssignment(
        AIBehaviorContext context)
    {
        IAIBehavior selectedBehavior = null;
        int selectedPriority = int.MinValue;

        EvaluateBehavior(
            shootBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            chaseBallBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            defendBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            formationBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        return selectedBehavior?.CreateAssignment(context);
    }

    /// <summary>
    /// Compares a behavior against the currently selected behavior.
    /// </summary>
    /// <param name="behavior">The behavior being evaluated.</param>
    /// <param name="context">The actor's behavior context.</param>
    /// <param name="selectedBehavior">
    /// The currently selected behavior.
    /// </param>
    /// <param name="selectedPriority">
    /// The priority of the currently selected behavior.
    /// </param>
    private void EvaluateBehavior(
        IAIBehavior behavior,
        AIBehaviorContext context,
        ref IAIBehavior selectedBehavior,
        ref int selectedPriority)
    {
        if (!behavior.CanExecute(context))
            return;

        int behaviorPriority =
            behavior.GetPriority(context);

        if (behaviorPriority <= selectedPriority)
            return;

        selectedBehavior = behavior;
        selectedPriority = behaviorPriority;
    }

    /// <summary>
    /// Finds the active non-goalkeeper AI actor closest to a loose or
    /// opponent-controlled ball.
    /// </summary>
    /// <param name="actors">The actors available for selection.</param>
    /// <returns>
    /// The closest valid actor, or null when the team already possesses the ball
    /// or no valid actor exists.
    /// </returns>
    private IAIActor FindPrimaryBallChaser(
        IReadOnlyList<IAIActor> actors)
    {
        if (gameState.HasPossession(teamId))
            return null;

        IAIActor closestActor = null;
        float closestDistanceSquared = float.MaxValue;

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsActive
                || !actor.IsAIControlled
                || actor.IsGoalkeeper
                || actor.HasBall)
            {
                continue;
            }

            float distanceSquared =
                (gameState.BallPosition - actor.Position)
                .sqrMagnitude;

            if (distanceSquared >= closestDistanceSquared)
                continue;

            closestDistanceSquared = distanceSquared;
            closestActor = actor;
        }

        return closestActor;
    }

    /// <summary>
    /// Sends each assignment to its corresponding actor controller.
    /// </summary>
    /// <param name="decision">The decision being applied.</param>
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
    /// Executes the current assignment for every registered actor.
    /// </summary>
    private void ExecuteAssignments()
    {
        foreach (AIActorController controller in actorControllers.Values)
            controller.ExecuteAssignment();
    }
}