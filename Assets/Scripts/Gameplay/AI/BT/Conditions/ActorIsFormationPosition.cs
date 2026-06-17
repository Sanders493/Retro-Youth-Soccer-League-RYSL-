using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks whether the actor's formation position is included in a
/// designer-configured list of permitted positions.
/// </summary>
[CreateAssetMenu(
    fileName = "Actor Is Formation Position",
    menuName = "Soccer AI/Nodes/Conditions/Actor Is Formation Position")]
public sealed class ActorIsFormationPositionCondition :
    AIConditionNode
{
    [Tooltip(
        "The formation positions permitted to pass this condition.")]
    public List<EFormationPosition> allowedFormationPositions =
        new List<EFormationPosition>();

    /// <summary>
    /// Checks whether the actor's assigned formation position is permitted.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the actor's formation position is included in the
    /// configured list.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || allowedFormationPositions == null
            || allowedFormationPositions.Count == 0)
        {
            return false;
        }

        return allowedFormationPositions.Contains(
            context.FormationPosition);
    }
}