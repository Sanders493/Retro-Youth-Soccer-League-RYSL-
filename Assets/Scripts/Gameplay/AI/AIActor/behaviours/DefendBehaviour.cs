using UnityEngine;

/// <summary>
/// Positions AI-controlled actors defensively based on their role,
/// formation lane, the ball's location, and whether the ball is inside
/// the defending penalty box.
/// </summary>
public sealed class DefendBehavior : IAIBehavior
{
    private readonly int priority;

    /// <summary>
    /// Creates a defensive positioning behavior.
    /// </summary>
    /// <param name="priority">
    /// The priority of defending relative to other behaviors.
    /// </param>
    public DefendBehavior(int priority = 25)
    {
        this.priority = priority;
    }

    /// <summary>
    /// Determines whether the actor should use defensive positioning.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when the opposing team possesses the ball and the actor should
    /// recover defensively; otherwise, false.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        if (!context.GameState.IsMatchActive
            || !context.Actor.IsActive
            || !context.Actor.IsAIControlled
            || context.Actor.HasBall
            || context.IsPrimaryBallChaser)
        {
            return false;
        }

        if (context.GameState.TeamInPossession == ETeamId.None
            || context.GameState.HasPossession(context.Actor.TeamId))
        {
            return false;
        }

        bool ballIsInPenaltyBox =
            context.GameState.IsInsideDefendingPenaltyBox(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        if (ballIsInPenaltyBox)
            return context.Actor.PlayerRole != EPlayerRole.Goalkeeper;

        // return context.Actor.PlayerRole == EPlayerRole.Defender
            // || context.Actor.PlayerRole == EPlayerRole.Midfielder;
        return context.Actor.PlayerRole != EPlayerRole.Goalkeeper;
    }

    /// <summary>
    /// Returns the priority of the defensive behavior.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The configured defensive priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates a role-aware defensive movement assignment.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting defensive assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        Vector2 targetPosition = GetDefensiveTargetPosition(context);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            targetPosition,
            priority: priority);
    }

    /// <summary>
    /// Calculates the actor's defensive target using its role,
    /// formation side, and the ball's location.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The defensive target in world coordinates.</returns>
    private Vector2 GetDefensiveTargetPosition(
        AIBehaviorContext context)
    {
        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        bool ballIsInPenaltyBox =
            context.GameState.IsInsideDefendingPenaltyBox(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        float horizontalPosition =
            GetDefensiveHorizontalPosition(
                context.FormationPosition,
                ballFieldPosition.x,
                ballIsInPenaltyBox);

        float verticalPosition =
            GetDefensiveDepth(
                context.Actor.PlayerRole,
                ballFieldPosition.y,
                ballIsInPenaltyBox);

        Vector2 teamRelativeTarget = new Vector2(
            horizontalPosition,
            verticalPosition);

        return context.GameState.GetWorldPositionFromTeamRelative(
            context.Actor.TeamId,
            teamRelativeTarget);
    }

    /// <summary>
    /// Calculates the actor's horizontal defensive lane.
    /// </summary>
    /// <param name="formationPosition">
    /// The actor's assigned formation position.
    /// </param>
    /// <param name="ballHorizontalPosition">
    /// The ball's team-relative horizontal position.
    /// </param>
    /// <param name="ballIsInPenaltyBox">
    /// Whether the ball is inside the defending penalty box.
    /// </param>
    /// <returns>The target horizontal field position.</returns>
    private float GetDefensiveHorizontalPosition(
        EFormationPosition formationPosition,
        float ballHorizontalPosition,
        bool ballIsInPenaltyBox)
    {
        float formationLane = GetFormationLane(formationPosition);

        float ballInfluence = ballIsInPenaltyBox
            ? 0.75f
            : 0.5f;

        float targetLane = Mathf.Lerp(
            formationLane,
            ballHorizontalPosition,
            ballInfluence);

        return Mathf.Clamp(targetLane, -1f, 1f);
    }

    /// <summary>
    /// Returns the horizontal lane associated with a formation position.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position being evaluated.
    /// </param>
    /// <returns>
    /// -1 for left positions, 0 for center positions,
    /// and 1 for right positions.
    /// </returns>
    private float GetFormationLane(
        EFormationPosition formationPosition)
    {
        switch (formationPosition)
        {
            case EFormationPosition.LeftDefender:
            case EFormationPosition.LeftMidfielder:
            case EFormationPosition.LeftForward:
                return -1f;

            case EFormationPosition.RightDefender:
            case EFormationPosition.RightMidfielder:
            case EFormationPosition.RightForward:
                return 1f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Calculates how far from the defending goal the actor should position.
    /// </summary>
    /// <param name="playerRole">The actor's tactical role.</param>
    /// <param name="ballDepth">
    /// The ball's team-relative distance from the defending goal.
    /// </param>
    /// <param name="ballIsInPenaltyBox">
    /// Whether the ball is inside the defending penalty box.
    /// </param>
    /// <returns>
    /// A normalized depth where 0 is the defending goal and 1 is the
    /// attacking goal.
    /// </returns>
    private float GetDefensiveDepth(
        EPlayerRole playerRole,
        float ballDepth,
        bool ballIsInPenaltyBox)
    {
        if (ballIsInPenaltyBox)
        {
            switch (playerRole)
            {
                case EPlayerRole.Defender:
                    return Mathf.Clamp(
                        ballDepth - 0.03f,
                        0.05f,
                        0.2f);

                case EPlayerRole.Midfielder:
                    return Mathf.Clamp(
                        ballDepth + 0.05f,
                        0.12f,
                        0.28f);

                case EPlayerRole.Forward:
                    return Mathf.Clamp(
                        ballDepth + 0.12f,
                        0.2f,
                        0.35f);

                default:
                    return ballDepth;
            }
        }

        switch (playerRole)
        {
            case EPlayerRole.Defender:
                return Mathf.Clamp(
                    ballDepth - 0.12f,
                    0.1f,
                    0.4f);

            case EPlayerRole.Midfielder:
                return Mathf.Clamp(
                    ballDepth - 0.02f,
                    0.25f,
                    0.55f);

            case EPlayerRole.Forward:
                return Mathf.Clamp(
                    ballDepth + 0.15f,
                    0.4f,
                    0.7f);

            default:
                return ballDepth;
        }
    }
}