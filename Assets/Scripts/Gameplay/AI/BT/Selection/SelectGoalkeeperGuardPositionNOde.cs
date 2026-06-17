using UnityEngine;

/// <summary>
/// Selects a goalkeeper position between the ball and the defending goal
/// while restricting the goalkeeper to a configurable goal-area shape.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Goalkeeper Guard Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Goalkeeper Guard Position")]
public sealed class SelectGoalkeeperGuardPositionNode :
    AIBehaviorTreeNode
{
    [Header("Depth")]
    [Tooltip(
        "The goalkeeper's normal team-relative depth near the goal line.")]
    [SerializeField, Range(0f, 1f)]
    private float restingDepth = 0.05f;

    [Tooltip(
        "The furthest team-relative depth the goalkeeper may step forward.")]
    [SerializeField, Range(0f, 1f)]
    private float maximumStepOutDepth = 0.18f;

    [Header("Lateral Movement")]
    [Tooltip(
        "The greatest team-relative lateral distance the goalkeeper may " +
        "move from the center of the goal.")]
    [SerializeField, Range(0f, 1f)]
    private float maximumLateralPosition = 0.35f;

    [Tooltip(
        "How strongly the goalkeeper follows the ball's lateral position.")]
    [SerializeField, Range(0f, 1f)]
    private float lateralTrackingInfluence = 0.85f;

    /// <summary>
    /// Selects a position that keeps the goalkeeper between the ball and the
    /// defending goal.
    /// </summary>
    /// <param name="context">
    /// The actor-specific behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a goalkeeper guard position is selected.
    /// </returns>
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

        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        float ballDepth =
            Mathf.Clamp01(
                ballFieldPosition.x);

        float selectedDepth =
            Mathf.Lerp(
                maximumStepOutDepth,
                restingDepth,
                ballDepth);

        selectedDepth =
            Mathf.Clamp(
                selectedDepth,
                restingDepth,
                maximumStepOutDepth);

        float selectedLateralPosition =
            ballFieldPosition.y
            * lateralTrackingInfluence;

        selectedLateralPosition =
            Mathf.Clamp(
                selectedLateralPosition,
                -maximumLateralPosition,
                maximumLateralPosition);

        Vector2 selectedFieldPosition =
            new Vector2(
                selectedDepth,
                selectedLateralPosition);

        Vector2 selectedWorldPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                selectedFieldPosition);

        context.ClearSelectedActor();

        context.SetSelectedPosition(
            selectedWorldPosition);

        return EBehaviorTreeResult.Success;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts goalkeeper positioning values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        restingDepth =
            Mathf.Clamp01(
                restingDepth);

        maximumStepOutDepth =
            Mathf.Clamp(
                maximumStepOutDepth,
                restingDepth,
                1f);

        maximumLateralPosition =
            Mathf.Clamp01(
                maximumLateralPosition);

        lateralTrackingInfluence =
            Mathf.Clamp01(
                lateralTrackingInfluence);
    }
#endif
}