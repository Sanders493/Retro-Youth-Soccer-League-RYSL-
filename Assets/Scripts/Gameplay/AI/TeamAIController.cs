using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


/// <summary>
/// Stores behavior priorities for one player role.
/// </summary>
[System.Serializable]
public struct RoleBehaviorPriorities
{
    [SerializeField] private int formation;
    [SerializeField] private int chaseBall;
    [SerializeField] private int shoot;
    [SerializeField] private int defend;
    [SerializeField] private int openSpace;
    [SerializeField] private int receivePass;
    [SerializeField] private int goalkeeper;

    public int Formation => formation;
    public int ChaseBall => chaseBall;
    public int Shoot => shoot;
    public int Defend => defend;
    public int OpenSpace => openSpace;
    public int ReceivePass => receivePass;
    public int Goalkeeper => goalkeeper;
}


/// <summary>
/// Coordinates the behaviors of AI-controlled actors belonging to one team.
/// </summary>
public class TeamAIController : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private ETeamId teamId;
    [SerializeField] private GameState gameState;

    [Header("Crowding")]
    [SerializeField] private float minimumTargetSeparation = 1.25f;
    [SerializeField] private float crowdingSearchStep = 0.75f;
    [SerializeField] private int crowdingSearchRings = 3;

    [Header("Formation")]
    [SerializeField, SerializedDictionary("ActorID", "Formation Position")]
    private SerializedDictionary<string, EFormationPosition>
        formationPositions = new();

    [Header("Formation Behavior")]
    [SerializeField, SerializedDictionary("Player Role", "Behavior Priorities")]
    private SerializedDictionary<EPlayerRole, RoleBehaviorPriorities>
        roleBehaviorPriorities = new();
    
    [SerializeField] private float formationTolerance = 0.25f;

    [Header("Chase Ball Behavior")]

    [Header("Shoot Behavior")]
    [SerializeField] private float maximumShootingDistance = 6f;
    [SerializeField] private float maximumPassingDistance = 8f;

    [Header("Defend Behavior")]

    [Header("Open Space Behavior")]
    [SerializeField] private float horizontalSearchDistance = 0.35f;
    [SerializeField] private float verticalSearchDistance = 0.2f;
    [SerializeField] private float opponentAvoidanceDistance = 2f;

    [Header("Receive Pass Behavior")]
    [SerializeField] private float receiveStoppingDistance = 0.25f;

    [Header("Goalkeeper Behavior")]
    [SerializeField] private float goalkeeperGoalOffset = 0.5f;


    private FormationBehavior _formationBehavior;
    private ChaseBallBehavior _chaseBallBehavior;
    private ShootBehavior _shootBehavior;
    private DefendBehavior _defendBehavior;
    private OpenSpaceBehavior _openSpaceBehavior;
    private ReceivePassBehavior _receivePassBehavior;
    private GoalkeeperBehavior _goalkeeperBehavior;

    private Dictionary<string, AIActorController> _actorControllers;

    public ETeamAIState CurrentState
    {
        get;
        private set;
    }

    public TeamDecision CurrentDecision
    {
        get;
        private set;
    }

    /// <summary>
    /// Adds default priority entries for missing player roles.
    /// </summary>
    private void OnValidate()
    {
        AddRolePrioritiesIfMissing(
            EPlayerRole.Goalkeeper,
            new RoleBehaviorPriorities());

        AddRolePrioritiesIfMissing(
            EPlayerRole.Defender,
            new RoleBehaviorPriorities());

        AddRolePrioritiesIfMissing(
            EPlayerRole.Midfielder,
            new RoleBehaviorPriorities());

        AddRolePrioritiesIfMissing(
            EPlayerRole.Forward,
            new RoleBehaviorPriorities());
    }
    
    /// <summary>
    /// Adds a player-role priority entry when one does not already exist.
    /// </summary>
    /// <param name="playerRole">The role being checked.</param>
    /// <param name="priorities">The default priorities.</param>
    private void AddRolePrioritiesIfMissing(
        EPlayerRole playerRole,
        RoleBehaviorPriorities priorities)
    {
        if (roleBehaviorPriorities.ContainsKey(playerRole))
            return;

        roleBehaviorPriorities.Add(
            playerRole,
            priorities);
    }
    
    /// <summary>
    /// Registers actors after all scene objects have initialized.
    /// </summary>
    private void Start()
    {
        RegisterActors();
    }

    /// <summary>
    /// Updates the team AI during the physics update.
    /// </summary>
    private void FixedUpdate()
    {
        UpdateActorRegistration();
        UpdateTeam();
    }

    /// <summary>
    /// Evaluates, separates, and executes the team's current assignments.
    /// </summary>
    public void UpdateTeam()
    {
        CurrentState = DetermineTeamState();
        CurrentDecision = CreateTeamDecision();
        CurrentDecision =
            ResolveCrowdedAssignments(CurrentDecision);

        ApplyDecision(CurrentDecision);
        ExecuteAssignments();
    }

    /// <summary>
    /// Registers all currently available AI-controlled actors.
    /// </summary>
    private void RegisterActors()
    {
        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(teamId);

        foreach (IAIActor actor in actors)
            RegisterActor(actor);
    }

    /// <summary>
    /// Registers actors that were added after initialization.
    /// </summary>
    private void UpdateActorRegistration()
    {
        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(teamId);

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || _actorControllers.ContainsKey(actor.ActorId))
            {
                continue;
            }

            RegisterActor(actor);
        }
    }

    /// <summary>
    /// Registers one AI-controlled actor.
    /// </summary>
    /// <param name="actor">The actor to register.</param>
    private void RegisterActor(IAIActor actor)
    {
        if (actor == null
            || !actor.IsAIControlled
            || string.IsNullOrWhiteSpace(actor.ActorId))
        {
            return;
        }

        if (actor.ActionOutput == null)
        {
            Debug.LogError(
                $"{name}: Actor {actor.ActorId} has no IAIActionOutput.",
                this);

            return;
        }

        if (!(actor is MonoBehaviour actorBehaviour))
        {
            Debug.LogError(
                $"{name}: Actor {actor.ActorId} must be a MonoBehaviour.",
                this);

            return;
        }

        AIActorController controller =
            actorBehaviour.GetComponent<AIActorController>();

        if (controller == null)
        {
            controller =
                actorBehaviour.gameObject
                    .AddComponent<AIActorController>();
        }

        controller.Initialize(actor,gameState);

        _actorControllers[actor.ActorId] = controller;
    }

    /// <summary>
    /// Adjusts movement assignments so multiple actors are not sent to nearly
    /// identical destinations.
    /// </summary>
    /// <param name="decision">The original team decision.</param>
    /// <returns>A decision containing separated movement targets.</returns>
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
    /// <returns>The assignment comparison result.</returns>
    private int CompareAssignmentPriority(
        ActorAssignment first,
        ActorAssignment second)
    {
        return second.Priority.CompareTo(first.Priority);
    }

    /// <summary>
    /// Resolves one assignment against previously reserved targets.
    /// </summary>
    /// <param name="assignment">The assignment being resolved.</param>
    /// <param name="reservedTargets">Previously reserved destinations.</param>
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
    /// <returns>True when the assignment uses a movement target.</returns>
    private bool UsesMovementTarget(
        ActorAssignment assignment)
    {
        return assignment.ActionType == EAIActionType.Move
            || assignment.ActionType
            == EAIActionType.HoldPosition;
    }

    /// <summary>
    /// Determines whether a target is separated from all reserved targets.
    /// </summary>
    /// <param name="target">The proposed target.</param>
    /// <param name="reservedTargets">The reserved targets.</param>
    /// <returns>True when the target is available.</returns>
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
    /// Searches around a crowded destination for an available target.
    /// </summary>
    /// <param name="actorId">The actor requiring a target.</param>
    /// <param name="originalTarget">The original destination.</param>
    /// <param name="reservedTargets">The reserved targets.</param>
    /// <returns>The nearest available destination.</returns>
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
    /// Calculates the preferred separation direction.
    /// </summary>
    /// <param name="actor">The actor being repositioned.</param>
    /// <param name="target">The crowded target.</param>
    /// <returns>The preferred normalized direction.</returns>
    private Vector2 GetPreferredSeparationDirection(
        IAIActor actor,
        Vector2 target)
    {
        if (actor == null)
            return Vector2.right;

        Vector2 direction =
            actor.Position - target;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
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
    /// Rotates a two-dimensional direction.
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
    /// Restricts a destination to the playable field.
    /// </summary>
    /// <param name="actor">The actor receiving the destination.</param>
    /// <param name="worldTarget">The proposed destination.</param>
    /// <returns>A destination inside the field.</returns>
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
    /// Creates assignments for every AI-controlled actor.
    /// </summary>
    /// <returns>The resulting team decision.</returns>
    private TeamDecision CreateTeamDecision()
    {
        TeamDecision decision =
            new TeamDecision(CurrentState);

        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(teamId);

        IAIActor primaryBallChaser =
            FindPrimaryBallChaser(actors);

        foreach (IAIActor actor in actors)
        {
            if (!actor.IsActive
                || !actor.IsAIControlled)
            {
                continue;
            }

            if (!formationPositions.TryGetValue(
                    actor.ActorId,
                    out EFormationPosition formationPosition))
            {
                continue;
            }

            bool isPrimaryBallChaser =
                primaryBallChaser != null
                && primaryBallChaser.ActorId
                == actor.ActorId;

            AIBehaviorContext context =
                new AIBehaviorContext(
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
    /// <param name="context">The actor's behavior context.</param>
    /// <returns>The selected assignment.</returns>
    private ActorAssignment SelectAssignment(
        AIBehaviorContext context)
    {
        IAIBehavior selectedBehavior = null;
        int selectedPriority = int.MinValue;

        EvaluateBehavior(
            _goalkeeperBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _receivePassBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _shootBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _chaseBallBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _defendBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _openSpaceBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        EvaluateBehavior(
            _formationBehavior,
            context,
            ref selectedBehavior,
            ref selectedPriority);

        return selectedBehavior?.CreateAssignment(context);
    }

    
    /// <summary>
    /// Gets the editor-configured priority for a behavior and player role.
    /// </summary>
    /// <param name="playerRole">The actor's player role.</param>
    /// <param name="behavior">The behavior being evaluated.</param>
    /// <returns>The configured behavior priority.</returns>
    private int GetRoleBehaviorPriority(
        EPlayerRole playerRole,
        IAIBehavior behavior)
    {
        if (!roleBehaviorPriorities.TryGetValue(
                playerRole,
                out RoleBehaviorPriorities priorities))
        {
            return behavior.GetPriority(null);
        }

        if (behavior == _formationBehavior)
            return priorities.Formation;

        if (behavior == _chaseBallBehavior)
            return priorities.ChaseBall;

        if (behavior == _shootBehavior)
            return priorities.Shoot;

        if (behavior == _defendBehavior)
            return priorities.Defend;

        if (behavior == _openSpaceBehavior)
            return priorities.OpenSpace;

        if (behavior == _receivePassBehavior)
            return priorities.ReceivePass;

        if (behavior == _goalkeeperBehavior)
            return priorities.Goalkeeper;

        return int.MinValue;
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
            GetRoleBehaviorPriority(
                context.Actor.PlayerRole,
                behavior);

        if (behaviorPriority <= selectedPriority)
            return;

        selectedBehavior = behavior;
        selectedPriority = behaviorPriority;
    }


    /// <summary>
    /// Finds the closest valid actor to chase the ball.
    /// </summary>
    /// <param name="actors">The available actors.</param>
    /// <returns>The selected ball chaser.</returns>
    private IAIActor FindPrimaryBallChaser(
        IReadOnlyList<IAIActor> actors)
    {
        if (gameState.HasPossession(teamId))
            return null;

        IAIActor closestActor = null;
        float closestDistanceSquared =
            float.MaxValue;

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

            if (distanceSquared
                >= closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared =
                distanceSquared;

            closestActor = actor;
        }

        return closestActor;
    }

    /// <summary>
    /// Sends each assignment to its actor controller.
    /// </summary>
    /// <param name="decision">The decision being applied.</param>
    private void ApplyDecision(
        TeamDecision decision)
    {
        foreach (ActorAssignment assignment
                 in decision.Assignments)
        {
            if (!_actorControllers.TryGetValue(
                    assignment.ActorId,
                    out AIActorController controller))
            {
                continue;
            }

            controller.SetAssignment(assignment);
        }
    }

    /// <summary>
    /// Executes every registered actor assignment.
    /// </summary>
    private void ExecuteAssignments()
    {
        foreach (AIActorController controller
                 in _actorControllers.Values)
        {
            controller.ExecuteAssignment();
        }
    }
}