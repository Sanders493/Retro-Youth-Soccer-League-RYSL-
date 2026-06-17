using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects a defensive position that covers a dangerous opposing receiver
/// while remaining goal-side.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Cover Position",
    menuName = "Soccer AI/Nodes/Selection/Select Cover Position")]
public sealed class SelectCoverPositionNode :
    AIBehaviorTreeNode
{
    [Header("Coverage")]
    [Tooltip(
        "How far toward the defending goal the cover defender should stand " +
        "from the marked opponent.")]
    [SerializeField]
    private float goalSideOffset = 1f;

    [Tooltip(
        "The maximum distance from the actor at which an opponent may be " +
        "selected for coverage.")]
    [SerializeField]
    private float maximumCoverDistance = 10f;

    [Header("Scoring")]
    [SerializeField]
    private float goalDangerWeight = 2f;

    [SerializeField]
    private float actorDistanceWeight = 0.25f;

    [SerializeField]
    private float formationSideWeight = 0.5f;

    /// <summary>
    /// Selects and stores a defensive cover position.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a dangerous opponent is selected for coverage.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        IAIActor ballOwner =
            context.GameState.BallOwner;

        if (ballOwner == null
            || ballOwner.TeamId
                == context.Actor.TeamId)
        {
            return EBehaviorTreeResult.Failure;
        }

        ETeamId opposingTeam =
            GetOpposingTeam(
                context.Actor.TeamId);

        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                opposingTeam);

        if (opponents == null)
            return EBehaviorTreeResult.Failure;

        Vector2 defendingGoal =
            context.GameState.GetDefendingGoalPosition(
                context.Actor.TeamId);

        Vector2 formationWorldPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        Vector2 formationFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                formationWorldPosition);

        float maximumDistanceSquared =
            maximumCoverDistance
            * maximumCoverDistance;

        IAIActor bestOpponent = null;
        float bestScore = float.MinValue;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive
                || opponent.ActorId
                    == ballOwner.ActorId)
            {
                continue;
            }

            float actorDistanceSquared =
                (opponent.Position
                 - context.Actor.Position)
                .sqrMagnitude;

            if (actorDistanceSquared
                > maximumDistanceSquared)
            {
                continue;
            }

            float score =
                ScoreOpponent(
                    context,
                    opponent,
                    defendingGoal,
                    formationFieldPosition);

            if (bestOpponent != null
                && score <= bestScore)
            {
                continue;
            }

            bestOpponent = opponent;
            bestScore = score;
        }

        if (bestOpponent == null)
            return EBehaviorTreeResult.Failure;

        Vector2 towardGoal =
            defendingGoal
            - bestOpponent.Position;

        if (towardGoal.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 targetPosition =
            bestOpponent.Position
            + towardGoal.normalized
            * goalSideOffset;

        Vector2 fieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                targetPosition);

        fieldPosition.x =
            Mathf.Clamp01(
                fieldPosition.x);

        fieldPosition.y =
            Mathf.Clamp(
                fieldPosition.y,
                -1f,
                1f);

        context.SelectedActor =
            bestOpponent;

        context.SelectedPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                fieldPosition);

        context.HasSelectedPosition =
            true;

        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Scores an opponent according to defensive danger and formation fit.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="opponent">
    /// The opponent being scored.
    /// </param>
    /// <param name="defendingGoal">
    /// The actor team's defending goal.
    /// </param>
    /// <param name="formationFieldPosition">
    /// The actor's team-relative formation position.
    /// </param>
    /// <returns>The opponent's defensive danger score.</returns>
    private float ScoreOpponent(
        AIBehaviorContext context,
        IAIActor opponent,
        Vector2 defendingGoal,
        Vector2 formationFieldPosition)
    {
        float distanceToGoal =
            Vector2.Distance(
                opponent.Position,
                defendingGoal);

        float goalDanger =
            1f / Mathf.Max(
                distanceToGoal,
                0.01f);

        float actorDistance =
            Vector2.Distance(
                context.Actor.Position,
                opponent.Position);

        Vector2 opponentFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                opponent.Position);

        float formationSideDistance =
            Mathf.Abs(
                opponentFieldPosition.y
                - formationFieldPosition.y);

        return goalDanger
                * goalDangerWeight
            - actorDistance
                * actorDistanceWeight
            - formationSideDistance
                * formationSideWeight;
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    /// <param name="team">
    /// The actor's team.
    /// </param>
    /// <returns>The opposing team.</returns>
    private ETeamId GetOpposingTeam(
        ETeamId team)
    {
        return team == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        goalSideOffset =
            Mathf.Max(
                0f,
                goalSideOffset);

        maximumCoverDistance =
            Mathf.Max(
                0f,
                maximumCoverDistance);

        goalDangerWeight =
            Mathf.Max(
                0f,
                goalDangerWeight);

        actorDistanceWeight =
            Mathf.Max(
                0f,
                actorDistanceWeight);

        formationSideWeight =
            Mathf.Max(
                0f,
                formationSideWeight);
    }
#endif
}