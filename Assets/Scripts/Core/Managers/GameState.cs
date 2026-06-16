using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stores the current match information used by the soccer AI system.
/// </summary>
public class GameState : MonoBehaviour, IGameState
{
    [Header("Match")]
    [SerializeField] private bool isMatchActive;

    private readonly List<IAIActor> _registeredActors = new();
    private readonly List<IAIActor> _activeActors = new();
    private readonly List<IAIActor> _teamActors = new();

    [Header("Ball")]
    [SerializeField] private SoccerBall soccerBall;

    [Header("Field")]
    [SerializeField] private FieldBounds fieldBounds;
    [SerializeField] private Transform playerTeamGoal;
    [SerializeField] private Transform enemyTeamGoal;
    [SerializeField] private BoxCollider2D playerTeamPenaltyBox;
    [SerializeField] private BoxCollider2D enemyTeamPenaltyBox;

    [Header("Formation Positions")]
    [SerializedDictionary(
        "Formation Position",
        "Team Relative Position")]
    [SerializeField]
    private SerializedDictionary<EFormationPosition, Vector2>
        formationPositions = new();

    [Header("Formation Gizmos")]
    [SerializeField] private bool showFormationGizmos = true;
    [SerializeField] private bool showPlayerTeamFormation = true;
    [SerializeField] private bool showEnemyTeamFormation = true;
    [SerializeField] private float formationGizmoRadius = 0.2f;
    [SerializeField] private Color playerTeamFormationColor = Color.blue;
    [SerializeField] private Color enemyTeamFormationColor = Color.red;

    /// <summary>
    /// Gets whether normal match gameplay is currently active.
    /// </summary>
    public bool IsMatchActive
    {
        get => isMatchActive;
        private set => isMatchActive = value;
    }
 
    /// <summary>
    /// Gets whether a pass is currently active.
    /// </summary>
    public bool HasActivePass
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the identifier of the intended pass receiver.
    /// </summary>
    public string IntendedPassReceiverId
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the intended world-space pass destination.
    /// </summary>
    public Vector2 IntendedPassTargetPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the team that currently possesses or most recently controlled
    /// the ball.
    /// </summary>
    public ETeamId TeamInPossession
    {
        get
        {
            IAIActor owner = BallOwner;

            if (owner != null)
                return owner.TeamId;

            IAIActor lastKicker = GetActorFromGameObject(
                soccerBall != null
                    ? soccerBall.LastKicker
                    : null);

            return lastKicker != null
                ? lastKicker.TeamId
                : ETeamId.None;
        }
    }

    /// <summary>
    /// Gets whether a player currently has full control of the ball.
    /// </summary>
    public bool HasBallOwner => BallOwner != null;

    /// <summary>
    /// Gets the actor currently in full control of the ball.
    /// </summary>
    public IAIActor BallOwner
    {
        get
        {
            if (soccerBall == null)
                return null;

            return GetActorFromGameObject(
                soccerBall.CurrentController);
        }
    }

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
    /// Adds default formation entries when this component is first added.
    /// </summary>
    private void Reset()
    {
        EnsureFormationPositions();
    }

    /// <summary>
    /// Validates formation settings when values are changed in the editor.
    /// </summary>
    private void OnValidate()
    {
        formationGizmoRadius =
            Mathf.Max(0.01f, formationGizmoRadius);

        EnsureFormationPositions();
        ClampFormationPositions();
    }

    /// <summary>
    /// Registers an actor with the match state.
    /// </summary>
    /// <param name="actor">The actor to register.</param>
    public void RegisterActor(IAIActor actor)
    {
        if (actor == null
            || _registeredActors.Contains(actor))
        {
            return;
        }

        _registeredActors.Add(actor);
    }

    /// <summary>
    /// Removes an actor from the match state.
    /// </summary>
    /// <param name="actor">The actor to remove.</param>
    public void UnregisterActor(IAIActor actor)
    {
        if (actor == null)
            return;

        _registeredActors.Remove(actor);
    }

    /// <summary>
    /// Returns every active actor in the match.
    /// </summary>
    /// <returns>A read-only collection of active match actors.</returns>
    public IReadOnlyList<IAIActor> GetAllActors()
    {
        _activeActors.Clear();

        for (int i = _registeredActors.Count - 1;
             i >= 0;
             i--)
        {
            IAIActor actor = _registeredActors[i];

            if (actor == null)
            {
                _registeredActors.RemoveAt(i);
                continue;
            }

            if (actor.IsActive)
                _activeActors.Add(actor);
        }

        return _activeActors;
    }

    /// <summary>
    /// Returns every active actor belonging to the specified team.
    /// </summary>
    /// <param name="teamId">
    /// The team whose actors should be returned.
    /// </param>
    /// <returns>
    /// A read-only collection of active actors belonging to the team.
    /// </returns>
    public IReadOnlyList<IAIActor> GetTeamActors(
        ETeamId teamId)
    {
        _teamActors.Clear();

        for (int i = _registeredActors.Count - 1;
             i >= 0;
             i--)
        {
            IAIActor actor = _registeredActors[i];

            if (actor == null)
            {
                _registeredActors.RemoveAt(i);
                continue;
            }

            if (actor.IsActive
                && actor.TeamId == teamId)
            {
                _teamActors.Add(actor);
            }
        }

        return _teamActors;
    }

    /// <summary>
    /// Returns the actor with the specified identifier.
    /// </summary>
    /// <param name="actorId">
    /// The unique actor identifier.
    /// </param>
    /// <returns>
    /// The matching actor, or null when no actor is found.
    /// </returns>
    public IAIActor GetActor(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
            return null;

        for (int i = _registeredActors.Count - 1;
             i >= 0;
             i--)
        {
            IAIActor actor = _registeredActors[i];

            if (actor == null)
            {
                _registeredActors.RemoveAt(i);
                continue;
            }

            if (actor.ActorId == actorId)
                return actor;
        }

        return null;
    }

    /// <summary>
    /// Marks the match as active.
    /// </summary>
    public void StartMatch()
    {
        IsMatchActive = true;
    }

    /// <summary>
    /// Marks the match as inactive.
    /// </summary>
    public void StopMatch()
    {
        IsMatchActive = false;
    }

    /// <summary>
    /// Records an active pass intended for a specific actor.
    /// </summary>
    /// <param name="receiverId">The identifier of the intended receiver.</param>
    /// <param name="targetPosition">The receiver's current world position.</param>
    public void BeginPass(
        string receiverId,
        Vector2 targetPosition)
    {
        HasActivePass = true;
        IntendedPassReceiverId = receiverId;
        IntendedPassTargetPosition = targetPosition;
    }

    /// <summary>
    /// Records an active pass toward a world-space position.
    /// </summary>
    /// <param name="targetPosition">The intended pass destination.</param>
    public void BeginPass(Vector2 targetPosition)
    {
        HasActivePass = true;
        IntendedPassReceiverId = string.Empty;
        IntendedPassTargetPosition = targetPosition;
    }

    /// <summary>
    /// Clears the currently active pass information.
    /// </summary>
    public void ClearActivePass()
    {
        HasActivePass = false;
        IntendedPassReceiverId = string.Empty;
        IntendedPassTargetPosition = Vector2.zero;
    }

    /// <summary>
    /// Determines whether the specified team currently possesses the ball.
    /// </summary>
    /// <param name="teamId">The team to check.</param>
    /// <returns>
    /// True when the team possesses or most recently controlled the ball.
    /// </returns>
    public bool HasPossession(ETeamId teamId)
    {
        if (teamId == ETeamId.None)
            return false;

        return TeamInPossession == teamId;
    }

    /// <summary>
    /// Finds an AI actor attached to the supplied GameObject.
    /// </summary>
    /// <param name="target">
    /// The GameObject to search.
    /// </param>
    /// <returns>
    /// The attached AI actor, or null when one cannot be found.
    /// </returns>
    private IAIActor GetActorFromGameObject(
        GameObject target)
    {
        if (target == null)
            return null;

        MonoBehaviour[] components =
            target.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is IAIActor actor)
                return actor;
        }

        return null;
    }

    /// <summary>
    /// Determines whether a world position is inside the specified team's
    /// defending penalty box.
    /// </summary>
    /// <param name="teamId">
    /// The team defending the penalty box.
    /// </param>
    /// <param name="worldPosition">
    /// The world position being checked.
    /// </param>
    /// <returns>
    /// True when the position is inside the team's defending penalty box.
    /// </returns>
    public bool IsInsideDefendingPenaltyBox(
        ETeamId teamId,
        Vector2 worldPosition)
    {
        BoxCollider2D penaltyBox =
            GetDefendingPenaltyBox(teamId);

        if (penaltyBox == null
            || !penaltyBox.enabled)
        {
            return false;
        }

        return penaltyBox.OverlapPoint(worldPosition);
    }

    /// <summary>
    /// Gets the penalty-box collider defended by the specified team.
    /// </summary>
    /// <param name="teamId">The defending team.</param>
    /// <returns>
    /// The team's penalty-box collider, or null when the team is invalid.
    /// </returns>
    private BoxCollider2D GetDefendingPenaltyBox(
        ETeamId teamId)
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
    /// <param name="teamId">
    /// The team whose perspective is used.
    /// </param>
    /// <param name="worldPosition">
    /// The world position to convert.
    /// </param>
    /// <returns>
    /// The normalized team-relative field position.
    /// </returns>
    public Vector2 GetTeamRelativeFieldPosition(
        ETeamId teamId,
        Vector2 worldPosition)
    {
        Bounds bounds = FieldBounds;

        if (bounds.size.x <= 0f
            || bounds.size.y <= 0f)
        {
            return Vector2.zero;
        }

        float normalizedWorldX =
            Mathf.InverseLerp(
                bounds.min.x,
                bounds.max.x,
                worldPosition.x);

        float normalizedWorldY =
            Mathf.InverseLerp(
                bounds.min.y,
                bounds.max.y,
                worldPosition.y);

        float horizontal;
        float vertical;

        if (teamId == ETeamId.PlayerTeam)
        {
            horizontal =
                Mathf.Lerp(
                    -1f,
                    1f,
                    normalizedWorldX);

            vertical = normalizedWorldY;
        }
        else
        {
            horizontal =
                Mathf.Lerp(
                    1f,
                    -1f,
                    normalizedWorldX);

            vertical = 1f - normalizedWorldY;
        }

        return new Vector2(
            horizontal,
            vertical);
    }

    /// <summary>
    /// Converts normalized team-relative coordinates into a world position.
    /// </summary>
    /// <param name="teamId">
    /// The team whose perspective is used.
    /// </param>
    /// <param name="teamRelativePosition">
    /// The normalized team-relative position.
    /// </param>
    /// <returns>
    /// The corresponding world position.
    /// </returns>
    public Vector2 GetWorldPositionFromTeamRelative(
        ETeamId teamId,
        Vector2 teamRelativePosition)
    {
        Bounds bounds = FieldBounds;

        float horizontal =
            Mathf.Clamp(
                teamRelativePosition.x,
                -1f,
                1f);

        float vertical =
            Mathf.Clamp01(
                teamRelativePosition.y);

        float normalizedX =
            Mathf.InverseLerp(
                -1f,
                1f,
                horizontal);

        float worldX;
        float worldY;

        if (teamId == ETeamId.PlayerTeam)
        {
            worldX =
                Mathf.Lerp(
                    bounds.min.x,
                    bounds.max.x,
                    normalizedX);

            worldY =
                Mathf.Lerp(
                    bounds.min.y,
                    bounds.max.y,
                    vertical);
        }
        else
        {
            worldX =
                Mathf.Lerp(
                    bounds.max.x,
                    bounds.min.x,
                    normalizedX);

            worldY =
                Mathf.Lerp(
                    bounds.max.y,
                    bounds.min.y,
                    vertical);
        }

        return new Vector2(
            worldX,
            worldY);
    }

    /// <summary>
    /// Converts a formation position into a world position.
    /// </summary>
    /// <param name="teamId">
    /// The team using the formation.
    /// </param>
    /// <param name="formationPosition">
    /// The requested formation position.
    /// </param>
    /// <returns>
    /// The formation position in world space.
    /// </returns>
    public Vector2 GetFormationWorldPosition(
        ETeamId teamId,
        EFormationPosition formationPosition)
    {
        if (!formationPositions.TryGetValue(
                formationPosition,
                out Vector2 relativePosition))
        {
            relativePosition =
                new Vector2(0f, 0.5f);
        }

        relativePosition.x =
            Mathf.Clamp(
                relativePosition.x,
                -1f,
                1f);

        relativePosition.y =
            Mathf.Clamp01(
                relativePosition.y);

        return GetWorldPositionFromTeamRelative(
            teamId,
            relativePosition);
    }

    /// <summary>
    /// Returns the goal the specified team is attacking.
    /// </summary>
    /// <param name="teamId">
    /// The attacking team.
    /// </param>
    /// <returns>
    /// The opposing goal position.
    /// </returns>
    public Vector2 GetAttackingGoalPosition(
        ETeamId teamId)
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
    /// <param name="teamId">
    /// The defending team.
    /// </param>
    /// <returns>
    /// The team's own goal position.
    /// </returns>
    public Vector2 GetDefendingGoalPosition(
        ETeamId teamId)
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

    /// <summary>
    /// Ensures every formation position has an editor entry.
    /// </summary>
    private void EnsureFormationPositions()
    {
        if (formationPositions == null)
        {
            formationPositions =
                new SerializedDictionary<
                    EFormationPosition,
                    Vector2>();
        }

        AddFormationPositionIfMissing(
            EFormationPosition.Goalkeeper,
            new Vector2(0f, 0.05f));

        AddFormationPositionIfMissing(
            EFormationPosition.LeftDefender,
            new Vector2(-0.55f, 0.25f));

        AddFormationPositionIfMissing(
            EFormationPosition.CenterDefender,
            new Vector2(0f, 0.22f));

        AddFormationPositionIfMissing(
            EFormationPosition.RightDefender,
            new Vector2(0.55f, 0.25f));

        AddFormationPositionIfMissing(
            EFormationPosition.LeftMidfielder,
            new Vector2(-0.55f, 0.5f));

        AddFormationPositionIfMissing(
            EFormationPosition.CenterMidfielder,
            new Vector2(0f, 0.48f));

        AddFormationPositionIfMissing(
            EFormationPosition.RightMidfielder,
            new Vector2(0.55f, 0.5f));

        AddFormationPositionIfMissing(
            EFormationPosition.LeftForward,
            new Vector2(-0.5f, 0.75f));

        AddFormationPositionIfMissing(
            EFormationPosition.CenterForward,
            new Vector2(0f, 0.78f));

        AddFormationPositionIfMissing(
            EFormationPosition.RightForward,
            new Vector2(0.5f, 0.75f));
    }

    /// <summary>
    /// Adds a default formation position when the entry is missing.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position being checked.
    /// </param>
    /// <param name="relativePosition">
    /// The default team-relative position.
    /// </param>
    private void AddFormationPositionIfMissing(
        EFormationPosition formationPosition,
        Vector2 relativePosition)
    {
        if (formationPositions.ContainsKey(
                formationPosition))
        {
            return;
        }

        formationPositions.Add(
            formationPosition,
            relativePosition);
    }

    /// <summary>
    /// Restricts editor formation values to the supported normalized range.
    /// </summary>
    private void ClampFormationPositions()
    {
        if (formationPositions == null)
            return;

        List<EFormationPosition> keys =
            new List<EFormationPosition>(
                formationPositions.Keys);

        foreach (EFormationPosition key in keys)
        {
            Vector2 position =
                formationPositions[key];

            position.x =
                Mathf.Clamp(
                    position.x,
                    -1f,
                    1f);

            position.y =
                Mathf.Clamp01(
                    position.y);

            formationPositions[key] = position;
        }
    }

    /// <summary>
    /// Draws the configured formation positions in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showFormationGizmos
            || fieldBounds == null
            || formationPositions == null)
        {
            return;
        }

        if (showPlayerTeamFormation)
        {
            DrawFormationGizmos(
                ETeamId.PlayerTeam,
                playerTeamFormationColor);
        }

        if (showEnemyTeamFormation)
        {
            DrawFormationGizmos(
                ETeamId.EnemyTeam,
                enemyTeamFormationColor);
        }
    }

    /// <summary>
    /// Draws every formation position for one team.
    /// </summary>
    /// <param name="teamId">
    /// The team whose formation is being drawn.
    /// </param>
    /// <param name="gizmoColor">
    /// The color used for the formation gizmos.
    /// </param>
    private void DrawFormationGizmos(
        ETeamId teamId,
        Color gizmoColor)
    {
        Gizmos.color = gizmoColor;

        foreach (
            KeyValuePair<EFormationPosition, Vector2> entry
            in formationPositions)
        {
            Vector2 relativePosition =
                new Vector2(
                    Mathf.Clamp(
                        entry.Value.x,
                        -1f,
                        1f),
                    Mathf.Clamp01(
                        entry.Value.y));

            Vector2 worldPosition =
                GetWorldPositionFromTeamRelative(
                    teamId,
                    relativePosition);

            Gizmos.DrawWireSphere(
                worldPosition,
                formationGizmoRadius);

            Gizmos.DrawLine(
                worldPosition
                    + Vector2.left
                    * formationGizmoRadius,
                worldPosition
                    + Vector2.right
                    * formationGizmoRadius);

            Gizmos.DrawLine(
                worldPosition
                    + Vector2.down
                    * formationGizmoRadius,
                worldPosition
                    + Vector2.up
                    * formationGizmoRadius);

#if UNITY_EDITOR
            Handles.color = gizmoColor;

            Handles.Label(
                worldPosition
                    + Vector2.up
                    * formationGizmoRadius
                    * 1.5f,
                $"{teamId}: {entry.Key}");
#endif
        }
    }
}