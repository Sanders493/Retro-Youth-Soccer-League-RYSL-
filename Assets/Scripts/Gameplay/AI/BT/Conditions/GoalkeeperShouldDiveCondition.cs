using UnityEngine;

/// <summary>
/// Checks whether the selected interception point requires a goalkeeper
/// dive rather than normal movement.
/// </summary>
[CreateAssetMenu(
    fileName = "Goalkeeper Should Dive",
    menuName =
        "Soccer AI/Nodes/Conditions/Goalkeeper Should Dive")]
public sealed class GoalkeeperShouldDiveCondition :
    AIConditionNode
{
    [SerializeField]
    private float minimumDiveDistance = 1.25f;

    [SerializeField]
    private float maximumDiveDistance = 4f;

    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || !context.Actor.IsGoalkeeper
            || !context.HasSelectedPosition)
        {
            return false;
        }

        float distance =
            Vector2.Distance(
                context.Actor.Position,
                context.SelectedPosition);

        return distance >= minimumDiveDistance
               && distance <= maximumDiveDistance;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minimumDiveDistance =
            Mathf.Max(
                0f,
                minimumDiveDistance);

        maximumDiveDistance =
            Mathf.Max(
                minimumDiveDistance,
                maximumDiveDistance);
    }
#endif
}