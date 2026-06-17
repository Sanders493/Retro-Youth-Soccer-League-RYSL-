using UnityEngine;

/// <summary>
/// Checks whether the ball is inside a designer-configured team-relative
/// field region.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball In Field Region",
    menuName = "Soccer AI/Nodes/Conditions/Ball In Field Region")]
public sealed class BallInFieldRegionCondition :
    AIConditionNode
{
    [SerializeField] private EFieldDepthRegion depthRegion;
    [SerializeField] private EFieldSideRegion sideRegion;

    [Tooltip(
        "The normalized depth separating the team's half from the " +
        "opponent's half.")]
    [SerializeField, Range(0f, 1f)]
    private float halfBoundary = 0.5f;

    [Tooltip(
        "The normalized lateral boundary separating the center from each " +
        "side.")]
    [SerializeField, Range(0f, 1f)]
    private float sideBoundary = 1f / 3f;

    /// <summary>
    /// Checks whether the ball is inside the configured field region.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the ball is inside both the configured depth and side
    /// regions.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        return IsInsideDepthRegion(
                context,
                ballFieldPosition.x)
            && IsInsideSideRegion(
                ballFieldPosition.y);
    }

    /// <summary>
    /// Checks the configured team-relative field depth.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <param name="depthPosition">
    /// The ball's normalized team-relative depth position.
    /// </param>
    /// <returns>
    /// True when the ball is inside the configured depth region.
    /// </returns>
    private bool IsInsideDepthRegion(
        AIBehaviorContext context,
        float depthPosition)
    {
        switch (depthRegion)
        {
            case EFieldDepthRegion.Any:
                return true;

            case EFieldDepthRegion.TeamHalf:
                return depthPosition < halfBoundary;

            case EFieldDepthRegion.OpponentHalf:
                return depthPosition >= halfBoundary;

            case EFieldDepthRegion.TeamGoalBox:
                return context.GameState.IsInsideGoalArea(
                    context.Actor.TeamId,
                    context.GameState.BallPosition);

            case EFieldDepthRegion.OpponentGoalBox:
                return context.GameState.IsInsideGoalArea(
                    GetOpposingTeam(
                        context.Actor.TeamId),
                    context.GameState.BallPosition);

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks the configured team-relative lateral region.
    /// </summary>
    /// <param name="lateralPosition">
    /// The ball's normalized team-relative left-to-right position.
    /// </param>
    /// <returns>
    /// True when the ball is inside the configured side region.
    /// </returns>
    private bool IsInsideSideRegion(
        float lateralPosition)
    {
        bool isLeft =
            lateralPosition < -sideBoundary;

        bool isCenter =
            lateralPosition >= -sideBoundary
            && lateralPosition <= sideBoundary;

        bool isRight =
            lateralPosition > sideBoundary;

        switch (sideRegion)
        {
            case EFieldSideRegion.Any:
                return true;

            case EFieldSideRegion.Left:
                return isLeft;

            case EFieldSideRegion.Center:
                return isCenter;

            case EFieldSideRegion.Right:
                return isRight;

            case EFieldSideRegion.LeftOrCenter:
                return isLeft || isCenter;

            case EFieldSideRegion.RightOrCenter:
                return isRight || isCenter;

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    /// <param name="currentTeam">
    /// The actor's current team.
    /// </param>
    /// <returns>The opposing team.</returns>
    private ETeamId GetOpposingTeam(
        ETeamId currentTeam)
    {
        return currentTeam == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured boundaries to normalized values.
    /// </summary>
    private void OnValidate()
    {
        halfBoundary =
            Mathf.Clamp01(
                halfBoundary);

        sideBoundary =
            Mathf.Clamp01(
                sideBoundary);
    }
#endif
}