using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides read-only match information required by the AI system.
/// </summary>
public interface IGameState
{
    Vector2 BallPosition { get; }

    Vector2 BallVelocity { get; }

    Bounds FieldBounds { get; }

    ETeamId TeamInPossession { get; }

    bool HasBallOwner { get; }

    IAIActor BallOwner { get; }

    bool IsMatchActive { get; }

    float RemainingMatchTime { get; }

    /// <summary>
    /// Returns every active actor in the match.
    /// </summary>
    /// <returns>A read-only collection of match actors.</returns>
    IReadOnlyList<IAIActor> GetAllActors();

    /// <summary>
    /// Returns every active actor belonging to the specified team.
    /// </summary>
    /// <param name="TeamId">The team whose actors should be returned.</param>
    /// <returns>A read-only collection of actors belonging to the team.</returns>
    IReadOnlyList<IAIActor> GetTeamActors(ETeamId TeamId);

    /// <summary>
    /// Returns the actor with the specified identifier.
    /// </summary>
    /// <param name="actorId">The unique actor identifier.</param>
    /// <returns>The matching actor, or null when no actor is found.</returns>
    IAIActor GetActor(string actorId);

    /// <summary>
    /// Returns the goal position that the specified team is attacking.
    /// </summary>
    /// <param name="TeamId">The attacking team.</param>
    /// <returns>The world position of the opposing goal.</returns>
    Vector2 GetAttackingGoalPosition(ETeamId TeamId);

    /// <summary>
    /// Returns the goal position that the specified team is defending.
    /// </summary>
    /// <param name="TeamId">The defending team.</param>
    /// <returns>The world position of the team's goal.</returns>
    Vector2 GetDefendingGoalPosition(ETeamId TeamId);

    /// <summary>
    /// Determines whether the specified team currently possesses the ball.
    /// </summary>
    /// <param name="TeamId">The team to check.</param>
    /// <returns>True when the team possesses the ball; otherwise, false.</returns>
    bool HasPossession(ETeamId TeamId);
}