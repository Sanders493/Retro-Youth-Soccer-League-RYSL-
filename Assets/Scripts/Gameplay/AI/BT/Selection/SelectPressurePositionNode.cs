using UnityEngine;

/// <summary>
/// Selects a defensive pressure position between the opposing ball owner and
/// the actor's defending goal.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Pressure Position",
    menuName = "Soccer AI/Nodes/Selection/Select Pressure Position")]
public sealed class SelectPressurePositionNode :
    AIBehaviorTreeNode
{
    [Tooltip(
        "The distance the defender should remain goal-side of the ball owner.")]
    [SerializeField]
    private float pressureOffset = 0.75f;

    /// <summary>
    /// Selects a position slightly between the opposing ball owner and the
    /// defending goal.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a valid pressure position is selected.
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

        Vector2 defendingGoal =
            context.GameState.GetDefendingGoalPosition(
                context.Actor.TeamId);

        Vector2 towardGoal =
            defendingGoal
            - ballOwner.Position;

        if (towardGoal.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 targetPosition =
            ballOwner.Position
            + towardGoal.normalized
            * pressureOffset;

        Vector2 teamRelativePosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                targetPosition);

        teamRelativePosition.x =
            Mathf.Clamp01(
                teamRelativePosition.x);

        teamRelativePosition.y =
            Mathf.Clamp(
                teamRelativePosition.y,
                -1f,
                1f);

        context.SelectedPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                teamRelativePosition);

        context.HasSelectedPosition =
            true;

        return EBehaviorTreeResult.Success;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the pressure offset to a valid value.
    /// </summary>
    private void OnValidate()
    {
        pressureOffset =
            Mathf.Max(
                0f,
                pressureOffset);
    }
#endif
}