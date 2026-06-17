using UnityEngine;

/// <summary>
/// Evaluates children in order until one succeeds or remains running.
/// </summary>
[CreateAssetMenu(
    fileName = "Selector",
    menuName = "Soccer AI/Nodes/Composite/Selector")]
public sealed class AISelectorNode : AICompositeNode
{
    /// <summary>
    /// Evaluates the selector's children.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>
    /// Success or Running from the first matching child, or Failure when no
    /// child matches.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        foreach (AIBehaviorTreeNode child in Children)
        {
            if (child == null)
                continue;

            EBehaviorTreeResult result =
                child.Evaluate(context);

            if (result != EBehaviorTreeResult.Failure)
                return result;
        }

        return EBehaviorTreeResult.Failure;
    }
}