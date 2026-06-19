using UnityEngine;

/// <summary>
/// Evaluates children in order until one succeeds or remains running.
/// </summary>
[CreateAssetMenu(
    fileName = "Selector",
    menuName = "Soccer AI/Nodes/Composite/Selector")]
public sealed class AISelectorNode : AICompositeNode
{
    [Tooltip(
        "Enable this only on the selector used as the behavior-tree root.")]
    [SerializeField]
    private bool isRootSelector;
    
    /// <summary>
    /// Evaluates children in order until one succeeds or remains running.
    /// </summary>
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
                child.Evaluate(
                    context);

            if (result == EBehaviorTreeResult.Failure)
                continue;

            if (isRootSelector)
            {
                context.SetRootSelection(
                    child.DisplayName);
            }
            else if (string.IsNullOrWhiteSpace(
                         context.NestedSelectorSelection))
            {
                context.SetNestedSelectorSelection(
                    child.DisplayName);
            }

            return result;
        }

        return EBehaviorTreeResult.Failure;
    }
}