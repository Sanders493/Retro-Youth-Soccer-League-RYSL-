using UnityEngine;

/// <summary>
/// Represents the attributes and behaviors of a player actor.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerKickController))]
public class PlayerActor :
    MonoBehaviour,
    IAIActor,
    IAIActionOutput
{
    [Header("Identity")]
    [SerializeField] private string actorId;
    [SerializeField] private ETeamId teamId;
    [SerializeField] private EFormationPosition formationPosition;
    [SerializeField] private bool isAIControlled;

 

    [Header("State")]
    [SerializeField] private bool isActive;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 14f;
    [SerializeField] private float stoppingDistance = 0.05f;
    
    [Header("Player Shooting")]
    [SerializeField] private float playerShotAimDistance = 10f;

    private Vector2 lastPlayerAimDirection = Vector2.right;

    private Vector2 currentVelocity;
    private Vector2 movementDirection;
    private Vector2 movementTarget;

    private bool hasMovementTarget;

    private Rigidbody2D rigidbodyComponent;
    private PlayerInputReader inputReader;
    private PlayerKickController kickController;

    public string ActorId => actorId;
    public ETeamId TeamId => teamId;
    public Vector2 Velocity => currentVelocity;
    public bool IsActive => isActive;
    public bool IsAIControlled => isAIControlled;

    public EFormationPosition FormationPosition => formationPosition;

    public EPlayerRole PlayerRole =>
        GetPlayerRole(formationPosition);

    public bool IsGoalkeeper =>
        formationPosition == EFormationPosition.Goalkeeper;
    public bool HasBall => kickController != null && kickController.HasBall;

    public IAIActionOutput ActionOutput => this;

    public Vector2 Position
    {
        get => rigidbodyComponent != null
            ? rigidbodyComponent.position
            : transform.position;

        set
        {
            if (rigidbodyComponent != null)
            {
                rigidbodyComponent.position = value;
                return;
            }

            transform.position = value;
        }
    }
    [SerializeField] private GameState gameState;

    /// <summary>
    /// Registers this actor with the game state.
    /// </summary>
    private void OnEnable()
    {
        if (gameState == null)
        {
            Debug.LogError(
                $"{name}: No GameState has been assigned.",
                this);

            return;
        }

        gameState.RegisterActor(this);
    }

    /// <summary>
    /// Removes this actor from the game state.
    /// </summary>
    private void OnDisable()
    {
        if (gameState != null)
            gameState.UnregisterActor(this);
    }
    /// <summary>
    /// Retrieves the components used by the player.
    /// </summary>
    private void Awake()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        inputReader = GetComponent<PlayerInputReader>();
        kickController = GetComponent<PlayerKickController>();
    }

    /// <summary>
    /// Reads input for human-controlled actors.
    /// </summary>
    private void Update()
    {
        if (!isActive || isAIControlled)
            return;

        ReadPlayerInput();
    }

    /// <summary>
    /// Applies movement during the physics update.
    /// </summary>
    private void FixedUpdate()
    {
        if (!isActive)
        {
            currentVelocity = Vector2.zero;
            return;
        }

        UpdateMovement();
    }
    
  

    /// <summary>
    /// Gets the general player role associated with a formation position.
    /// </summary>
    /// <param name="position">
    /// The player's specific formation position.
    /// </param>
    /// <returns>
    /// The general role associated with the formation position.
    /// </returns>
    private static EPlayerRole GetPlayerRole(
        EFormationPosition position)
    {
        switch (position)
        {
            case EFormationPosition.Goalkeeper:
                return EPlayerRole.Goalkeeper;

            case EFormationPosition.LeftDefender:
            case EFormationPosition.CenterDefender:
            case EFormationPosition.RightDefender:
                return EPlayerRole.Defender;

            case EFormationPosition.LeftMidfielder:
            case EFormationPosition.CenterMidfielder:
            case EFormationPosition.RightMidfielder:
                return EPlayerRole.Midfielder;

            case EFormationPosition.LeftForward:
            case EFormationPosition.CenterForward:
            case EFormationPosition.RightForward:
                return EPlayerRole.Forward;

            default:
                return EPlayerRole.Midfielder;
        }
    }

    /// <summary>
    /// Converts player input into the same requests used by AI actors.
    /// </summary>
    private void ReadPlayerInput()
    {
        if (inputReader == null || kickController == null)
            return;

        Vector2 inputDirection = inputReader.MovementInput;

        RequestMovementDirection(inputDirection);

        if (inputDirection.sqrMagnitude > 0.01f)
            lastPlayerAimDirection = inputDirection.normalized;

        if (inputReader.PassPressed && HasBall)
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
                RequestPass(
                    actorId,
                    Position + lastPlayerAimDirection);
            }
        }

        if (inputReader.ShootTakeBallPressed)
        {
            if (HasBall)
            {
                Vector2 shotTarget =
                    Position +
                    lastPlayerAimDirection *
                    playerShotAimDistance;

                RequestShoot(
                    actorId,
                    shotTarget);
            }
            else
            {
                RequestTakeBall(actorId);
            }
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
                movementTarget - rigidbodyComponent.position;

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
            accelerationRate * Time.fixedDeltaTime);

        Vector2 nextPosition =
            rigidbodyComponent.position +
            currentVelocity * Time.fixedDeltaTime;

        rigidbodyComponent.MovePosition(nextPosition);
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
        if (!IsRequestForThisActor(requestingActorId) ||
            !HasBall)
        {
            return;
        }

        kickController.PassToActor(
            targetActorId,
            gameObject);
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
        if (!IsRequestForThisActor(requestingActorId) ||
            !HasBall)
        {
            return;
        }

        kickController.PassToPosition(
            targetPosition,
            gameObject);
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
        if (!IsRequestForThisActor(requestingActorId) ||
            !HasBall)
        {
            return;
        }

        kickController.ShootAtPosition(
            targetPosition,
            gameObject);
    }

    /// <summary>
    /// Requests that this actor attempt to take possession of the ball.
    /// </summary>
    /// <param name="requestingActorId">
    /// The identifier of the actor attempting possession.
    /// </param>
    public void RequestTakeBall(string requestingActorId)
    {
        if (!IsRequestForThisActor(requestingActorId) ||
            HasBall)
        {
            return;
        }

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
    /// Configures the player actor.
    /// </summary>
    /// <param name="id">The player identifier.</param>
    /// <param name="team">The player's team.</param>
    /// <param name="formation">The player's formation position.</param>
    /// <param name="active">Whether the player is active.</param>
    /// <param name="aiControlled">
    /// Whether the player is AI controlled.
    /// </param>
    /// <param name="startsWithBall">
    /// Whether this player begins with possession.
    /// </param>
    public void Initialize(
        string id,
        ETeamId team,
        EFormationPosition formation,
        bool active,
        bool aiControlled,
        bool startsWithBall)
    {
        actorId = id;
        teamId = team;
        formationPosition = formation;
        isActive = active;
        isAIControlled = aiControlled;

        if (startsWithBall && kickController != null)
            kickController.TakeStartingPossession();
    }
}