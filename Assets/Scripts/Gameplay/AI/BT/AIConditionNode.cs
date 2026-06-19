using UnityEngine;

/// <summary>
/// Provides a base class for behavior-tree condition nodes.
/// </summary>
public abstract class AIConditionNode : AIBehaviorTreeNode
{
    [SerializeField] private bool invertResult;

    /// <summary>
    /// Evaluates the condition and optionally inverts its result.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>Success when the condition passes; otherwise, Failure.</returns>
    public sealed override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        bool conditionPassed =
            context != null && CheckCondition(context);

        if (invertResult)
            conditionPassed = !conditionPassed;

        return conditionPassed
            ? EBehaviorTreeResult.Success
            : EBehaviorTreeResult.Failure;
    }

    /// <summary>
    /// Checks this node's condition.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <returns>True when the condition passes.</returns>
    protected abstract bool CheckCondition(
        AIBehaviorContext context);
}