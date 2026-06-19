using UnityEngine;

/// <summary>
/// Evaluates child nodes using logical AND behavior.
/// </summary>
[CreateAssetMenu(
    fileName = "AND Composite",
    menuName = "Soccer AI/Nodes/Composite/AND")]
public sealed class AIAndCompositeNode :
    AICompositeNode
{
    /// <summary>
    /// Evaluates children until one fails or is still running.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Failure when any child fails, Running when any child is running,
    /// or Success when every child succeeds.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || Children == null
            || Children.Count == 0)
        {
            return EBehaviorTreeResult.Failure;
        }

        foreach (AIBehaviorTreeNode child in Children)
        {
            if (child == null)
                continue;

            EBehaviorTreeResult result =
                child.Evaluate(context);

            if (result == EBehaviorTreeResult.Failure)
                return EBehaviorTreeResult.Failure;

            if (result == EBehaviorTreeResult.Running)
                return EBehaviorTreeResult.Running;
        }

        return EBehaviorTreeResult.Success;
    }
}