using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects a safe clearance position away from nearby opponents and toward
/// the opponent's half.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Clearance Target",
    menuName = "Soccer AI/Nodes/Selection/Select Clearance Target")]
public sealed class SelectClearanceTargetNode :
    AIBehaviorTreeNode
{
    [Header("Clearance Target")]
    [Tooltip(
        "The team-relative depth toward which the ball should be cleared.")]
    [SerializeField, Range(0f, 1f)]
    private float clearanceDepth = 0.8f;

    [Tooltip(
        "The lateral distance used when clearing toward either side.")]
    [SerializeField, Range(0f, 1f)]
    private float lateralClearanceOffset = 0.75f;

    [Header("Scoring")]
    [SerializeField]
    private float opponentDistanceWeight = 1f;

    [SerializeField]
    private float teammateDistanceWeight = 0.25f;

    /// <summary>
    /// Selects and stores the safest clearance target.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a clearance target is selected.
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

        Vector2[] candidateFieldPositions =
        {
            new Vector2(
                clearanceDepth,
                -lateralClearanceOffset),

            new Vector2(
                clearanceDepth,
                0f),

            new Vector2(
                clearanceDepth,
                lateralClearanceOffset)
        };

        bool foundTarget = false;
        float bestScore = float.MinValue;
        Vector2 bestTarget = default;

        foreach (Vector2 candidateFieldPosition
                 in candidateFieldPositions)
        {
            Vector2 candidateWorldPosition =
                context.GameState
                    .GetWorldPositionFromTeamRelative(
                        context.Actor.TeamId,
                        candidateFieldPosition);

            float score =
                ScoreCandidate(
                    context,
                    candidateWorldPosition);

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

        context.SelectedActor = null;

        context.SelectedPosition =
            bestTarget;

        context.HasSelectedPosition =
            true;

        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Scores a possible clearance destination.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="candidatePosition">
    /// The proposed world-space clearance position.
    /// </param>
    /// <returns>The clearance score.</returns>
    private float ScoreCandidate(
        AIBehaviorContext context,
        Vector2 candidatePosition)
    {
        float opponentDistance =
            GetNearestActorDistance(
                context,
                GetOpposingTeam(
                    context.Actor.TeamId),
                candidatePosition,
                null);

        float teammateDistance =
            GetNearestActorDistance(
                context,
                context.Actor.TeamId,
                candidatePosition,
                context.Actor.ActorId);

        return opponentDistance
                * opponentDistanceWeight
            + teammateDistance
                * teammateDistanceWeight;
    }

    /// <summary>
    /// Gets the distance from a position to the nearest valid actor on a
    /// team.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="team">
    /// The team whose actors are checked.
    /// </param>
    /// <param name="position">
    /// The world-space position being evaluated.
    /// </param>
    /// <param name="ignoredActorId">
    /// An actor identifier to ignore.
    /// </param>
    /// <returns>The nearest valid actor distance.</returns>
    private float GetNearestActorDistance(
        AIBehaviorContext context,
        ETeamId team,
        Vector2 position,
        string ignoredActorId)
    {
        IReadOnlyList<IAIActor> actors =
            context.GameState.GetTeamActors(
                team);

        if (actors == null
            || actors.Count == 0)
        {
            return 0f;
        }

        float nearestDistance =
            float.MaxValue;

        bool foundActor = false;

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || !actor.IsActive
                || actor.ActorId
                    == ignoredActorId)
            {
                continue;
            }

            foundActor = true;

            nearestDistance =
                Mathf.Min(
                    nearestDistance,
                    Vector2.Distance(
                        position,
                        actor.Position));
        }

        return foundActor
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
        clearanceDepth =
            Mathf.Clamp01(
                clearanceDepth);

        lateralClearanceOffset =
            Mathf.Clamp01(
                lateralClearanceOffset);

        opponentDistanceWeight =
            Mathf.Max(
                0f,
                opponentDistanceWeight);

        teammateDistanceWeight =
            Mathf.Max(
                0f,
                teammateDistanceWeight);
    }
#endif
}