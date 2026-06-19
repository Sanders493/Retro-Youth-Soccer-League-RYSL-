using UnityEngine;

/// <summary>
/// Provides actor-specific runtime information while evaluating a behavior
/// tree. Shared behavior-tree node assets should store temporary state here
/// rather than in their ScriptableObject fields.
/// </summary>
public sealed class AIBehaviorContext
{
    /// <summary>
    /// Gets the actor currently being evaluated.
    /// </summary>
    public IAIActor Actor
    {
        get;
    }

    /// <summary>
    /// Gets the current read-only match state.
    /// </summary>
    public IGameState GameState
    {
        get;
    }

    /// <summary>
    /// Gets the actor's assigned formation position.
    /// </summary>
    public EFormationPosition FormationPosition
    {
        get;
    }

    /// <summary>
    /// Gets the team's current tactical state.
    /// </summary>
    public ETeamAIState TeamState
    {
        get;
    }

    /// <summary>
    /// Gets whether this actor is the selected loose-ball chaser.
    /// </summary>
    public bool IsPrimaryBallChaser
    {
        get;
    }

    /// <summary>
    /// Gets whether this actor is the selected pressure defender.
    /// </summary>
    public bool IsPrimaryDefender
    {
        get;
    }

    /// <summary>
    /// Gets the assignment produced by the current evaluation.
    /// </summary>
    public ActorAssignment Assignment
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the actor selected by a condition or selection node.
    /// </summary>
    public IAIActor SelectedActor
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the world-space position selected by a selection node.
    /// </summary>
    public Vector2 SelectedPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets whether a valid selected position is currently stored.
    /// </summary>
    public bool HasSelectedPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the branch selected by the root selector.
    /// </summary>
    public string RootSelection
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the branch selected by the first internal selector beneath the
    /// root selection.
    /// </summary>
    public string NestedSelectorSelection
    {
        get;
        private set;
    }

    /// <summary>
    /// Creates a behavior-tree context for one actor.
    /// </summary>
    /// <param name="actor">
    /// The actor being evaluated.
    /// </param>
    /// <param name="gameState">
    /// The current read-only match state.
    /// </param>
    /// <param name="formationPosition">
    /// The formation position assigned to the actor.
    /// </param>
    /// <param name="teamState">
    /// The team's current tactical state.
    /// </param>
    /// <param name="isPrimaryBallChaser">
    /// Whether this actor was selected to pursue the loose ball.
    /// </param>
    /// <param name="isPrimaryDefender">
    /// Whether this actor was selected to pressure the opposing ball owner.
    /// </param>
    public AIBehaviorContext(
        IAIActor actor,
        IGameState gameState,
        EFormationPosition formationPosition,
        ETeamAIState teamState,
        bool isPrimaryBallChaser,
        bool isPrimaryDefender)
    {
        Actor =
            actor;

        GameState =
            gameState;

        FormationPosition =
            formationPosition;

        TeamState =
            teamState;

        IsPrimaryBallChaser =
            isPrimaryBallChaser;

        IsPrimaryDefender =
            isPrimaryDefender;

        ResetResults();
    }

    /// <summary>
    /// Stores the assignment produced by an action node.
    /// </summary>
    /// <param name="assignment">
    /// The assignment to store.
    /// </param>
    public void SetAssignment(
        ActorAssignment assignment)
    {
        Assignment =
            assignment;
    }

    /// <summary>
    /// Clears the assignment produced during evaluation.
    /// </summary>
    public void ClearAssignment()
    {
        Assignment =
            null;
    }

    /// <summary>
    /// Stores an actor selected during tree evaluation.
    /// </summary>
    /// <param name="actor">
    /// The actor to store.
    /// </param>
    public void SetSelectedActor(
        IAIActor actor)
    {
        SelectedActor =
            actor;
    }

    /// <summary>
    /// Clears the currently selected actor.
    /// </summary>
    public void ClearSelectedActor()
    {
        SelectedActor =
            null;
    }

    /// <summary>
    /// Stores a world-space position selected during tree evaluation.
    /// </summary>
    /// <param name="position">
    /// The selected world-space position.
    /// </param>
    public void SetSelectedPosition(
        Vector2 position)
    {
        SelectedPosition =
            position;

        HasSelectedPosition =
            true;
    }

    /// <summary>
    /// Clears the currently selected world-space position.
    /// </summary>
    public void ClearSelectedPosition()
    {
        SelectedPosition =
            default;

        HasSelectedPosition =
            false;
    }

    /// <summary>
    /// Stores both a selected actor and its current world-space position.
    /// </summary>
    /// <param name="actor">
    /// The selected actor.
    /// </param>
    public void SetSelectedActorAndPosition(
        IAIActor actor)
    {
        SetSelectedActor(
            actor);

        if (actor == null)
        {
            ClearSelectedPosition();
            return;
        }

        SetSelectedPosition(
            actor.Position);
    }

    /// <summary>
    /// Clears all target-selection values.
    /// </summary>
    public void ClearSelection()
    {
        ClearSelectedActor();
        ClearSelectedPosition();
    }

    /// <summary>
    /// Records the branch selected by the root selector.
    /// </summary>
    /// <param name="nodeName">
    /// The selected root-child node name.
    /// </param>
    public void SetRootSelection(
        string nodeName)
    {
        RootSelection =
            string.IsNullOrWhiteSpace(
                nodeName)
                ? string.Empty
                : nodeName;
    }

    /// <summary>
    /// Records the branch selected by the first internal selector beneath
    /// the root selection.
    /// </summary>
    /// <param name="nodeName">
    /// The selected internal child node name.
    /// </param>
    public void SetNestedSelectorSelection(
        string nodeName)
    {
        NestedSelectorSelection =
            string.IsNullOrWhiteSpace(
                nodeName)
                ? string.Empty
                : nodeName;
    }

    /// <summary>
    /// Clears recorded behavior-tree selector results.
    /// </summary>
    public void ClearTreeSelections()
    {
        RootSelection =
            string.Empty;

        NestedSelectorSelection =
            string.Empty;
    }

    /// <summary>
    /// Gets a readable summary of the selector branches that produced the
    /// current result.
    /// </summary>
    /// <returns>
    /// The root selection followed by the nested selector selection when one
    /// exists.
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

    /// <summary>
    /// Clears every temporary value produced during the previous tree
    /// evaluation.
    /// </summary>
    public void ResetResults()
    {
        ClearAssignment();
        ClearSelection();
        ClearTreeSelections();
    }
}