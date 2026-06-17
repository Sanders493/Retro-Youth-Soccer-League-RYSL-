using UnityEngine;

/// <summary>
/// Selects a defensive support position behind and beside the primary
/// defender.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Defend Support Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Defend Support Position")]
public sealed class SelectDefendSupportPositionNode :
    AIBehaviorTreeNode
{
    [Header("Support Shape")]
    [Tooltip(
        "How far behind the ball owner the support defender should remain.")]
    [SerializeField]
    private float goalSideOffset = 2f;

    [Tooltip(
        "The lateral distance separating the supporting defender from the " +
        "direct pressure line.")]
    [SerializeField]
    private float lateralOffset = 1.5f;

    [Tooltip(
        "How strongly the actor should remain near its assigned formation " +
        "side.")]
    [SerializeField, Range(0f, 1f)]
    private float formationSideInfluence = 0.5f;

    /// <summary>
    /// Selects a supporting defensive position while another actor directly
    /// pressures the ball owner.
    /// </summary>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || context.IsPrimaryDefender)
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

        towardGoal.Normalize();

        Vector2 lateralDirection =
            new Vector2(
                -towardGoal.y,
                towardGoal.x);

        Vector2 formationPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        float actorSide =
            Vector2.Dot(
                context.Actor.Position
                    - ballOwner.Position,
                lateralDirection);

        float lateralSign =
            actorSide < 0f
                ? -1f
                : 1f;

        Vector2 defensiveSupportPosition =
            ballOwner.Position
            + towardGoal * goalSideOffset
            + lateralDirection
                * lateralOffset
                * lateralSign;

        Vector2 blendedPosition =
            Vector2.Lerp(
                defensiveSupportPosition,
                formationPosition,
                formationSideInfluence);

        Vector2 fieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                blendedPosition);

        fieldPosition.x =
            Mathf.Clamp01(
                fieldPosition.x);

        fieldPosition.y =
            Mathf.Clamp(
                fieldPosition.y,
                -1f,
                1f);

        Vector2 selectedPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                fieldPosition);

        context.ClearSelectedActor();

        context.SetSelectedPosition(
            selectedPosition);

        return EBehaviorTreeResult.Success;
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

        lateralOffset =
            Mathf.Max(
                0f,
                lateralOffset);

        formationSideInfluence =
            Mathf.Clamp01(
                formationSideInfluence);
    }
#endif
}