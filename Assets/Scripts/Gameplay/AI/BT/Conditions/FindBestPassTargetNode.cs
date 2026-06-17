using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects the best currently available teammate to receive a pass.
/// </summary>
[CreateAssetMenu(
    fileName = "Find Best Pass Target",
    menuName = "Soccer AI/Nodes/Selection/Find Best Pass Target")]
public sealed class FindBestPassTargetNode :
    AIBehaviorTreeNode
{
    [Header("Pass Range")]
    [SerializeField]
    private float minimumPassDistance = 1.5f;

    [SerializeField]
    private float maximumPassDistance = 10f;

    [Header("Passing Lane")]
    [Tooltip(
        "The maximum distance from the pass line at which an opponent " +
        "blocks the passing lane.")]
    [SerializeField]
    private float laneBlockRadius = 0.75f;

    [Header("Scoring")]
    [SerializeField]
    private float forwardProgressWeight = 2f;

    [SerializeField]
    private float opponentSpaceWeight = 1f;

    [SerializeField]
    private float distanceWeight = 0.25f;

    [SerializeField]
    private float goalProgressWeight = 1f;

    /// <summary>
    /// Finds and stores the best valid teammate to receive a pass.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a valid pass target is found.
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

        Vector2 passerPosition =
            context.Actor.Position;

        Vector2 passerFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                passerPosition);

        float minimumDistanceSquared =
            minimumPassDistance
            * minimumPassDistance;

        float maximumDistanceSquared =
            maximumPassDistance
            * maximumPassDistance;

        IAIActor bestTarget = null;
        float bestScore = float.MinValue;

        foreach (IAIActor teammate in teammates)
        {
            if (!IsCandidateValid(
                    context,
                    teammate,
                    passerPosition,
                    minimumDistanceSquared,
                    maximumDistanceSquared))
            {
                continue;
            }

            Vector2 teammateFieldPosition =
                context.GameState
                    .GetTeamRelativeFieldPosition(
                        context.Actor.TeamId,
                        teammate.Position);

            float score =
                ScoreCandidate(
                    context,
                    teammate,
                    passerPosition,
                    passerFieldPosition,
                    teammateFieldPosition);

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

        context.SelectedActor =
            bestTarget;

        context.SelectedPosition =
            bestTarget.Position;

        context.HasSelectedPosition =
            true;

        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Checks whether a teammate can currently receive a pass.
    /// </summary>
    private bool IsCandidateValid(
        AIBehaviorContext context,
        IAIActor candidate,
        Vector2 passerPosition,
        float minimumDistanceSquared,
        float maximumDistanceSquared)
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

        float distanceSquared =
            (candidate.Position - passerPosition)
            .sqrMagnitude;

        if (distanceSquared
                < minimumDistanceSquared
            || distanceSquared
                > maximumDistanceSquared)
        {
            return false;
        }

        return !IsPassingLaneBlocked(
            context,
            passerPosition,
            candidate.Position);
    }

    /// <summary>
    /// Scores one valid pass target.
    /// </summary>
    private float ScoreCandidate(
        AIBehaviorContext context,
        IAIActor candidate,
        Vector2 passerPosition,
        Vector2 passerFieldPosition,
        Vector2 candidateFieldPosition)
    {
        float forwardProgress =
            candidateFieldPosition.x
            - passerFieldPosition.x;

        float nearestOpponentDistance =
            GetNearestOpponentDistance(
                context,
                candidate.Position);

        float passDistance =
            Vector2.Distance(
                passerPosition,
                candidate.Position);

        float goalProgress =
            candidateFieldPosition.x;

        return forwardProgress
                * forwardProgressWeight
            + nearestOpponentDistance
                * opponentSpaceWeight
            - passDistance
                * distanceWeight
            + goalProgress
                * goalProgressWeight;
    }

    /// <summary>
    /// Checks whether an opponent is close enough to intercept the pass line.
    /// </summary>
    private bool IsPassingLaneBlocked(
        AIBehaviorContext context,
        Vector2 start,
        Vector2 end)
    {
        IReadOnlyList<IAIActor> opponents =
            context.GameState.GetTeamActors(
                GetOpposingTeam(
                    context.Actor.TeamId));

        if (opponents == null)
            return false;

        foreach (IAIActor opponent in opponents)
        {
            if (opponent == null
                || !opponent.IsActive)
            {
                continue;
            }

            if (GetDistanceToSegment(
                    opponent.Position,
                    start,
                    end)
                <= laneBlockRadius)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the nearest opponent distance from a candidate.
    /// </summary>
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
    /// Gets the distance between a point and a line segment.
    /// </summary>
    private float GetDistanceToSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment =
            end - start;

        float segmentLengthSquared =
            segment.sqrMagnitude;

        if (segmentLengthSquared
            <= Mathf.Epsilon)
        {
            return Vector2.Distance(
                point,
                start);
        }

        float interpolation =
            Vector2.Dot(
                point - start,
                segment)
            / segmentLengthSquared;

        interpolation =
            Mathf.Clamp01(
                interpolation);

        Vector2 closestPoint =
            start
            + segment * interpolation;

        return Vector2.Distance(
            point,
            closestPoint);
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
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
        minimumPassDistance =
            Mathf.Max(
                0f,
                minimumPassDistance);

        maximumPassDistance =
            Mathf.Max(
                minimumPassDistance,
                maximumPassDistance);

        laneBlockRadius =
            Mathf.Max(
                0f,
                laneBlockRadius);

        forwardProgressWeight =
            Mathf.Max(
                0f,
                forwardProgressWeight);

        opponentSpaceWeight =
            Mathf.Max(
                0f,
                opponentSpaceWeight);

        distanceWeight =
            Mathf.Max(
                0f,
                distanceWeight);

        goalProgressWeight =
            Mathf.Max(
                0f,
                goalProgressWeight);
    }
#endif
}