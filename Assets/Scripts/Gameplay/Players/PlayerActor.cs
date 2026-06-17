using UnityEngine;

/// <summary>
/// This class represent all the attributes and behaviors of a player
/// </summary>

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerActor : MonoBehaviour, IAIActor
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

    private Vector2 currentVelocity;
    private Vector2 movementInput;

    private PlayerInputReader inputReader;

    public string ActorId => actorId;
    public ETeamId TeamId => teamId;

    public Vector2 Position
    {
        get => transform.position;

        set => transform.position = value;
    }

    public float MoveSpeed
    {
        get { return this.moveSpeed;}
        set { this.moveSpeed = value;}
    }
    public Vector2 Velocity => currentVelocity;
    public bool IsActive => isActive;
    public bool IsAIControlled => isAIControlled;
    public bool IsGoalkeeper => isGoalkeeper;
    public bool HasBall => hasBall;

    public EPlayerRole PlayerRole => playerRole;
    public EFormationPosition FormationPosition => formationPosition;

    /// <summary>
    /// Starts a player gameobject
    /// </summary>
    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    /// <summary>
    /// Reads the input from the user and runs the appropriate behaviors
    /// </summary>
    private void Update()
    {
        if (isAIControlled) return;

        ReadInput();
        MovePlayer();
    }

    /// <summary>
    /// Gets the input from the input readers
    /// </summary>
    private void ReadInput()
    {
        if (inputReader != null)
        {
            movementInput = inputReader.MovementInput;
        }
    }

    /// <summary>
    /// Moves the player in the desired direction
    /// </summary>
    private void MovePlayer()
    {
        Vector2 targetVelocity = movementInput * moveSpeed;

        float accelRate = (movementInput.magnitude > 0.1f) ? acceleration : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            accelRate * Time.deltaTime
        );

        Position += currentVelocity * Time.deltaTime;

    }

    /// <summary>
    /// Used for external setup
    /// </summary>
    /// <param name="id">the player id/name</param>
    /// <param name="team">the player team</param>
    /// <param name="role">the player's role</param>
    /// <param name="formation">the player's position in the formation</param>
    /// <param name="active">whether the player is active or not</param>
    /// <param name="aiControlled">whether the player is AI controlled or not</param>
    /// <param name="goalkeeper">whether the player is a goalkeeper or not</param>
    /// <param name="ball">whether the player has the ball or not</param>
    public void Initialize(
        string id,
        ETeamId team,
        EPlayerRole role,
        EFormationPosition formation,
        bool active,
        bool aiControlled,
        bool goalkeeper,
        bool ball
    )
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