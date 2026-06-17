using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// Evaluates behavior trees and coordinates AI-controlled actors belonging
/// to one team.
/// </summary>
public sealed class TeamAIController :
    MonoBehaviour
{
    [Header("Team")]
    [SerializeField]
    private ETeamId teamId;

    [SerializeField]
    private GameState gameState;

    [Header("Behavior Trees")]
    [SerializeField, SerializedDictionary(
        "Player Role",
        "Behavior Tree")]
    private SerializedDictionary<EPlayerRole, AIBehaviorTree>
        roleBehaviorTrees = new();

    [Header("Primary Chaser")]
    [Tooltip(
        "The closeness share required for a player-controlled teammate to " +
        "take priority over the nearest AI-controlled ball chaser.")]
    [SerializeField, Range(0.5f, 1f)]
    private float playerChaserPriorityShare = 0.6f;

    [Header("Crowding")]
    [Tooltip(
        "The minimum permitted distance between movement destinations.")]
    [SerializeField]
    private float minimumTargetSeparation = 1.25f;

    [Tooltip(
        "The distance between successive crowding-search candidates.")]
    [SerializeField]
    private float crowdingSearchStep = 0.75f;

    [Tooltip(
        "The number of candidate rings searched around a crowded target.")]
    [SerializeField]
    private int crowdingSearchRings = 3;

    [Header("Debug")]
    [SerializeField]
    private bool logUncoveredTreeCases;

    private Dictionary<string, AIActorController>
        actorControllers;

    /// <summary>
    /// Gets the team's current tactical state.
    /// </summary>
    public ETeamAIState CurrentState
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the team's most recently created decision.
    /// </summary>
    public TeamDecision CurrentDecision
    {
        get;
        private set;
    }

    /// <summary>
    /// Initializes runtime collections and validates required references.
    /// </summary>
    private void Awake()
    {
        actorControllers =
            new Dictionary<string, AIActorController>();

        if (gameState != null)
            return;

        Debug.LogError(
            $"{name}: No GameState has been assigned.",
            this);

        enabled = false;
    }

    /// <summary>
    /// Registers currently available actors after scene initialization.
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
        if (gameState == null
            || !gameState.IsMatchActive)
        {
            return;
        }

        UpdateActorRegistration();
        UpdateTeam();
    }

    /// <summary>
    /// Evaluates behavior trees, resolves crowded destinations, applies the
    /// resulting decision, and executes all assignments.
    /// </summary>
    public void UpdateTeam()
    {
        CurrentState =
            DetermineTeamState();

        TeamDecision decision =
            CreateTeamDecision();

        CurrentDecision =
            ResolveCrowdedAssignments(
                decision);

        ApplyDecision(
            CurrentDecision);

        ExecuteAssignments();
    }

    /// <summary>
    /// Finds the behavior tree assigned to a player role.
    /// </summary>
    /// <param name="playerRole">
    /// The actor's general player role.
    /// </param>
    /// <returns>
    /// The assigned behavior tree, or null when none is configured.
    /// </returns>
    private AIBehaviorTree GetBehaviorTree(
        EPlayerRole playerRole)
    {
        roleBehaviorTrees.TryGetValue(
            playerRole,
            out AIBehaviorTree behaviorTree);

        return behaviorTree;
    }

    /// <summary>
    /// Registers one AI-controlled actor with its runtime controller.
    /// </summary>
    /// <param name="actor">
    /// The actor being registered.
    /// </param>
    private void RegisterActor(
        IAIActor actor)
    {
        if (actor == null
            || !actor.IsAIControlled
            || string.IsNullOrWhiteSpace(
                actor.ActorId))
        {
            return;
        }

        if (actor.ActionOutput == null)
        {
            Debug.LogError(
                $"{name}: Actor {actor.ActorId} has no " +
                $"IAIActionOutput.",
                this);

            return;
        }

        if (!(actor is MonoBehaviour actorBehaviour))
        {
            Debug.LogError(
                $"{name}: Actor {actor.ActorId} must be a " +
                $"MonoBehaviour.",
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

        controller.Initialize(
            actor,
            gameState);

        actorControllers[actor.ActorId] =
            controller;
    }

    /// <summary>
    /// Registers every currently active AI-controlled actor on this team.
    /// </summary>
    private void RegisterActors()
    {
        if (gameState == null)
            return;

        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(
                teamId);

        if (actors == null)
            return;

        foreach (IAIActor actor in actors)
        {
            RegisterActor(
                actor);
        }
    }

    /// <summary>
    /// Registers actors added after this controller was initialized.
    /// </summary>
    private void UpdateActorRegistration()
    {
        if (gameState == null
            || actorControllers == null)
        {
            return;
        }

        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(
                teamId);

        if (actors == null)
            return;

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || string.IsNullOrWhiteSpace(
                    actor.ActorId)
                || actorControllers.ContainsKey(
                    actor.ActorId))
            {
                continue;
            }

            RegisterActor(
                actor);
        }
    }

    /// <summary>
    /// Determines the team's current tactical state.
    /// </summary>
    /// <returns>
    /// The current team state.
    /// </returns>
    private ETeamAIState DetermineTeamState()
    {
        if (!gameState.IsMatchActive)
            return ETeamAIState.Stopped;

        if (gameState.HasPossession(
                teamId))
        {
            return ETeamAIState.Attacking;
        }

        if (gameState.TeamInPossession
            == ETeamId.None)
        {
            return ETeamAIState.Transitioning;
        }

        return ETeamAIState.Defending;
    }

    /// <summary>
    /// Evaluates each AI-controlled actor's assigned behavior tree.
    /// </summary>
    /// <returns>
    /// The resulting team decision.
    /// </returns>
    private TeamDecision CreateTeamDecision()
    {
        TeamDecision decision =
            new TeamDecision(
                CurrentState);

        IReadOnlyList<IAIActor> actors =
            gameState.GetTeamActors(
                teamId);

        if (actors == null)
            return decision;

        IAIActor primaryBallChaser =
            FindPrimaryBallChaser(
                actors);

        IAIActor primaryDefender =
            FindPrimaryDefender(
                actors);

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || !actor.IsActive
                || !actor.IsAIControlled)
            {
                continue;
            }

            bool isPrimaryBallChaser =
                primaryBallChaser != null
                && primaryBallChaser.ActorId
                    == actor.ActorId;

            bool isPrimaryDefender =
                primaryDefender != null
                && primaryDefender.ActorId
                    == actor.ActorId;

            AIBehaviorContext context =
                new AIBehaviorContext(
                    actor,
                    gameState,
                    actor.FormationPosition,
                    CurrentState,
                    isPrimaryBallChaser,
                    isPrimaryDefender);

            ActorAssignment assignment =
                SelectAssignment(
                    context);

            if (assignment != null)
            {
                decision.AddAssignment(
                    assignment);
            }
        }

        return decision;
    }

    /// <summary>
    /// Evaluates the behavior tree assigned to an actor's role.
    /// </summary>
    /// <param name="context">
    /// The actor-specific behavior-tree context.
    /// </param>
    /// <returns>
    /// The assignment produced by the tree, or null when no branch succeeds.
    /// </returns>
    private ActorAssignment SelectAssignment(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null)
        {
            return null;
        }

        AIBehaviorTree behaviorTree =
            GetBehaviorTree(
                context.Actor.PlayerRole);

        if (behaviorTree == null)
        {
            LogUncoveredTreeCase(
                context,
                "no behavior tree is assigned to this role");

            return null;
        }

        context.ResetResults();

        EBehaviorTreeResult result =
            behaviorTree.Evaluate(
                context);

        if (result == EBehaviorTreeResult.Failure
            || context.Assignment == null)
        {
            LogUncoveredTreeCase(
                context,
                result == EBehaviorTreeResult.Failure
                    ? "the behavior tree returned Failure"
                    : "the tree returned without producing an assignment");

            return null;
        }

        return context.Assignment.WithTreeSelection(
            context.RootSelection,
            context.NestedSelectorSelection);
    }

    /// <summary>
    /// Logs a behavior-tree case that produced no assignment.
    /// </summary>
    private void LogUncoveredTreeCase(
        AIBehaviorContext context,
        string reason)
    {
        if (!logUncoveredTreeCases)
            return;

        Debug.Log(
            $"{name}: No assignment produced. " +
            $"Actor={context.Actor.ActorId}, " +
            $"Role={context.Actor.PlayerRole}, " +
            $"TeamState={context.TeamState}, " +
            $"BallOwner=" +
            $"{context.GameState.BallOwner?.ActorId ?? "None"}, " +
            $"RootSelection=" +
            $"{context.RootSelection ?? string.Empty}, " +
            $"NestedSelection=" +
            $"{context.NestedSelectorSelection ?? string.Empty}, " +
            $"Reason={reason}.",
            this);
    }

    /// <summary>
    /// Finds the primary AI ball chaser while allowing the player-controlled
    /// actor to take priority when sufficiently closer.
    /// </summary>
    /// <param name="actors">
    /// Every active actor belonging to the team.
    /// </param>
    /// <returns>
    /// The selected AI ball chaser, or null when the player should chase.
    /// </returns>
    private IAIActor FindPrimaryBallChaser(
        IReadOnlyList<IAIActor> actors)
    {
        if (gameState.HasBallOwner
            || actors == null)
        {
            return null;
        }

        IAIActor closestAIActor =
            null;

        float closestAIDistance =
            float.PositiveInfinity;

        float closestPlayerDistance =
            float.PositiveInfinity;

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || !actor.IsActive
                || actor.HasBall)
            {
                continue;
            }
            
            if (gameState.IsBallControlBlockedFor(
                    actor))
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    gameState.BallPosition,
                    actor.Position);

            if (actor.IsAIControlled)
            {
                if (distance
                    >= closestAIDistance)
                {
                    continue;
                }

                closestAIDistance =
                    distance;

                closestAIActor =
                    actor;
            }
            else
            {
                closestPlayerDistance =
                    Mathf.Min(
                        closestPlayerDistance,
                        distance);
            }
        }

        if (ShouldPlayerBePrimaryChaser(
                closestPlayerDistance,
                closestAIDistance))
        {
            return null;
        }

        return closestAIActor;
    }

    /// <summary>
    /// Determines whether the player-controlled actor should be the primary
    /// ball chaser.
    /// </summary>
    private bool ShouldPlayerBePrimaryChaser(
        float playerDistance,
        float closestAIDistance)
    {
        if (float.IsPositiveInfinity(
                playerDistance))
        {
            return false;
        }

        if (float.IsPositiveInfinity(
                closestAIDistance))
        {
            return true;
        }

        float totalDistance =
            playerDistance
            + closestAIDistance;

        if (totalDistance <= Mathf.Epsilon)
            return true;

        float playerClosenessShare =
            closestAIDistance
            / totalDistance;

        return playerClosenessShare
            >= playerChaserPriorityShare;
    }

    /// <summary>
    /// Finds the closest eligible AI actor to pressure the opposing ball
    /// owner.
    /// </summary>
    /// <param name="actors">
    /// The actors belonging to this team.
    /// </param>
    /// <returns>
    /// The selected primary defender, or null when the opposing team does
    /// not control the ball.
    /// </returns>
    private IAIActor FindPrimaryDefender(
        IReadOnlyList<IAIActor> actors)
    {
        IAIActor opponentBallOwner =
            gameState.BallOwner;

        if (actors == null
            || opponentBallOwner == null
            || opponentBallOwner.TeamId
                == teamId)
        {
            return null;
        }

        IAIActor closestDefender =
            null;

        float closestDistanceSquared =
            float.PositiveInfinity;

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || !actor.IsActive
                || !actor.IsAIControlled
                || actor.IsGoalkeeper
                || actor.HasBall)
            {
                continue;
            }

            float distanceSquared =
                (opponentBallOwner.Position
                 - actor.Position)
                .sqrMagnitude;

            if (distanceSquared
                >= closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared =
                distanceSquared;

            closestDefender =
                actor;
        }

        return closestDefender;
    }

    /// <summary>
    /// Adjusts movement assignments so actors are not sent to nearly
    /// identical destinations.
    /// </summary>
    /// <param name="decision">
    /// The original team decision.
    /// </param>
    /// <returns>
    /// A decision containing separated movement destinations.
    /// </returns>
    private TeamDecision ResolveCrowdedAssignments(
        TeamDecision decision)
    {
        TeamDecision resolvedDecision =
            new TeamDecision(
                decision.TeamState);

        List<Vector2> reservedTargets =
            new List<Vector2>();

        foreach (
            ActorAssignment assignment
            in decision.Assignments)
        {
            ActorAssignment resolvedAssignment =
                ResolveAssignmentTarget(
                    assignment,
                    reservedTargets);

            resolvedDecision.AddAssignment(
                resolvedAssignment);

            if (UsesMovementTarget(
                    resolvedAssignment))
            {
                reservedTargets.Add(
                    resolvedAssignment.TargetPosition);
            }
        }

        return resolvedDecision;
    }

    /// <summary>
    /// Resolves one assignment against previously reserved destinations.
    /// </summary>
    private ActorAssignment ResolveAssignmentTarget(
        ActorAssignment assignment,
        IReadOnlyList<Vector2> reservedTargets)
    {
        if (!UsesMovementTarget(
                assignment))
        {
            return assignment;
        }

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
            assignment.RootSelection,
            assignment.NestedSelectorSelection);
    }

    /// <summary>
    /// Determines whether an assignment reserves a movement destination.
    /// </summary>
    private bool UsesMovementTarget(
        ActorAssignment assignment)
    {
        return assignment != null
            && assignment.ActionType
                == EAIActionType.Move;
    }

    /// <summary>
    /// Determines whether a proposed destination is separated from every
    /// reserved destination.
    /// </summary>
    private bool IsTargetAvailable(
        Vector2 target,
        IReadOnlyList<Vector2> reservedTargets)
    {
        float minimumDistanceSquared =
            minimumTargetSeparation
            * minimumTargetSeparation;

        foreach (Vector2 reservedTarget
                 in reservedTargets)
        {
            if ((target - reservedTarget)
                    .sqrMagnitude
                < minimumDistanceSquared)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Searches around a crowded destination for the nearest available
    /// target.
    /// </summary>
    private Vector2 FindAvailableTarget(
        string actorId,
        Vector2 originalTarget,
        IReadOnlyList<Vector2> reservedTargets)
    {
        IAIActor actor =
            gameState.GetActor(
                actorId);

        Vector2 preferredDirection =
            GetPreferredSeparationDirection(
                actor,
                originalTarget);

        for (int ring = 1;
             ring <= crowdingSearchRings;
             ring++)
        {
            float distance =
                crowdingSearchStep
                * ring;

            Vector2[] candidates =
            {
                originalTarget
                    + preferredDirection
                    * distance,

                originalTarget
                    - preferredDirection
                    * distance,

                originalTarget
                    + RotateDirection(
                        preferredDirection,
                        90f)
                    * distance,

                originalTarget
                    + RotateDirection(
                        preferredDirection,
                        -90f)
                    * distance,

                originalTarget
                    + RotateDirection(
                        preferredDirection,
                        45f)
                    * distance,

                originalTarget
                    + RotateDirection(
                        preferredDirection,
                        -45f)
                    * distance
            };

            foreach (Vector2 candidate
                     in candidates)
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
    /// Calculates a preferred direction for separating a crowded target.
    /// </summary>
    private Vector2 GetPreferredSeparationDirection(
        IAIActor actor,
        Vector2 target)
    {
        if (actor == null)
            return Vector2.up;

        Vector2 direction =
            actor.Position
            - target;

        if (direction.sqrMagnitude
            > Mathf.Epsilon)
        {
            return direction.normalized;
        }

        Vector2 actorFieldPosition =
            gameState.GetTeamRelativeFieldPosition(
                actor.TeamId,
                actor.Position);

        return actorFieldPosition.y <= 0f
            ? Vector2.down
            : Vector2.up;
    }

    /// <summary>
    /// Rotates a two-dimensional direction by the supplied angle.
    /// </summary>
    private Vector2 RotateDirection(
        Vector2 direction,
        float degrees)
    {
        float radians =
            degrees
            * Mathf.Deg2Rad;

        float cosine =
            Mathf.Cos(
                radians);

        float sine =
            Mathf.Sin(
                radians);

        return new Vector2(
            direction.x * cosine
                - direction.y * sine,
            direction.x * sine
                + direction.y * cosine);
    }

    /// <summary>
    /// Restricts a destination to the playable field.
    /// </summary>
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
            Mathf.Clamp01(
                fieldPosition.x);

        fieldPosition.y =
            Mathf.Clamp(
                fieldPosition.y,
                -1f,
                1f);

        return gameState
            .GetWorldPositionFromTeamRelative(
                actor.TeamId,
                fieldPosition);
    }

    /// <summary>
    /// Clears old assignments and applies the team's latest decision.
    /// </summary>
    private void ApplyDecision(
        TeamDecision decision)
    {
        foreach (
            AIActorController controller
            in actorControllers.Values)
        {
            if (controller == null)
                continue;

            controller.ClearAssignment();
        }

        foreach (
            ActorAssignment assignment
            in decision.Assignments)
        {
            if (!actorControllers.TryGetValue(
                    assignment.ActorId,
                    out AIActorController controller))
            {
                continue;
            }

            controller.SetAssignment(
                assignment);
        }
    }

    /// <summary>
    /// Executes every registered actor assignment.
    /// </summary>
    private void ExecuteAssignments()
    {
        if (actorControllers == null)
            return;

        foreach (
            AIActorController controller
            in actorControllers.Values)
        {
            if (controller == null)
                continue;

            controller.ExecuteAssignment();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        playerChaserPriorityShare =
            Mathf.Clamp(
                playerChaserPriorityShare,
                0.5f,
                1f);

        minimumTargetSeparation =
            Mathf.Max(
                0f,
                minimumTargetSeparation);

        crowdingSearchStep =
            Mathf.Max(
                0.01f,
                crowdingSearchStep);

        crowdingSearchRings =
            Mathf.Max(
                1,
                crowdingSearchRings);
    }
#endif
}