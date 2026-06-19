using UnityEngine;

/// <summary>
/// Selects a position from which the intended receiver should meet an active
/// pass.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Pass Reception Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Pass Reception Position")]
public sealed class SelectPassReceptionPositionNode :
    AIBehaviorTreeNode
{
    [Header("Reception")]
    [Tooltip(
        "How far the reception target may lead the receiver in the current " +
        "ball-travel direction.")]
    [SerializeField]
    private float maximumLeadDistance = 1.5f;

    [Tooltip(
        "How strongly ball speed contributes to the lead distance.")]
    [SerializeField]
    private float velocityLeadMultiplier = 0.15f;

    [Tooltip(
        "The maximum distance the selected reception position may be from " +
        "the intended pass destination.")]
    [SerializeField]
    private float maximumTargetAdjustment = 2f;

    /// <summary>
    /// Selects and stores a world-space pass reception position.
    /// </summary>
    /// <param name="context">
    /// The current actor's behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when this actor is the intended receiver of an active pass
    /// and a reception position is selected.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || !context.GameState.HasActivePass)
        {
            return EBehaviorTreeResult.Failure;
        }

        string intendedReceiverId =
            context.GameState.IntendedPassReceiverId;

        if (string.IsNullOrWhiteSpace(
                intendedReceiverId)
            || intendedReceiverId
                != context.Actor.ActorId)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 intendedTarget =
            context.GameState.IntendedPassTargetPosition;

        Vector2 ballVelocity =
            context.GameState.BallVelocity;

        Vector2 receptionPosition =
            intendedTarget;

        if (ballVelocity.sqrMagnitude
            > Mathf.Epsilon)
        {
            float leadDistance =
                Mathf.Min(
                    ballVelocity.magnitude
                        * velocityLeadMultiplier,
                    maximumLeadDistance);

            receptionPosition +=
                ballVelocity.normalized
                * leadDistance;
        }

        receptionPosition =
            LimitTargetAdjustment(
                intendedTarget,
                receptionPosition);

        receptionPosition =
            ClampToField(
                context,
                receptionPosition);

        context.SetSelectedActor (context.Actor);

        context.SetSelectedPosition(receptionPosition);


        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Restricts the reception position to a maximum distance from the
    /// original pass destination.
    /// </summary>
    private Vector2 LimitTargetAdjustment(
        Vector2 intendedTarget,
        Vector2 adjustedTarget)
    {
        Vector2 adjustment =
            adjustedTarget - intendedTarget;

        if (adjustment.sqrMagnitude
            <= maximumTargetAdjustment
            * maximumTargetAdjustment)
        {
            return adjustedTarget;
        }

        return intendedTarget
            + adjustment.normalized
            * maximumTargetAdjustment;
    }

    /// <summary>
    /// Restricts a world-space position to the playable field.
    /// </summary>
    private Vector2 ClampToField(
        AIBehaviorContext context,
        Vector2 worldPosition)
    {
        Vector2 fieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                worldPosition);

        fieldPosition.x =
            Mathf.Clamp01(
                fieldPosition.x);

        fieldPosition.y =
            Mathf.Clamp(
                fieldPosition.y,
                -1f,
                1f);

        return context.GameState
            .GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                fieldPosition);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts reception settings to valid values.
    /// </summary>
    private void OnValidate()
    {
        maximumLeadDistance =
            Mathf.Max(
                0f,
                maximumLeadDistance);

        velocityLeadMultiplier =
            Mathf.Max(
                0f,
                velocityLeadMultiplier);

        maximumTargetAdjustment =
            Mathf.Max(
                0f,
                maximumTargetAdjustment);
    }
#endif
}