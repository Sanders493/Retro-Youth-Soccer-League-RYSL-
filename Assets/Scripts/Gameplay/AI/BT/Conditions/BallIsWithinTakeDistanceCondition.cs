using UnityEngine;

/// <summary>
/// Checks whether the actor is close enough to attempt taking the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball Is Within Take Range",
    menuName = "Soccer AI/Nodes/Conditions/Ball Is Within Take Range")]
public sealed class BallIsWithinTakeRangeCondition :
    AIConditionNode
{
    [Tooltip(
        "The maximum world-space distance at which the actor may attempt " +
        "to take possession.")]
    [SerializeField]
    private float takeBallDistance = 1.5f;

    /// <summary>
    /// Checks whether the actor is sufficiently close to the ball.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null
            || context.Actor.HasBall)
        {
            return false;
        }

        IAIActor ballOwner =
            context.GameState.BallOwner;

        if (ballOwner != null
            && ballOwner.TeamId == context.Actor.TeamId)
        {
            return false;
        }

        float distanceSquared =
            (context.GameState.BallPosition
             - context.Actor.Position)
            .sqrMagnitude;

        return distanceSquared
               <= takeBallDistance
               * takeBallDistance;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        takeBallDistance =
            Mathf.Max(
                0f,
                takeBallDistance);
    }
#endif
}