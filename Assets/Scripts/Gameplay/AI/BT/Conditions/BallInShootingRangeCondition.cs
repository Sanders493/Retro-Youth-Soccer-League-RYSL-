using UnityEngine;

/// <summary>
/// Checks whether the actor is close enough to the opposing goal to shoot.
/// </summary>
[CreateAssetMenu(
    fileName = "Ball In Shooting Range",
    menuName = "Soccer AI/Nodes/Conditions/Ball In Shooting Range")]
public sealed class BallInShootingRangeCondition :
    AIConditionNode
{
    [SerializeField]
    private float maximumShootingDistance = 6f;

    /// <summary>
    /// Checks whether the actor is within shooting range of the opposing goal.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the actor is within the configured shooting distance.
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

        Vector2 goalPosition =
            context.GameState.GetAttackingGoalPosition(
                context.Actor.TeamId);

        float distanceSquared =
            (goalPosition - context.Actor.Position)
            .sqrMagnitude;

        return distanceSquared
               <= maximumShootingDistance
               * maximumShootingDistance;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts the shooting distance to a valid value.
    /// </summary>
    private void OnValidate()
    {
        maximumShootingDistance =
            Mathf.Max(
                0f,
                maximumShootingDistance);
    }
#endif
}