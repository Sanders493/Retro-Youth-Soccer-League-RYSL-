using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the current match information used by the soccer AI system.
/// </summary>
public class GameState : MonoBehaviour, IGameState
{
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

    
    public bool HasPossession(ETeamId TeamId)
    {
        throw new System.NotImplementedException();
    }
    

    [Header("Match")]
    [SerializeField] private bool isMatchActive;
    [SerializeField] private float remainingMatchTime;

    private readonly List<IAIActor> actors = new List<IAIActor>();
    private readonly List<IAIActor> playerTeamActors = new List<IAIActor>();
    private readonly List<IAIActor> enemyTeamActors = new List<IAIActor>();
    
    private IAIActor ballOwner;

    private bool hasActivePass;
    private string intendedPassReceiverId;
    private Vector2 intendedPassTargetPosition;

    [Header("Ball")]
    [SerializeField] private SoccerBall soccerBall;
    
    [Header("Field")]
    [SerializeField] private FieldBounds fieldBounds;

    [SerializeField] private Transform playerTeamGoal;
    [SerializeField] private Transform enemyTeamGoal;

    [SerializeField] private BoxCollider2D playerTeamPenaltyBox;
    [SerializeField] private BoxCollider2D enemyTeamPenaltyBox;
    //=======================================
    // BALL
    //=======================================
 
    /// <summary>
    /// Gets the ball's current world position.
    /// </summary>
    public Vector2 BallPosition
    {
        get
        {
            if (soccerBall == null)
                return Vector2.zero;

            return soccerBall.transform.position;
        }
    }

    /// <summary>
    /// Gets the ball's current velocity.
    /// </summary>
    public Vector2 BallVelocity
    {
        get
        {
            if (soccerBall == null)
                return Vector2.zero;

            return soccerBall.CurrentVelocity;
        }
    }
    
    //=======================================
    // FIELD
    //=======================================
    
    /// <summary>
    /// Gets the playable field bounds.
    /// </summary>
    public Bounds FieldBounds
    {
        get
        {
            if (fieldBounds == null)
                return new Bounds();

            return fieldBounds.Bounds;
        }
    }

    /// <summary>
    /// Determines whether a world position is inside the specified team's
    /// defending penalty box.
    /// </summary>
    /// <param name="teamId">The team defending the penalty box.</param>
    /// <param name="worldPosition">The world position being checked.</param>
    /// <returns>
    /// True when the position is inside the team's defending penalty box.
    /// </returns>
    public bool IsInsideDefendingPenaltyBox(
        ETeamId teamId,
        Vector2 worldPosition)
    {
        BoxCollider2D penaltyBox = GetDefendingPenaltyBox(teamId);

        if (penaltyBox == null || !penaltyBox.enabled)
            return false;

        return penaltyBox.OverlapPoint(worldPosition);
    }

    /// <summary>
    /// Gets the penalty-box collider defended by the specified team.
    /// </summary>
    /// <param name="teamId">The defending team.</param>
    /// <returns>
    /// The team's penalty-box collider, or null when the team is invalid.
    /// </returns>
    private BoxCollider2D GetDefendingPenaltyBox(ETeamId teamId)
    {
        switch (teamId)
        {
            case ETeamId.PlayerTeam:
                return playerTeamPenaltyBox;

            case ETeamId.EnemyTeam:
                return enemyTeamPenaltyBox;

            default:
                return null;
        }
    }
    
     /// <summary>
    /// Converts a world position into normalized coordinates from a team's
    /// perspective.
    /// </summary>
    /// <param name="teamId">The team whose perspective is used.</param>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>The normalized team-relative field position.</returns>
    public Vector2 GetTeamRelativeFieldPosition(
        ETeamId teamId,
        Vector2 worldPosition)
    {
        Bounds bounds = FieldBounds;

        if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            return Vector2.zero;

        float normalizedWorldX = Mathf.InverseLerp(
            bounds.min.x,
            bounds.max.x,
            worldPosition.x);

        float normalizedWorldY = Mathf.InverseLerp(
            bounds.min.y,
            bounds.max.y,
            worldPosition.y);

        float horizontal;
        float vertical;

        if (teamId == ETeamId.PlayerTeam)
        {
            horizontal = Mathf.Lerp(-1f, 1f, normalizedWorldX);
            vertical = normalizedWorldY;
        }
        else
        {
            horizontal = Mathf.Lerp(1f, -1f, normalizedWorldX);
            vertical = 1f - normalizedWorldY;
        }

        return new Vector2(horizontal, vertical);
    }


    /// <summary>
    /// Converts normalized team-relative coordinates into a world position.
    /// </summary>
    /// <param name="teamId">The team whose perspective is used.</param>
    /// <param name="teamRelativePosition">
    /// The normalized team-relative position.
    /// </param>
    /// <returns>The corresponding world position.</returns>
    public Vector2 GetWorldPositionFromTeamRelative(
        ETeamId teamId,
        Vector2 teamRelativePosition)
    {
        Bounds bounds = FieldBounds;

        float horizontal = Mathf.Clamp(
            teamRelativePosition.x,
            -1f,
            1f);

        float vertical = Mathf.Clamp01(
            teamRelativePosition.y);

        float normalizedX = Mathf.InverseLerp(
            -1f,
            1f,
            horizontal);

        float worldX;
        float worldY;

        if (teamId == ETeamId.PlayerTeam)
        {
            worldX = Mathf.Lerp(
                bounds.min.x,
                bounds.max.x,
                normalizedX);

            worldY = Mathf.Lerp(
                bounds.min.y,
                bounds.max.y,
                vertical);
        }
        else
        {
            worldX = Mathf.Lerp(
                bounds.max.x,
                bounds.min.x,
                normalizedX);

            worldY = Mathf.Lerp(
                bounds.max.y,
                bounds.min.y,
                vertical);
        }

        return new Vector2(worldX, worldY);
    }

    /// <summary>
    /// Converts a formation position into a world position.
    /// </summary>
    /// <param name="teamId">The team using the formation.</param>
    /// <param name="formationPosition">The requested formation position.</param>
    /// <returns>The formation position in world space.</returns>
    public Vector2 GetFormationWorldPosition(
        ETeamId teamId,
        EFormationPosition formationPosition)
    {
        Vector2 relativePosition;

        switch (formationPosition)
        {
            case EFormationPosition.Goalkeeper:
                relativePosition = new Vector2(0f, 0.05f);
                break;

            case EFormationPosition.LeftDefender:
                relativePosition = new Vector2(-0.55f, 0.25f);
                break;

            case EFormationPosition.CenterDefender:
                relativePosition = new Vector2(0f, 0.22f);
                break;

            case EFormationPosition.RightDefender:
                relativePosition = new Vector2(0.55f, 0.25f);
                break;

            case EFormationPosition.LeftMidfielder:
                relativePosition = new Vector2(-0.55f, 0.5f);
                break;

            case EFormationPosition.CenterMidfielder:
                relativePosition = new Vector2(0f, 0.48f);
                break;

            case EFormationPosition.RightMidfielder:
                relativePosition = new Vector2(0.55f, 0.5f);
                break;

            case EFormationPosition.LeftForward:
                relativePosition = new Vector2(-0.5f, 0.75f);
                break;

            case EFormationPosition.CenterForward:
                relativePosition = new Vector2(0f, 0.78f);
                break;

            case EFormationPosition.RightForward:
                relativePosition = new Vector2(0.5f, 0.75f);
                break;

            default:
                relativePosition = new Vector2(0f, 0.5f);
                break;
        }

        return GetWorldPositionFromTeamRelative(
            teamId,
            relativePosition);
    }

    /// <summary>
    /// Returns the goal the specified team is attacking.
    /// </summary>
    /// <param name="teamId">The attacking team.</param>
    /// <returns>The opposing goal position.</returns>
    public Vector2 GetAttackingGoalPosition(ETeamId teamId)
    {
        if (teamId == ETeamId.PlayerTeam)
        {
            if (enemyTeamGoal != null)
                return enemyTeamGoal.position;
        }
        else if (teamId == ETeamId.EnemyTeam)
        {
            if (playerTeamGoal != null)
                return playerTeamGoal.position;
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Returns the goal the specified team is defending.
    /// </summary>
    /// <param name="teamId">The defending team.</param>
    /// <returns>The team's own goal position.</returns>
    public Vector2 GetDefendingGoalPosition(ETeamId teamId)
    {
        if (teamId == ETeamId.PlayerTeam)
        {
            if (playerTeamGoal != null)
                return playerTeamGoal.position;
        }
        else if (teamId == ETeamId.EnemyTeam)
        {
            if (enemyTeamGoal != null)
                return enemyTeamGoal.position;
        }

        return Vector2.zero;
    }
}