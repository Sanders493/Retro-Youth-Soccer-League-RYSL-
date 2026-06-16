using UnityEngine;

/// <summary>
/// Represents the attributes and behaviors of a player actor.
/// </summary>
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerKickController))]
public sealed class PlayerActor :
    MonoBehaviour,
    IAIActor,
    IAIActionOutput
{
    [Header("Identity")]
    [SerializeField] private string actorId;
    [SerializeField] private ETeamId teamId;
    [SerializeField] private EPlayerRole playerRole;
    [SerializeField] private EFormationPosition formationPosition;
    [SerializeField] private bool isAIControlled;
    [SerializeField] private bool isGoalkeeper;

    [Header("State")]
    [SerializeField] private bool isActive;
    [SerializeField] private bool hasBall;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 14f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private Vector2 currentVelocity;
    private Vector2 movementDirection;
    private Vector2 movementTarget;

    private bool hasMovementTarget;

    private PlayerInputReader inputReader;
    private PlayerKickController kickController;

    public string ActorId => actorId;
    public ETeamId TeamId => teamId;
    public Vector2 Velocity => currentVelocity;
    public bool IsActive => isActive;
    public bool IsAIControlled => isAIControlled;
    public bool IsGoalkeeper => isGoalkeeper;
    public bool HasBall => hasBall;
    public EPlayerRole PlayerRole => playerRole;
    public EFormationPosition FormationPosition => formationPosition;

    public IAIActionOutput ActionOutput => this;

    public Vector2 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    /// <summary>
    /// Retrieves the components used by the player.
    /// </summary>
    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        kickController = GetComponent<PlayerKickController>();
    }

    /// <summary>
    /// Processes human input and executes the current movement request.
    /// </summary>
    private void Update()
    {
        if (!isActive)
            return;

        if (!isAIControlled)
            ReadPlayerInput();

        UpdateMovement();
    }

    /// <summary>
    /// Converts player input into the same requests used by AI actors.
    /// </summary>
    private void ReadPlayerInput()
    {
        if (inputReader == null)
            return;

        RequestMovementDirection(inputReader.MovementInput);

        if (inputReader.PassPressed)
        {
            PlayerActor receiver =
                kickController.GetNearestTeammate();

            if (receiver != null)
            {
                RequestPass(
                    actorId,
                    receiver.ActorId);
            }
            else
            {
                Vector2 direction =
                    inputReader.MovementInput;

                if (direction == Vector2.zero)
                    direction = Vector2.right;

                RequestPass(
                    actorId,
                    Position + direction);
            }
        }

        if (inputReader.ShootPressed)
        {
            RequestShoot(
                actorId,
                kickController.OpponentGoalPosition);
        }

        if (inputReader.TakeBallPressed)
        {
            RequestTakeBall(actorId);
        }
    }

    /// <summary>
    /// Moves the actor using either a direction or a world-space target.
    /// </summary>
    private void UpdateMovement()
    {
        Vector2 desiredDirection = movementDirection;

        if (hasMovementTarget)
        {
            Vector2 difference =
                movementTarget - Position;

            if (difference.sqrMagnitude <=
                stoppingDistance * stoppingDistance)
            {
                hasMovementTarget = false;
                desiredDirection = Vector2.zero;
            }
            else
            {
                desiredDirection = difference.normalized;
            }
        }

        Vector2 targetVelocity =
            desiredDirection * moveSpeed;

        float accelerationRate =
            desiredDirection.sqrMagnitude > 0.01f
                ? acceleration
                : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            accelerationRate * Time.deltaTime);

        Position += currentVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Requests movement using a normalized direction.
    /// </summary>
    /// <param name="direction">The requested movement direction.</param>
    private void RequestMovementDirection(Vector2 direction)
    {
        hasMovementTarget = false;
        movementDirection = Vector2.ClampMagnitude(
            direction,
            1f);
    }

    /// <summary>
    /// Requests that this actor move toward a world position.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the actor making the request.
    /// </param>
    /// <param name="targetPosition">
    /// The requested movement destination.
    /// </param>
    public void RequestMove(
        string requestingActorId,
        Vector2 targetPosition)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        movementTarget = targetPosition;
        movementDirection = Vector2.zero;
        hasMovementTarget = true;
    }

    /// <summary>
    /// Requests that this actor stop moving.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the actor making the request.
    /// </param>
    public void RequestStop(string requestingActorId)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        movementDirection = Vector2.zero;
        hasMovementTarget = false;
    }

    /// <summary>
    /// Requests that this actor pass to another actor.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the passing actor.
    /// </param>
    /// <param name="targetActorId">
    /// The identifier of the intended receiver.
    /// </param>
    public void RequestPass(
        string requestingActorId,
        string targetActorId)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        kickController.PassToActor(targetActorId);
    }

    /// <summary>
    /// Requests that this actor pass toward a world position.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the passing actor.
    /// </param>
    /// <param name="targetPosition">
    /// The intended pass destination.
    /// </param>
    public void RequestPass(
        string requestingActorId,
        Vector2 targetPosition)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        kickController.PassToPosition(targetPosition);
    }

    /// <summary>
    /// Requests that this actor shoot toward a world position.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the shooting actor.
    /// </param>
    /// <param name="targetPosition">
    /// The intended shot destination.
    /// </param>
    public void RequestShoot(
        string requestingActorId,
        Vector2 targetPosition)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        kickController.ShootAtPosition(targetPosition);
    }

    /// <summary>
    /// Requests that this actor attempt to take possession of the ball.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the actor attempting possession.
    /// </param>
    public void RequestTakeBall(string requestingActorId)
    {
        if (!IsRequestForThisActor(requestingActorId))
            return;

        kickController.TryTakeBall();
    }

    /// <summary>
    /// Determines whether an action request belongs to this actor.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier included in the request.
    /// </param>
    /// <returns>
    /// True when the request belongs to this actor.
    /// </returns>
    private bool IsRequestForThisActor(
        string requestingActorId)
    {
        return requestingActorId == actorId;
    }

    /// <summary>
    /// Changes whether this actor currently possesses the ball.
    /// </summary>
    /// <param name="value">
    /// Whether the actor possesses the ball.
    /// </param>
    public void SetHasBall(bool value)
    {
        hasBall = value;
    }

    /// <summary>
    /// Configures the player actor.
    /// </summary>
    /// <param name="id">The player identifier.</param>
    /// <param name="team">The player's team.</param>
    /// <param name="role">The player's role.</param>
    /// <param name="formation">The player's formation position.</param>
    /// <param name="active">Whether the player is active.</param>
    /// <param name="aiControlled">
    /// Whether the player is AI controlled.
    /// </param>
    /// <param name="goalkeeper">
    /// Whether the player is a goalkeeper.
    /// </param>
    /// <param name="ball">Whether the player has the ball.</param>
    public void Initialize(
        string id,
        ETeamId team,
        EPlayerRole role,
        EFormationPosition formation,
        bool active,
        bool aiControlled,
        bool goalkeeper,
        bool ball)
    {
        actorId = id;
        teamId = team;
        playerRole = role;
        formationPosition = formation;
        isActive = active;
        isAIControlled = aiControlled;
        isGoalkeeper = goalkeeper;
        hasBall = ball;
    }
}