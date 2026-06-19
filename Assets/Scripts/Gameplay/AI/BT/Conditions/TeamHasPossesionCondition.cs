using UnityEngine;

/// <summary>
/// Checks whether the actor's team currently possesses the ball and the actor
/// is eligible to provide attacking support.
/// </summary>
[CreateAssetMenu(
    fileName = "Team Has Possession",
    menuName = "Soccer AI/Nodes/Conditions/Team Has Possession")]
public sealed class TeamHasPossessionCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the actor should enter the attacking-support branch.
    /// </summary>
    /// <param name="context">
    /// The actor's current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when the actor's team possesses the ball and this actor is not
    /// the ball owner or goalkeeper.
    /// </returns>
    protected override bool CheckCondition(
        AIBehaviorContext context)
    {
        if (context.Actor == null
            || context.GameState == null)
        {
            return false;
        }

        return context.GameState.IsMatchActive
               && context.Actor.IsActive
               && context.Actor.IsAIControlled
               && !context.Actor.HasBall
               && !context.Actor.IsGoalkeeper
               && context.GameState.HasPossession(
                   context.Actor.TeamId);
    }
}