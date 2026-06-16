using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the current match information used by the soccer AI system.
/// </summary>
public class GameState : MonoBehaviour, IGameState
{
    [Header("Field")]
    [SerializeField] private BoxCollider2D fieldBoundsCollider;
    [SerializeField] private Transform playerTeamGoal;
    [SerializeField] private Transform enemyTeamGoal;

    [Header("Penalty Boxes")]
    [SerializeField] private BoxCollider2D playerTeamPenaltyBox;
    [SerializeField] private BoxCollider2D enemyTeamPenaltyBox;

    [Header("Match")]
    [SerializeField] private bool isMatchActive;
    [SerializeField] private float remainingMatchTime;

    private readonly List<IAIActor> actors = new List<IAIActor>();
    private readonly List<IAIActor> playerTeamActors = new List<IAIActor>();
    private readonly List<IAIActor> enemyTeamActors = new List<IAIActor>();

    private Vector2 ballPosition;
    private Vector2 ballVelocity;
    private IAIActor ballOwner;

    private bool hasActivePass;
    private string intendedPassReceiverId;
    private Vector2 intendedPassTargetPosition;

    public Vector2 BallPosition => ballPosition;

    public Vector2 BallVelocity => ballVelocity;

    public Bounds FieldBounds =>
        fieldBoundsCollider != null
            ? fieldBoundsCollider.bounds
            : default;

    public ETeamId TeamInPossession =>
        ballOwner != null
            ? ballOwner.TeamId
            : ETeamId.None;

    public bool HasBallOwner => ballOwner != null;

    public IAIActor BallOwner => ballOwner;

    public bool IsMatchActive => isMatchActive;

    public float RemainingMatchTime => remainingMatchTime;

    public bool HasActivePass => hasActivePass;

    public string IntendedPassReceiverId =>
        intendedPassReceiverId;

    public Vector2 IntendedPassTargetPosition =>
        intendedPassTargetPosition;


    public IReadOnlyList<IAIActor> GetAllActors()
    {
        throw new System.NotImplementedException();
    }

    public IReadOnlyList<IAIActor> GetTeamActors(ETeamId TeamId)
    {
        throw new System.NotImplementedException();
    }

    public IAIActor GetActor(string actorId)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetTeamRelativeFieldPosition(ETeamId teamId, Vector2 worldPosition)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInsideDefendingPenaltyBox(ETeamId teamId, Vector2 worldPosition)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetWorldPositionFromTeamRelative(ETeamId teamId, Vector2 teamRelativePosition)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetFormationWorldPosition(ETeamId teamId, EFormationPosition formationPosition)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetAttackingGoalPosition(ETeamId TeamId)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetDefendingGoalPosition(ETeamId TeamId)
    {
        throw new System.NotImplementedException();
    }

    public bool HasPossession(ETeamId TeamId)
    {
        throw new System.NotImplementedException();
    }
}