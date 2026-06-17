using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stores a designer-authored behavior tree.
/// </summary>
[CreateAssetMenu(
    fileName = "AI Behavior Tree",
    menuName = "Soccer AI/Behavior Tree")]
public sealed class AIBehaviorTree : ScriptableObject
{
    [SerializeField]
    private AIBehaviorTreeNode rootNode;

    /// <summary>
    /// Gets the root node of the behavior tree.
    /// </summary>
    public AIBehaviorTreeNode RootNode =>
        rootNode;

    /// <summary>
    /// Evaluates the tree for one actor.
    /// </summary>
    /// <param name="context">
    /// The actor's current AI context.
    /// </param>
    /// <returns>
    /// The result returned by the root node.
    /// </returns>
    public EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (rootNode == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        return rootNode.Evaluate(context);
    }

#if UNITY_EDITOR

    /// <summary>
    /// Prints the complete serialized structure of the behavior tree.
    /// </summary>
    [ContextMenu("Print Full Tree Structure")]
    public void PrintFullTreeStructure()
    {
        if (rootNode == null)
        {
            Debug.LogWarning(
                $"Behavior tree '{name}' does not have a root node.",
                this);

            return;
        }

        StringBuilder builder =
            new StringBuilder();

        HashSet<AIBehaviorTreeNode> visitedNodes =
            new HashSet<AIBehaviorTreeNode>();

        builder.AppendLine(
            $"Behavior Tree: {name}");

        AppendNodeStructure(
            rootNode,
            builder,
            string.Empty,
            true,
            visitedNodes);

        Debug.Log(
            builder.ToString(),
            this);
    }

    /// <summary>
    /// Recursively adds a node and its serialized child nodes to the output.
    /// </summary>
    /// <param name="node">
    /// The node currently being printed.
    /// </param>
    /// <param name="builder">
    /// The text builder receiving the tree structure.
    /// </param>
    /// <param name="indent">
    /// The indentation inherited from the parent node.
    /// </param>
    /// <param name="isLastChild">
    /// Whether the node is the final child of its parent.
    /// </param>
    /// <param name="visitedNodes">
    /// Nodes already visited during this traversal.
    /// </param>
    private static void AppendNodeStructure(
        AIBehaviorTreeNode node,
        StringBuilder builder,
        string indent,
        bool isLastChild,
        HashSet<AIBehaviorTreeNode> visitedNodes)
    {
        string branch =
            isLastChild
                ? "└── "
                : "├── ";

        if (node == null)
        {
            builder
                .Append(indent)
                .Append(branch)
                .AppendLine("<Missing Node>");

            return;
        }

        builder
            .Append(indent)
            .Append(branch)
            .Append(GetNodeDisplayName(node));

        if (!visitedNodes.Add(node))
        {
            builder.AppendLine(" [Circular Reference]");

            return;
        }

        builder.AppendLine();

        List<AIBehaviorTreeNode> children =
            GetSerializedChildNodes(node);

        if (children.Count == 0)
        {
            return;
        }

        string childIndent =
            indent
            + (isLastChild
                ? "    "
                : "│   ");

        for (int index = 0;
             index < children.Count;
             index++)
        {
            bool childIsLast =
                index == children.Count - 1;

            AppendNodeStructure(
                children[index],
                builder,
                childIndent,
                childIsLast,
                visitedNodes);
        }
    }

    /// <summary>
    /// Finds every serialized behavior-tree node referenced by a node.
    /// </summary>
    /// <param name="node">
    /// The node whose serialized properties are inspected.
    /// </param>
    /// <returns>
    /// The node's serialized child nodes in Inspector order.
    /// </returns>
    private static List<AIBehaviorTreeNode> GetSerializedChildNodes(
        AIBehaviorTreeNode node)
    {
        List<AIBehaviorTreeNode> children =
            new List<AIBehaviorTreeNode>();

        SerializedObject serializedNode =
            new SerializedObject(node);

        SerializedProperty iterator =
            serializedNode.GetIterator();

        bool enterChildren =
            true;

        while (iterator.NextVisible(
                   enterChildren))
        {
            enterChildren =
                true;

            if (iterator.propertyPath == "m_Script")
            {
                continue;
            }

            if (iterator.propertyType
                != SerializedPropertyType.ObjectReference)
            {
                continue;
            }

            AIBehaviorTreeNode childNode =
                iterator.objectReferenceValue
                as AIBehaviorTreeNode;

            if (childNode == null)
            {
                continue;
            }

            children.Add(childNode);

            // Do not inspect inside the referenced ScriptableObject here.
            // Its children are handled recursively by AppendNodeStructure.
            enterChildren =
                false;
        }

        return children;
    }

    /// <summary>
    /// Gets the readable label used when printing a tree node.
    /// </summary>
    /// <param name="node">
    /// The node being printed.
    /// </param>
    /// <returns>
    /// The node asset name and concrete node type.
    /// </returns>
    private static string GetNodeDisplayName(
        AIBehaviorTreeNode node)
    {
        string nodeName =
            string.IsNullOrWhiteSpace(node.name)
                ? "<Unnamed Node>"
                : node.name;

        return $"{nodeName} [{node.GetType().Name}]";
    }

#endif
}

#if UNITY_EDITOR

/// <summary>
/// Adds behavior-tree debugging controls to the AIBehaviorTree Inspector.
/// </summary>
[CustomEditor(typeof(AIBehaviorTree))]
public sealed class AIBehaviorTreeEditor : Editor
{
    /// <summary>
    /// Draws the normal Inspector and the tree-printing button.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        AIBehaviorTree behaviorTree =
            (AIBehaviorTree)target;

        using (new EditorGUI.DisabledScope(
                   behaviorTree.RootNode == null))
        {
            if (GUILayout.Button(
                    "Print Full Tree Structure",
                    GUILayout.Height(32f)))
            {
                behaviorTree.PrintFullTreeStructure();
            }
        }

        if (behaviorTree.RootNode == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a root node before printing the tree.",
                MessageType.Info);
        }
    }
}

#endif