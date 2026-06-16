using System.Collections.Generic;

/// <summary>
/// Contains the current tactical decision for one AI-controlled team.
/// </summary>
public sealed class TeamDecision
{
    private readonly List<ActorAssignment> assignments = new();

    public ETeamAIState TeamState { get; }

    public IReadOnlyList<ActorAssignment> Assignments => assignments;

    /// <summary>
    /// Creates a team decision with the specified tactical state.
    /// </summary>
    /// <param name="teamState">The current tactical state of the team.</param>
    public TeamDecision(ETeamAIState teamState)
    {
        TeamState = teamState;
    }

    /// <summary>
    /// Adds an actor assignment to the decision.
    /// </summary>
    /// <param name="assignment">The assignment to add.</param>
    public void AddAssignment(ActorAssignment assignment)
    {
        if (assignment == null)
            return;

        assignments.Add(assignment);
    }
}