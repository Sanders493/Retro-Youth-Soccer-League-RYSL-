using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects a safe forward position for an actor currently controlling the
/// ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Advance Position",
    menuName = "Soccer AI/Nodes/Selection/Select Advance Position")]
public sealed class SelectAdvancePositionNode :
    AIBehaviorTreeNode
{
    [Header("Advance Shape")]
    [Tooltip(
        "The team-relative forward distance considered for advancement.")]
    [SerializeField]
    private float forwardDistance = 0.12f;

    [Tooltip(
        "The team-relative lateral offset considered on either side.")]
    [SerializeField]
    private float lateralOffset = 0.12f;

    [Header("Scoring")]
    [SerializeField]
    private float forwardProgressWeight = 1f;

    [SerializeField]
    private float opponentSpaceWeight = 1f;

    [SerializeField]
    private float formationSideWeight = 0.5f;

    /// <summary>
    /// Selects and stores a safe position farther toward the opponent goal.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when an advance position is selected.
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

        Vector2 actorFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.Actor.Position);

        Vector2 formationWorldPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        Vector2 formationFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                formationWorldPosition);

        Vector2[] offsets =
        {
            new Vector2(
                forwardDistance,
                0f),

            new Vector2(
                forwardDistance,
                -lateralOffset),

            new Vector2(
                forwardDistance,
                lateralOffset)
        };

        bool foundTarget = false;
        float bestScore = float.MinValue;
        Vector2 bestTarget = default;

        foreach (Vector2 offset in offsets)
        {
            Vector2 candidateFieldPosition =
                actorFieldPosition
                + offset;

            candidateFieldPosition.x =
                Mathf.Clamp01(
                    candidateFieldPosition.x);

            candidateFieldPosition.y =
                Mathf.Clamp(
                    candidateFieldPosition.y,
                    -1f,
                    1f);

            Vector2 candidateWorldPosition =
                context.GameState
                    .GetWorldPositionFromTeamRelative(
                        context.Actor.TeamId,
                        candidateFieldPosition);

            float score =
                ScoreCandidate(
                    context,
                    candidateFieldPosition,
                    candidateWorldPosition,
                    formationFieldPosition);

            if (foundTarget
                && score <= bestScore)
            {
                continue;
            }

            foundTarget = true;
            bestScore = score;
            bestTarget = candidateWorldPosition;
        }

        if (!foundTarget)
            return EBehaviorTreeResult.Failure;

        context.ClearSelectedActor();

        context.SetSelectedPosition(bestTarget);


        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Scores one possible advance position.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="candidateFieldPosition">
    /// The candidate's team-relative position.
    /// </param>
    /// <param name="candidateWorldPosition">
    /// The candidate's world-space position.
    /// </param>
    /// <param name="formationFieldPosition">
    /// The actor's team-relative formation position.
    /// </param>
    /// <returns>The candidate score.</returns>
    private float ScoreCandidate(
        AIBehaviorContext context,
        Vector2 candidateFieldPosition,
        Vector2 candidateWorldPosition,
        Vector2 formationFieldPosition)
    {
        float forwardProgress =
            candidateFieldPosition.x;

        float opponentDistance =
            GetNearestOpponentDistance(
                context,
                candidateWorldPosition);

        float formationSideDistance =
            Mathf.Abs(
                candidateFieldPosition.y
                - formationFieldPosition.y);

        return forwardProgress
                * forwardProgressWeight
            + opponentDistance
                * opponentSpaceWeight
            - formationSideDistance
                * formationSideWeight;
    }

    /// <summary>
    /// Gets the distance from a position to the nearest opponent.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="position">
    /// The proposed world-space position.
    /// </param>
    /// <returns>The nearest opponent distance.</returns>
    private float GetNearestOpponentDistance(
        AIBehaviorContext context,
        Vector2 position)
    {
        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                GetOpposingTeam(
                    context.Actor.TeamId));

        if (opponents == null
            || opponents.Count == 0)
        {
            return 0f;
        }

        float nearestDistance =
            float.MaxValue;

        bool foundOpponent = false;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive)
            {
                continue;
            }

            foundOpponent = true;

            nearestDistance =
                Mathf.Min(
                    nearestDistance,
                    Vector2.Distance(
                        position,
                        opponent.Position));
        }

        return foundOpponent
            ? nearestDistance
            : 0f;
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    /// <param name="team">
    /// The current team.
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
        forwardDistance =
            Mathf.Max(
                0f,
                forwardDistance);

        lateralOffset =
            Mathf.Max(
                0f,
                lateralOffset);

        forwardProgressWeight =
            Mathf.Max(
                0f,
                forwardProgressWeight);

        opponentSpaceWeight =
            Mathf.Max(
                0f,
                opponentSpaceWeight);

        formationSideWeight =
            Mathf.Max(
                0f,
                formationSideWeight);
    }
#endif
}