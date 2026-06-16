using UnityEngine;

/// <summary>
/// Defines one designer-configurable behavior-tree node.
/// </summary>
public abstract class AIBehaviorTreeNode : ScriptableObject
{
    [SerializeField] private string displayName;

    public string DisplayName
    {
        get
        {
            return string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;
        }
    }

    /// <summary>
    /// Evaluates this node for one AI-controlled actor.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>The result of evaluating the node.</returns>
    public abstract EBehaviorTreeResult Evaluate(
        AIBehaviorContext context);
}