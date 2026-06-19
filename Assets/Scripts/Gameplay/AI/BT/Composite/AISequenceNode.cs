using UnityEngine;

/// <summary>
/// Evaluates children in order until one fails or remains running.
/// </summary>
[CreateAssetMenu(
    fileName = "Sequence",
    menuName = "Soccer AI/Nodes/Composite/Sequence")]
public sealed class AISequenceNode : AICompositeNode
{
    /// <summary>
    /// Evaluates the sequence's children.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>
    /// Failure or Running from the first incomplete child, or Success when
    /// every child succeeds.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        foreach (AIBehaviorTreeNode child in Children)
        {
            if (child == null)
                return EBehaviorTreeResult.Failure;

            EBehaviorTreeResult result =
                child.Evaluate(context);

            if (result != EBehaviorTreeResult.Success)
                return result;
        }

        return EBehaviorTreeResult.Success;
    }
}