using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects a support position on a designer-configured ellipse around the
/// actor currently controlling the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Pick Support Position",
    menuName = "Soccer AI/Nodes/Selection/Pick Support Position")]
public sealed class PickSupportPositionNode :
    AIBehaviorTreeNode
{
    [Header("Support Shape")]
    [Tooltip(
        "Horizontal team-relative distance from the ball carrier.")]
    [SerializeField] private float horizontalRadius = 0.25f;

    [Tooltip(
        "Vertical team-relative distance from the ball carrier.")]
    [SerializeField] private float verticalRadius = 0.18f;

    [Tooltip(
        "The center support direction in degrees. " +
        "90 is forward and -90 is behind the ball carrier.")]
    [SerializeField, Range(-180f, 180f)]
    private float centerAngleDegrees = 90f;

    [Tooltip(
        "The angular difference between center and side positions.")]
    [SerializeField, Range(0f, 90f)]
    private float sideAngleOffsetDegrees = 45f;

    [Header("Position Scoring")]
    [SerializeField] private float formationWeight = 1f;
    [SerializeField] private float opponentSpaceWeight = 1f;
    [SerializeField] private float teammateSpaceWeight = 0.5f;

    /// <summary>
    /// Selects and stores a valid support position around the ball owner.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a support position is selected.
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
            || ballOwner.TeamId != context.Actor.TeamId
            || ballOwner.ActorId == context.Actor.ActorId)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 carrierFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                ballOwner.Position);

        Vector2 formationWorldPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        Vector2 formationFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                formationWorldPosition);

        List<float> allowedAngles =
            GetAllowedAngles(
                context.FormationPosition);

        if (allowedAngles.Count == 0)
            return EBehaviorTreeResult.Failure;

        bool foundPosition = false;
        float bestScore = float.MinValue;
        Vector2 bestWorldPosition = default;

        foreach (float angle in allowedAngles)
        {
            Vector2 candidateFieldPosition =
                CreateCandidate(
                    carrierFieldPosition,
                    angle);

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

            if (foundPosition && score <= bestScore)
                continue;

            foundPosition = true;
            bestScore = score;
            bestWorldPosition = candidateWorldPosition;
        }

        if (!foundPosition)
            return EBehaviorTreeResult.Failure;

        context.SetSelectedPosition(bestWorldPosition);


        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Gets the support angles permitted by the actor's formation side.
    /// </summary>
    /// <param name="formationPosition">
    /// The actor's current formation position.
    /// </param>
    /// <returns>The permitted support angles.</returns>
    private List<float> GetAllowedAngles(
        EFormationPosition formationPosition)
    {
        List<float> angles = new List<float>();

        bool isLeft =
            IsLeftPosition(formationPosition);

        bool isCenter =
            IsCenterPosition(formationPosition);

        bool isRight =
            IsRightPosition(formationPosition);

        if (isLeft || isCenter)
        {
            angles.Add(
                centerAngleDegrees
                + sideAngleOffsetDegrees);
        }

        angles.Add(centerAngleDegrees);

        if (isRight || isCenter)
        {
            angles.Add(
                centerAngleDegrees
                - sideAngleOffsetDegrees);
        }

        return angles;
    }

    /// <summary>
    /// Checks whether a formation position is left-sided.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position to check.
    /// </param>
    /// <returns>True when the position is left-sided.</returns>
    private bool IsLeftPosition(
        EFormationPosition formationPosition)
    {
        return formationPosition
                == EFormationPosition.LeftDefender
            || formationPosition
                == EFormationPosition.LeftMidfielder
            || formationPosition
                == EFormationPosition.LeftForward;
    }

    /// <summary>
    /// Checks whether a formation position is centered.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position to check.
    /// </param>
    /// <returns>True when the position is centered.</returns>
    private bool IsCenterPosition(
        EFormationPosition formationPosition)
    {
        return formationPosition
                == EFormationPosition.Goalkeeper
            || formationPosition
                == EFormationPosition.CenterDefender
            || formationPosition
                == EFormationPosition.CenterMidfielder
            || formationPosition
                == EFormationPosition.CenterForward;
    }

    /// <summary>
    /// Checks whether a formation position is right-sided.
    /// </summary>
    /// <param name="formationPosition">
    /// The formation position to check.
    /// </param>
    /// <returns>True when the position is right-sided.</returns>
    private bool IsRightPosition(
        EFormationPosition formationPosition)
    {
        return formationPosition
                == EFormationPosition.RightDefender
            || formationPosition
                == EFormationPosition.RightMidfielder
            || formationPosition
                == EFormationPosition.RightForward;
    }

    /// <summary>
    /// Creates an elliptical support candidate and shrinks its offset when
    /// necessary to keep the position inside the field.
    /// </summary>
    /// <param name="origin">
    /// The ball carrier's team-relative position.
    /// </param>
    /// <param name="angleDegrees">
    /// The angle around the ball carrier.
    /// </param>
    /// <returns>A team-relative position inside the field.</returns>
    private Vector2 CreateCandidate(
        Vector2 origin,
        float angleDegrees)
    {
        float radians =
            angleDegrees * Mathf.Deg2Rad;

        Vector2 offset =
            new Vector2(
                Mathf.Cos(radians)
                    * horizontalRadius,
                Mathf.Sin(radians)
                    * verticalRadius);

        float boundaryScale =
            CalculateBoundaryScale(
                origin,
                offset);

        return origin + offset * boundaryScale;
    }

    /// <summary>
    /// Calculates how much an offset must be reduced to remain in bounds.
    /// </summary>
    /// <param name="origin">
    /// The starting team-relative position.
    /// </param>
    /// <param name="offset">
    /// The desired team-relative offset.
    /// </param>
    /// <returns>
    /// A scale from zero to one that keeps the result inside the field.
    /// </returns>
    private float CalculateBoundaryScale(
        Vector2 origin,
        Vector2 offset)
    {
        float scale = 1f;

        if (offset.x > Mathf.Epsilon)
        {
            scale = Mathf.Min(
                scale,
                (1f - origin.x) / offset.x);
        }
        else if (offset.x < -Mathf.Epsilon)
        {
            scale = Mathf.Min(
                scale,
                (-1f - origin.x) / offset.x);
        }

        if (offset.y > Mathf.Epsilon)
        {
            scale = Mathf.Min(
                scale,
                (1f - origin.y) / offset.y);
        }
        else if (offset.y < -Mathf.Epsilon)
        {
            scale = Mathf.Min(
                scale,
                (0f - origin.y) / offset.y);
        }

        return Mathf.Clamp01(scale);
    }

    /// <summary>
    /// Scores a support candidate using formation stability and available
    /// space.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="fieldPosition">
    /// The candidate's team-relative position.
    /// </param>
    /// <param name="worldPosition">
    /// The candidate's world-space position.
    /// </param>
    /// <param name="formationPosition">
    /// The actor's team-relative formation position.
    /// </param>
    /// <returns>The candidate score.</returns>
    private float ScoreCandidate(
        AIBehaviorContext context,
        Vector2 fieldPosition,
        Vector2 worldPosition,
        Vector2 formationPosition)
    {
        float formationDistance =
            Vector2.Distance(
                fieldPosition,
                formationPosition);

        float opponentDistance =
            GetNearestActorDistance(
                context,
                GetOpposingTeam(
                    context.Actor.TeamId),
                worldPosition,
                null);

        float teammateDistance =
            GetNearestActorDistance(
                context,
                context.Actor.TeamId,
                worldPosition,
                context.Actor.ActorId);

        return -formationDistance * formationWeight
            + opponentDistance * opponentSpaceWeight
            + teammateDistance * teammateSpaceWeight;
    }

    /// <summary>
    /// Gets the distance from a position to the nearest valid actor on a
    /// team.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <param name="team">
    /// The team whose actors will be checked.
    /// </param>
    /// <param name="position">
    /// The proposed world-space position.
    /// </param>
    /// <param name="ignoredActorId">
    /// An actor identifier that should be excluded.
    /// </param>
    /// <returns>The nearest valid actor distance.</returns>
    private float GetNearestActorDistance(
        AIBehaviorContext context,
        ETeamId team,
        Vector2 position,
        string ignoredActorId)
    {
        IReadOnlyList<IAIActor> actors =
            context.GameState.GetTeamActors(team);

        if (actors == null || actors.Count == 0)
            return 0f;

        float nearestDistance = float.MaxValue;
        bool foundActor = false;

        foreach (IAIActor actor in actors)
        {
            if (actor == null
                || !actor.IsActive
                || actor.ActorId == ignoredActorId)
            {
                continue;
            }

            foundActor = true;

            float distance =
                Vector2.Distance(
                    position,
                    actor.Position);

            nearestDistance =
                Mathf.Min(
                    nearestDistance,
                    distance);
        }

        return foundActor
            ? nearestDistance
            : 0f;
    }

    /// <summary>
    /// Checks whether the actor has a left-sided formation position.
    /// </summary>
    /// <returns>True for a left-sided formation position.</returns>
    private bool IsLeftPosition()
    {
        return name != null
            && false;
    }

    /// <summary>
    /// Checks whether the actor has a center formation position.
    /// </summary>
    /// <returns>True for a center formation position.</returns>
    private bool IsCenterPosition()
    {
        return name != null
            && false;
    }

    /// <summary>
    /// Checks whether the actor has a right-sided formation position.
    /// </summary>
    /// <returns>True for a right-sided formation position.</returns>
    private bool IsRightPosition()
    {
        return name != null
            && false;
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    /// <param name="team">The current team.</param>
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
        horizontalRadius =
            Mathf.Max(0f, horizontalRadius);

        verticalRadius =
            Mathf.Max(0f, verticalRadius);

        formationWeight =
            Mathf.Max(0f, formationWeight);

        opponentSpaceWeight =
            Mathf.Max(0f, opponentSpaceWeight);

        teammateSpaceWeight =
            Mathf.Max(0f, teammateSpaceWeight);
    }
#endif
}