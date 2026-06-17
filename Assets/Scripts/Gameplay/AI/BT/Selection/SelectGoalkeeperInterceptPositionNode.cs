using UnityEngine;

/// <summary>
/// Predicts where a moving ball will cross the goalkeeper's guarding depth
/// and selects that interception position.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Goalkeeper Interception Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Goalkeeper Interception Position")]
public sealed class SelectGoalkeeperInterceptionPositionNode :
    AIBehaviorTreeNode
{
    [Header("Interception")]
    [Tooltip(
        "The team-relative depth at which the goalkeeper attempts to " +
        "intercept an incoming shot.")]
    [SerializeField, Range(0f, 1f)]
    private float interceptionDepth = 0.06f;

    [Tooltip(
        "The maximum lateral distance the goalkeeper may defend.")]
    [SerializeField, Range(0f, 1f)]
    private float maximumLateralPosition = 0.4f;

    [Tooltip(
        "The minimum ball speed considered an incoming shot.")]
    [SerializeField]
    private float minimumIncomingSpeed = 1.5f;

    [Tooltip(
        "The maximum prediction time accepted for an interception.")]
    [SerializeField]
    private float maximumPredictionTime = 2f;

    /// <summary>
    /// Predicts the ball's intersection with the goalkeeper guarding line.
    /// </summary>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.Actor.IsGoalkeeper)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 ballVelocity =
            context.GameState.BallVelocity;

        if (ballVelocity.magnitude
            < minimumIncomingSpeed)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        Vector2 futureWorldPosition =
            context.GameState.BallPosition
            + ballVelocity;

        Vector2 futureFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                futureWorldPosition);

        Vector2 fieldVelocity =
            futureFieldPosition
            - ballFieldPosition;

        // Negative team-relative X means the ball is traveling toward the
        // goalkeeper's defending goal.
        if (fieldVelocity.x
            >= -Mathf.Epsilon)
        {
            return EBehaviorTreeResult.Failure;
        }

        float timeToInterception =
            (interceptionDepth
             - ballFieldPosition.x)
            / fieldVelocity.x;

        if (timeToInterception < 0f
            || timeToInterception
                > maximumPredictionTime)
        {
            return EBehaviorTreeResult.Failure;
        }

        float predictedLateralPosition =
            ballFieldPosition.y
            + fieldVelocity.y
            * timeToInterception;

        predictedLateralPosition =
            Mathf.Clamp(
                predictedLateralPosition,
                -maximumLateralPosition,
                maximumLateralPosition);

        Vector2 interceptionFieldPosition =
            new Vector2(
                interceptionDepth,
                predictedLateralPosition);

        Vector2 interceptionWorldPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                interceptionFieldPosition);

        context.ClearSelectedActor();

        context.SetSelectedPosition(
            interceptionWorldPosition);

        return EBehaviorTreeResult.Success;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        interceptionDepth =
            Mathf.Clamp01(
                interceptionDepth);

        maximumLateralPosition =
            Mathf.Clamp01(
                maximumLateralPosition);

        minimumIncomingSpeed =
            Mathf.Max(
                0f,
                minimumIncomingSpeed);

        maximumPredictionTime =
            Mathf.Max(
                0.01f,
                maximumPredictionTime);
    }
#endif
}