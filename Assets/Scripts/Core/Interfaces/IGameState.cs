using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides read-only match information required by the soccer AI systems.
/// </summary>
public interface IGameState
{
    Vector2 BallPosition { get; }

    Vector2 BallVelocity { get; }

    Player BallOwner { get; }

    IReadOnlyList<Player> HomeTeamPlayers { get; }

    IReadOnlyList<Player> AwayTeamPlayers { get; }

    Vector2 HomeGoalPosition { get; }

    Vector2 AwayGoalPosition { get; }

    Bounds FieldBounds { get; }

    Team TeamInPossession { get; }

    MatchState CurrentMatchState { get; }

    float RemainingMatchTime { get; }

    /// <summary>
    /// Returns the players belonging to the specified team.
    /// </summary>
    /// <param name="team">The team whose players should be returned.</param>
    /// <returns>A read-only collection of players on the team.</returns>
    IReadOnlyList<Player> GetPlayers(Team team);

    /// <summary>
    /// Returns the goal that the specified team is attacking.
    /// </summary>
    /// <param name="team">The attacking team.</param>
    /// <returns>The world position of the opposing goal.</returns>
    Vector2 GetAttackingGoalPosition(Team team);

    /// <summary>
    /// Returns the goal that the specified team is defending.
    /// </summary>
    /// <param name="team">The defending team.</param>
    /// <returns>The world position of the team's own goal.</returns>
    Vector2 GetDefendingGoalPosition(Team team);

    /// <summary>
    /// Determines whether the specified team currently possesses the ball.
    /// </summary>
    /// <param name="team">The team to check.</param>
    /// <returns>True when the team possesses the ball; otherwise, false.</returns>
    bool HasPossession(Team team);
}