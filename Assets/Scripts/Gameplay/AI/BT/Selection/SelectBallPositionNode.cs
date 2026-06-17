using UnityEngine;

/// <summary>
/// Selects the ball's current world-space position.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Ball Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Ball Position")]
public sealed class SelectBallPositionNode :
    AIBehaviorTreeNode
{
    /// <summary>
    /// Stores the ball's current world-space position in the behavior
    /// context.
    /// </summary>
    /// <param name="context">
    /// The actor-specific behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when the ball position is available.
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

        context.ClearSelectedActor();

        context.SetSelectedPosition(
            context.GameState.BallPosition);

        return EBehaviorTreeResult.Success;
    }
}