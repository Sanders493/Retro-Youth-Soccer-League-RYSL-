using UnityEngine;

/// <summary>
/// Stores a player's team and tactical role while exposing the player to the AI
/// system.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerInfo : MonoBehaviour, IAIActor
{
    [Header("Actor")]
    [SerializeField] private string actorId;
    [SerializeField] private ETeamId teamId;
    [SerializeField] private bool isAIControlled = true;

    [Header("Role")]
    [SerializeField] private EPlayerRole playerRole;
    [SerializeField] private EFormationPosition formationPosition;
    
    [SerializeField] private SoccerBall soccerBall;

    /// <summary>
    /// Gets whether the player currently possesses the ball.
    /// </summary>
    public bool HasBall =>
        soccerBall != null
        && soccerBall.CurrentController == gameObject;
    
    /// <summary>
    /// Gets the player's unique identifier.
    /// </summary>
    public string ActorId => actorId;

    /// <summary>
    /// Gets the team this player belongs to.
    /// </summary>
    public ETeamId TeamId => teamId;

    /// <summary>
    /// Gets the player's current world position.
    /// </summary>
    public Vector2 Position => transform.position;


    /// <summary>
    /// Gets whether the player is currently active.
    /// </summary>
    public bool IsActive => gameObject.activeInHierarchy;

    /// <summary>
    /// Gets whether the player is controlled by the AI.
    /// </summary>
    public bool IsAIControlled => isAIControlled;

    /// <summary>
    /// Gets whether the player is a goalkeeper.
    /// </summary>
    public bool IsGoalkeeper => playerRole == EPlayerRole.Goalkeeper;

    
    /// <summary>
    /// Gets the player's gameplay role.
    /// </summary>
    public EPlayerRole PlayerRole => playerRole;

    /// <summary>
    /// Gets the player's assigned formation position.
    /// </summary>
    public EFormationPosition FormationPosition => formationPosition;



}