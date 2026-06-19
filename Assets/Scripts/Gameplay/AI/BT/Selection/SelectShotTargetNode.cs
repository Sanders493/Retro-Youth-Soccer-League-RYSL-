using UnityEngine;

/// <summary>
/// Selects a target position inside the opposing goal.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Shot Target",
    menuName = "Soccer AI/Nodes/Selection/Select Shot Target")]
public sealed class SelectShotTargetNode :
    AIBehaviorTreeNode
{
    [Tooltip(
        "The maximum lateral offset from the goal center.")]
    [SerializeField]
    private float targetOffset = 0.75f;

    /// <summary>
    /// Selects and stores a shot target inside the opposing goal.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a shot target is selected.
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

        Vector2 goalPosition =
            context.GameState.GetAttackingGoalPosition(
                context.Actor.TeamId);

        Vector2 actorToGoal =
            goalPosition
            - context.Actor.Position;

        if (actorToGoal.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 lateralDirection =
            new Vector2(
                -actorToGoal.y,
                actorToGoal.x)
            .normalized;

        float signedOffset =
            Random.Range(
                -targetOffset,
                targetOffset);

        context.SetSelectedPosition(
            goalPosition
            + lateralDirection * signedOffset);


        return EBehaviorTreeResult.Success;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the target offset to a valid value.
    /// </summary>
    private void OnValidate()
    {
        targetOffset =
            Mathf.Max(
                0f,
                targetOffset);
    }
#endif
}