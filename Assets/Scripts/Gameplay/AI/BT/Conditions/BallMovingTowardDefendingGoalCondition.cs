using UnityEngine;

/// <summary>
/// Checks whether the ball is moving toward the actor's defending goal.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball Moving Toward Defending Goal",
    menuName =
        "Soccer AI/Nodes/Conditions/Ball Moving Toward Defending Goal")]
public sealed class BallMovingTowardDefendingGoalCondition :
    AIConditionNode
{
    [Tooltip(
        "The minimum directional alignment required toward the goal.")]
    [SerializeField, Range(0f, 1f)]
    private float minimumAlignment = 0.35f;

    /// <summary>
    /// Checks whether ball velocity points toward the defending goal.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        Vector2 velocity =
            context.GameState.BallVelocity;

        if (velocity.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return false;
        }

        Vector2 defendingGoal =
            context.GameState.GetDefendingGoalPosition(
                context.Actor.TeamId);

        Vector2 directionToGoal =
            defendingGoal
            - context.GameState.BallPosition;

        if (directionToGoal.sqrMagnitude
            <= Mathf.Epsilon)
        {
            return true;
        }

        float alignment =
            Vector2.Dot(
                velocity.normalized,
                directionToGoal.normalized);

        return alignment >= minimumAlignment;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minimumAlignment =
            Mathf.Clamp01(
                minimumAlignment);
    }
#endif
}