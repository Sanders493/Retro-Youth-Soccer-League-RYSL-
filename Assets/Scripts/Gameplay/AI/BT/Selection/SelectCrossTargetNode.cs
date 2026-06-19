using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects the best centrally positioned teammate to receive a cross.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Cross Target",
    menuName = "Soccer AI/Nodes/Selection/Select Cross Target")]
public sealed class SelectCrossTargetNode :
    AIBehaviorTreeNode
{
    [Header("Target Region")]
    [Tooltip(
        "The minimum team-relative depth required for a cross target.")]
    [SerializeField, Range(0f, 1f)]
    private float minimumTargetDepth = 0.65f;

    [Tooltip(
        "The maximum absolute lateral value considered central.")]
    [SerializeField, Range(0f, 1f)]
    private float centerWidth = 0.35f;

    [Header("Scoring")]
    [SerializeField]
    private float forwardProgressWeight = 1f;

    [SerializeField]
    private float opponentSpaceWeight = 1f;

    [SerializeField]
    private float centralPositionWeight = 1f;

    /// <summary>
    /// Selects and stores the best teammate to receive a cross.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a valid cross target is selected.
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

        IReadOnlyList<IAIActor> teammates =
            context.GameState.GetTeamActors(
                context.Actor.TeamId);

        if (teammates == null)
            return EBehaviorTreeResult.Failure;

        IAIActor bestTarget = null;
        float bestScore = float.MinValue;

        foreach (IAIActor teammate in teammates)
        {
            if (!IsValidTarget(
                    context,
                    teammate))
            {
                continue;
            }

            Vector2 fieldPosition =
                context.GameState
                    .GetTeamRelativeFieldPosition(
                        context.Actor.TeamId,
                        teammate.Position);

            float score =
                ScoreTarget(
                    context,
                    teammate,
                    fieldPosition);

            if (bestTarget != null
                && score <= bestScore)
            {
                continue;
            }

            bestTarget = teammate;
            bestScore = score;
        }

        if (bestTarget == null)
            return EBehaviorTreeResult.Failure;

        context.SetSelectedActor(bestTarget);

        context.SetSelectedPosition(bestTarget.Position);


        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Checks whether a teammate can receive a cross.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="candidate">
    /// The teammate being evaluated.
    /// </param>
    /// <returns>
    /// True when the candidate is in the central attacking region.
    /// </returns>
    private bool IsValidTarget(
        AIBehaviorContext context,
        IAIActor candidate)
    {
        if (candidate == null
            || !candidate.IsActive
            || candidate.ActorId
                == context.Actor.ActorId
            || candidate.TeamId
                != context.Actor.TeamId)
        {
            return false;
        }

        Vector2 fieldPosition =
            context.GameState
                .GetTeamRelativeFieldPosition(
                    context.Actor.TeamId,
                    candidate.Position);

        return fieldPosition.x
                >= minimumTargetDepth
            && Mathf.Abs(
                fieldPosition.y)
                <= centerWidth;
    }

    /// <summary>
    /// Scores a valid cross target.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="candidate">
    /// The candidate being scored.
    /// </param>
    /// <param name="fieldPosition">
    /// The candidate's team-relative position.
    /// </param>
    /// <returns>The candidate score.</returns>
    private float ScoreTarget(
        AIBehaviorContext context,
        IAIActor candidate,
        Vector2 fieldPosition)
    {
        float forwardProgress =
            fieldPosition.x;

        float centralScore =
            1f - Mathf.Clamp01(
                Mathf.Abs(
                    fieldPosition.y)
                / Mathf.Max(
                    centerWidth,
                    Mathf.Epsilon));

        float opponentDistance =
            GetNearestOpponentDistance(
                context,
                candidate.Position);

        return forwardProgress
                * forwardProgressWeight
            + centralScore
                * centralPositionWeight
            + opponentDistance
                * opponentSpaceWeight;
    }

    /// <summary>
    /// Gets the distance from a position to the nearest opponent.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="position">
    /// The world-space position being evaluated.
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
        minimumTargetDepth =
            Mathf.Clamp01(
                minimumTargetDepth);

        centerWidth =
            Mathf.Clamp01(
                centerWidth);

        forwardProgressWeight =
            Mathf.Max(
                0f,
                forwardProgressWeight);

        opponentSpaceWeight =
            Mathf.Max(
                0f,
                opponentSpaceWeight);

        centralPositionWeight =
            Mathf.Max(
                0f,
                centralPositionWeight);
    }
#endif
}