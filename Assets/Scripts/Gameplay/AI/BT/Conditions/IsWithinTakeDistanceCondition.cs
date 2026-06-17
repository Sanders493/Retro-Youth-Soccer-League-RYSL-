using UnityEngine;

/// <summary>
/// Checks whether an actor is close enough to take possession of the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Is Within Take Distance",
    menuName = "Soccer AI/Nodes/Conditions/Is Within Take Distance")]
public sealed class IsWithinTakeDistanceCondition :
    AIConditionNode
{
    [SerializeField] private float takeDistance = 0.75f;

    /// <summary>
    /// Checks the actor's distance from the current ball position.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the actor is close enough to take the ball.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context.Actor == null
            || context.GameState == null
            || context.Actor.HasBall
            || context.GameState.HasBallOwner)
        {
            return false;
        }

        float distanceSquared =
            (context.GameState.BallPosition
             - context.Actor.Position).sqrMagnitude;

        float takeDistanceSquared =
            takeDistance * takeDistance;

        return distanceSquared <= takeDistanceSquared;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the take distance to a valid value.
    /// </summary>
    private void OnValidate()
    {
        takeDistance =
            Mathf.Max(0f, takeDistance);
    }
#endif
}