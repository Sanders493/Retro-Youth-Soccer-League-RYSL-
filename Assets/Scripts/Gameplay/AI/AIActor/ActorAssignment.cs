using UnityEngine;

/// <summary>
/// Describes the action selected by an actor's behavior tree.
/// </summary>
public sealed class ActorAssignment
{
    /// <summary>
    /// Gets the identifier of the actor receiving the assignment.
    /// </summary>
    public string ActorId
    {
        get;
    }

    /// <summary>
    /// Gets the gameplay action the actor should perform.
    /// </summary>
    public EAIActionType ActionType
    {
        get;
    }

    /// <summary>
    /// Gets the world-space target used by the action.
    /// </summary>
    public Vector2 TargetPosition
    {
        get;
    }

    /// <summary>
    /// Gets the optional identifier of the targeted actor.
    /// </summary>
    public string TargetActorId
    {
        get;
    }

    /// <summary>
    /// Gets the branch selected by the behavior tree's root selector.
    /// </summary>
    public string RootSelection
    {
        get;
    }

    /// <summary>
    /// Gets the child selected by the first internal selector beneath the
    /// selected root branch.
    /// </summary>
    public string NestedSelectorSelection
    {
        get;
    }

    /// <summary>
    /// Creates an assignment for an AI-controlled actor.
    /// </summary>
    /// <param name="actorId">
    /// The identifier of the assigned actor.
    /// </param>
    /// <param name="actionType">
    /// The action the actor should perform.
    /// </param>
    /// <param name="targetPosition">
    /// The world-space target used by the action.
    /// </param>
    /// <param name="targetActorId">
    /// The optional identifier of a targeted actor.
    /// </param>
    /// <param name="rootSelection">
    /// The branch selected by the root selector.
    /// </param>
    /// <param name="nestedSelectorSelection">
    /// The branch selected by the first internal selector beneath the root
    /// selection.
    /// </param>
    public ActorAssignment(
        string actorId,
        EAIActionType actionType,
        Vector2 targetPosition,
        string targetActorId = null,
        string rootSelection = null,
        string nestedSelectorSelection = null)
    {
        ActorId =
            actorId;

        ActionType =
            actionType;

        TargetPosition =
            targetPosition;

        TargetActorId =
            targetActorId;

        RootSelection =
            rootSelection;

        NestedSelectorSelection =
            nestedSelectorSelection;
    }

    /// <summary>
    /// Creates a copy containing the selector branches that produced this
    /// assignment.
    /// </summary>
    /// <param name="rootSelection">
    /// The branch selected by the root selector.
    /// </param>
    /// <param name="nestedSelectorSelection">
    /// The branch selected by the first internal selector.
    /// </param>
    /// <returns>
    /// A copy containing the supplied tree-selection information.
    /// </returns>
    public ActorAssignment WithTreeSelection(
        string rootSelection,
        string nestedSelectorSelection)
    {
        return new ActorAssignment(
            ActorId,
            ActionType,
            TargetPosition,
            TargetActorId,
            rootSelection,
            nestedSelectorSelection);
    }

    /// <summary>
    /// Gets a readable representation of the behavior-tree selection.
    /// </summary>
    /// <returns>
    /// The root branch and optional nested selector branch.
    /// </returns>
    public string GetTreeSelectionText()
    {
        if (string.IsNullOrWhiteSpace(
                RootSelection))
        {
            return "No Selection";
        }

        if (string.IsNullOrWhiteSpace(
                NestedSelectorSelection))
        {
            return RootSelection;
        }

        return RootSelection
            + " → "
            + NestedSelectorSelection;
    }
}