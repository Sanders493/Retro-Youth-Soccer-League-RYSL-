using UnityEngine;

/// <summary>
/// Checks whether the opposing team currently has possession of the ball.
/// </summary>
[CreateAssetMenu(
    fileName = "Opponent Has Possession",
    menuName = "Soccer AI/Nodes/Conditions/Opponent Has Possession")]
public sealed class OpponentHasPossessionCondition :
    AIConditionNode
{
    /// <summary>
    /// Checks whether the opposing team currently possesses or most recently
    /// controlled the ball.
    /// </summary>
    /// <param name="context">
    /// The current behavior-tree context.
    /// </param>
    /// <returns>
    /// True when possession belongs to the opposing team.
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

        ETeamId opposingTeam =
            GetOpposingTeam(
                context.Actor.TeamId);

        return context.GameState.HasPossession(
            opposingTeam);
    }

    /// <summary>
    /// Gets the opposing team.
    /// </summary>
    /// <param name="team">
    /// The actor's team.
    /// </param>
    /// <returns>The opposing team.</returns>
    private ETeamId GetOpposingTeam(
        ETeamId team)
    {
        return team == ETeamId.PlayerTeam
            ? ETeamId.EnemyTeam
            : ETeamId.PlayerTeam;
    }
}