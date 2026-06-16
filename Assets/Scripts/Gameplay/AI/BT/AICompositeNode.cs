using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a base class for nodes that contain child nodes.
/// </summary>
public abstract class AICompositeNode : AIBehaviorTreeNode
{
    [SerializeField] private List<AIBehaviorTreeNode> children = new();

    protected IReadOnlyList<AIBehaviorTreeNode> Children => children;
}