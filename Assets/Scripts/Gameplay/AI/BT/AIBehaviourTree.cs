using UnityEngine;

/// <summary>
/// Stores a designer-authored behavior tree.
/// </summary>
[CreateAssetMenu(
    fileName = "AI Behavior Tree",
    menuName = "Soccer AI/Behavior Tree")]
public sealed class AIBehaviorTree : ScriptableObject
{
    [SerializeField] private AIBehaviorTreeNode rootNode;

    /// <summary>
    /// Evaluates the tree for one actor.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>The result returned by the root node.</returns>
    public EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (rootNode == null)
            return EBehaviorTreeResult.Failure;

        return rootNode.Evaluate(context);
    }
}