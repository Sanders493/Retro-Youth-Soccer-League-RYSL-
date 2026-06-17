using UnityEngine;

/// <summary>
/// Checks whether the ball is on a field side permitted by the actor's
/// assigned formation position.
/// </summary>
[CreateAssetMenu(
    fileName = "Actor Matches Ball Side",
    menuName = "Soccer AI/Nodes/Conditions/Actor Matches Ball Side")]
public sealed class ActorMatchesBallSideCondition :
    AIConditionNode
{
    [Tooltip(
        "When enabled, the ball must be on the actor's exact assigned side.")]
    [SerializeField]
    private bool strict;

    [Tooltip(
        "The normalized boundary separating the center from each side.")]
    [SerializeField, Range(0f, 1f)]
    private float sideBoundary = 1f / 3f;

    /// <summary>
    /// Checks whether the ball is on a side permitted by the actor's
    /// assigned formation position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the ball is on the actor's permitted side.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        EFieldSideRegion actorSide =
            GetFormationSide(
                context.FormationPosition);

        EFieldSideRegion ballSide =
            GetFieldSide(
                ballFieldPosition.y);

        if (strict)
            return actorSide == ballSide;

        switch (actorSide)
        {
            case EFieldSideRegion.Left:
                return ballSide == EFieldSideRegion.Left
                    || ballSide == EFieldSideRegion.Center;

            case EFieldSideRegion.Center:
                return true;

            case EFieldSideRegion.Right:
                return ballSide == EFieldSideRegion.Right
                    || ballSide == EFieldSideRegion.Center;

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the side assigned to a formation position.
    /// </summary>
    /// <param name="formationPosition">
    /// The actor's assigned formation position.
    /// </param>
    /// <returns>The formation side.</returns>
    private EFieldSideRegion GetFormationSide(
        EFormationPosition formationPosition)
    {
        switch (formationPosition)
        {
            case EFormationPosition.LeftDefender:
            case EFormationPosition.LeftMidfielder:
            case EFormationPosition.LeftForward:
                return EFieldSideRegion.Left;

            case EFormationPosition.RightDefender:
            case EFormationPosition.RightMidfielder:
            case EFormationPosition.RightForward:
                return EFieldSideRegion.Right;

            case EFormationPosition.Goalkeeper:
            case EFormationPosition.CenterDefender:
            case EFormationPosition.CenterMidfielder:
            case EFormationPosition.CenterForward:
                return EFieldSideRegion.Center;

            default:
                return EFieldSideRegion.Any;
        }
    }

    /// <summary>
    /// Determines the field side containing a lateral team-relative position.
    /// </summary>
    /// <param name="lateralPosition">
    /// The team-relative left/right position.
    /// </param>
    /// <returns>The corresponding field side.</returns>
    private EFieldSideRegion GetFieldSide(
        float lateralPosition)
    {
        if (lateralPosition < -sideBoundary)
            return EFieldSideRegion.Left;

        if (lateralPosition > sideBoundary)
            return EFieldSideRegion.Right;

        return EFieldSideRegion.Center;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the side boundary to a valid normalized range.
    /// </summary>
    private void OnValidate()
    {
        sideBoundary =
            Mathf.Clamp01(
                sideBoundary);
    }
#endif
}