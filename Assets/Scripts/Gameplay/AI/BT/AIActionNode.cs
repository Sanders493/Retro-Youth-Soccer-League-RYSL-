/// <summary>
/// Provides a base class for nodes that create actor assignments.
/// </summary>
public abstract class AIActionNode : AIBehaviorTreeNode
{
    /// <summary>
    /// Stores an assignment in the current context.
    /// </summary>
    /// <param name="context">The actor's current AI context.</param>
    /// <param name="assignment">The assignment produced by the node.</param>
    /// <returns>Success when the assignment is valid.</returns>
    protected EBehaviorTreeResult SetAssignment(
        AIBehaviorContext context,
        ActorAssignment assignment)
    {
        if (context == null || assignment == null)
            return EBehaviorTreeResult.Failure;

        context.Assignment = assignment;
        return EBehaviorTreeResult.Success;
    }
}