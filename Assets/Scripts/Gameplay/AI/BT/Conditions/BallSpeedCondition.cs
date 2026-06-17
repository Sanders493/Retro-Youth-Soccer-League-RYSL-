using UnityEngine;

/// <summary>
/// Checks whether the ball is moving at or above a configured speed.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball Speed At Least",
    menuName =
        "Soccer AI/Nodes/Conditions/Ball Speed At Least")]
public sealed class BallSpeedAtLeastCondition :
    AIConditionNode
{
    [SerializeField]
    private float minimumSpeed = 4f;

    /// <summary>
    /// Checks the current ball speed.
    /// </summary>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.GameState == null)
        {
            return false;
        }

        return context.GameState.BallVelocity.magnitude
               >= minimumSpeed;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minimumSpeed =
            Mathf.Max(
                0f,
                minimumSpeed);
    }
#endif
}