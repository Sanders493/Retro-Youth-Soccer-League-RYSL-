using UnityEngine;

/// <summary>
/// Evaluates child nodes using logical OR behavior.
/// </summary>
[CreateAssetMenu(
    fileName = "OR Composite",
    menuName = "Soccer AI/Nodes/Composite/OR")]
public sealed class AIOrCompositeNode :
    AICompositeNode
{
    /// <summary>
    /// Evaluates children until one succeeds or is still running.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when any child succeeds, Running when any child is running,
    /// or Failure when every child fails.
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

            if (result == EBehaviorTreeResult.Success)
                return EBehaviorTreeResult.Success;

            if (result == EBehaviorTreeResult.Running)
                return EBehaviorTreeResult.Running;
        }

        return EBehaviorTreeResult.Failure;
    }
}