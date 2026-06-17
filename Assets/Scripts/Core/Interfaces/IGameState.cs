using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides read-only match information required by the AI system.
/// </summary>
public interface IGameState
{
    bool IsMatchActive { get; }
    
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

    

    ETeamId TeamInPossession { get; }

    bool HasBallOwner { get; }

    IAIActor BallOwner { get; }

    Vector2 BallPosition { get; }
    
    Vector2 BallVelocity { get; }
    
    bool HasActivePass { get; }

    string IntendedPassReceiverId { get; }

    Vector2 IntendedPassTargetPosition { get; }

    /// <summary>
    /// Determines whether the specified team currently possesses the ball.
    /// </summary>
    /// <param name="TeamId">The team to check.</param>
    /// <returns>True when the team possesses the ball; otherwise, false.</returns>
    bool HasPossession(ETeamId TeamId);
    

    Bounds FieldBounds { get; }
    
    /// <summary>
    /// Converts a world position into team-relative normalized field coordinates.
    /// The horizontal value ranges from -1 on the team's left side to 1 on its
    /// right side. The vertical value ranges from 0 at the defending goal to 1 at
    /// the attacking goal.
    /// </summary>
    /// <param name="teamId">The team whose perspective should be used.</param>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>The normalized team-relative field position.</returns>
    Vector2 GetTeamRelativeFieldPosition(
        ETeamId teamId,
        Vector2 worldPosition);

    /// <summary>
    /// Determines whether a world position is inside the specified team's
    /// defending penalty box.
    /// </summary>
    /// <param name="teamId">The defending team.</param>
    /// <param name="worldPosition">The position being checked.</param>
    /// <returns>
    /// True when the position is inside the team's penalty box.
    /// </returns>
    bool IsInsideDefendingPenaltyBox(
        ETeamId teamId,
        Vector2 worldPosition);

    /// <summary>
    /// Converts team-relative normalized field coordinates into a world position.
    /// </summary>
    /// <param name="teamId">The team whose perspective should be used.</param>
    /// <param name="teamRelativePosition">
    /// A position where horizontal ranges from -1 to 1 and vertical ranges from
    /// 0 at the defending goal to 1 at the attacking goal.
    /// </param>
    /// <returns>The corresponding world position.</returns>
    Vector2 GetWorldPositionFromTeamRelative(
        ETeamId teamId,
        Vector2 teamRelativePosition);
    
    /// <summary>
    /// Converts a formation position into a world position for the specified team.
    /// </summary>
    /// <param name="teamId">The team using the formation position.</param>
    /// <param name="formationPosition">The abstract position on the field.</param>
    /// <returns>The corresponding world position.</returns>
    Vector2 GetFormationWorldPosition(
        ETeamId teamId,
        EFormationPosition formationPosition);
    
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

    bool IsInsideGoalArea(
        ETeamId teamId,
        Vector2 worldPosition);
    
}