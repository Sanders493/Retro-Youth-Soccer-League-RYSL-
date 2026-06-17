using UnityEngine;

/// <summary>
/// Selects a designer-configured team-relative field position.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Field Position",
    menuName = "Soccer AI/Nodes/Selection/Select Field Position")]
public sealed class SelectFieldPositionNode :
    AIBehaviorTreeNode
{
    [Header("Position")]
    [SerializeField, Range(-1f, 1f)]
    private float horizontalPosition;

    [SerializeField, Range(0f, 1f)]
    private float verticalPosition;

    [Header("Formation Influence")]
    [SerializeField, Range(0f, 1f)]
    private float formationInfluence;

    [Header("Ball Influence")]
    [SerializeField, Range(0f, 1f)]
    private float horizontalBallInfluence;

    [SerializeField, Range(0f, 1f)]
    private float verticalBallInfluence;

    /// <summary>
    /// Selects and stores a team-relative world position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a position is selected.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 selectedFieldPosition =
            new Vector2(
                horizontalPosition,
                verticalPosition);

        if (formationInfluence > 0f)
        {
            Vector2 formationWorldPosition =
                context.GameState.GetFormationWorldPosition(
                    context.Actor.TeamId,
                    context.FormationPosition);

            Vector2 formationFieldPosition =
                context.GameState.GetTeamRelativeFieldPosition(
                    context.Actor.TeamId,
                    formationWorldPosition);

            selectedFieldPosition =
                Vector2.Lerp(
                    selectedFieldPosition,
                    formationFieldPosition,
                    formationInfluence);
        }

        if (horizontalBallInfluence > 0f
            || verticalBallInfluence > 0f)
        {
            Vector2 ballFieldPosition =
                context.GameState.GetTeamRelativeFieldPosition(
                    context.Actor.TeamId,
                    context.GameState.BallPosition);

            selectedFieldPosition.x =
                Mathf.Lerp(
                    selectedFieldPosition.x,
                    ballFieldPosition.x,
                    horizontalBallInfluence);

            selectedFieldPosition.y =
                Mathf.Lerp(
                    selectedFieldPosition.y,
                    ballFieldPosition.y,
                    verticalBallInfluence);
        }

        selectedFieldPosition.x =
            Mathf.Clamp(
                selectedFieldPosition.x,
                -1f,
                1f);

        selectedFieldPosition.y =
            Mathf.Clamp01(
                selectedFieldPosition.y);

        context.SelectedPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                selectedFieldPosition);

        context.HasSelectedPosition = true;

        return EBehaviorTreeResult.Success;
    }
}