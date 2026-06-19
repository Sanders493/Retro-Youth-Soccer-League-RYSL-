using UnityEngine;

/// <summary>
/// Checks whether an actor's formation position supports a configured side
/// of the field.
/// </summary>
[CreateAssetMenu(
    fileName = "Actor Supports Field Side",
    menuName = "Soccer AI/Nodes/Conditions/Actor Supports Field Side")]
public sealed class ActorSupportsFieldSideCondition :
    AIConditionNode
{
    [SerializeField] private ESupportSide supportSide;

    /// <summary>
    /// Checks whether the actor can support the configured side.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context.Actor == null)
            return false;

        EFormationPosition position =
            context.FormationPosition;

        bool isLeft =
            position == EFormationPosition.LeftDefender
            || position == EFormationPosition.LeftMidfielder
            || position == EFormationPosition.LeftForward;

        bool isCenter =
            position == EFormationPosition.CenterDefender
            || position == EFormationPosition.CenterMidfielder
            || position == EFormationPosition.CenterForward;

        bool isRight =
            position == EFormationPosition.RightDefender
            || position == EFormationPosition.RightMidfielder
            || position == EFormationPosition.RightForward;

        switch (supportSide)
        {
            case ESupportSide.Left:
                return isLeft || isCenter;

            case ESupportSide.Center:
                return isLeft || isCenter || isRight;

            case ESupportSide.Right:
                return isRight || isCenter;

            default:
                return false;
        }
    }
}