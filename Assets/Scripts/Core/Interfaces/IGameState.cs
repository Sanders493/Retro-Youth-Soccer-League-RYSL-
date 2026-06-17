using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides read-only match information required by gameplay and AI systems.
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Gets whether normal match gameplay is currently active.
    /// </summary>
    bool IsMatchActive
    {
        get;
    }

    /// <summary>
    /// Gets the team that currently possesses or most recently controlled
    /// the ball.
    /// </summary>
    ETeamId TeamInPossession
    {
        get;
    }

    /// <summary>
    /// Gets whether an actor currently has full control of the ball.
    /// </summary>
    bool HasBallOwner
    {
        get;
    }

    /// <summary>
    /// Gets the actor currently controlling the ball.
    /// </summary>
    IAIActor BallOwner
    {
        get;
    }

    /// <summary>
    /// Gets the ball's current world-space position.
    /// </summary>
    Vector2 BallPosition
    {
        get;
    }

    /// <summary>
    /// Gets the ball's current world-space velocity.
    /// </summary>
    Vector2 BallVelocity
    {
        get;
    }

    /// <summary>
    /// Gets whether a pass is currently active.
    /// </summary>
    bool HasActivePass
    {
        get;
    }

    /// <summary>
    /// Gets the actor identifier of the intended pass receiver.
    /// </summary>
    string IntendedPassReceiverId
    {
        get;
    }
    /// <summary>
    /// Checks whether an actor is temporarily prevented from taking control of
    /// the ball.
    /// </summary>
    /// <param name="actor">
    /// The actor attempting to take possession.
    /// </param>
    /// <returns>
    /// True when temporary possession protection blocks the actor.
    /// </returns>
    bool IsBallControlBlockedFor(
        IAIActor actor);

    /// <summary>
    /// Gets the intended world-space destination of the active pass.
    /// </summary>
    Vector2 IntendedPassTargetPosition
    {
        get;
    }

    /// <summary>
    /// Gets the playable world-space field bounds.
    /// </summary>
    Bounds FieldBounds
    {
        get;
    }

    /// <summary>
    /// Returns every active actor currently registered with the match.
    /// </summary>
    /// <returns>
    /// A read-only collection of active match actors.
    /// </returns>
    IReadOnlyList<IAIActor> GetAllActors();

    /// <summary>
    /// Returns every active actor belonging to a team.
    /// </summary>
    /// <param name="teamId">
    /// The team whose actors should be returned.
    /// </param>
    /// <returns>
    /// A read-only collection of active actors belonging to the team.
    /// </returns>
    IReadOnlyList<IAIActor> GetTeamActors(
        ETeamId teamId);

    /// <summary>
    /// Returns the actor with the specified identifier.
    /// </summary>
    /// <param name="actorId">
    /// The unique actor identifier.
    /// </param>
    /// <returns>
    /// The matching actor, or null when no actor is found.
    /// </returns>
    IAIActor GetActor(
        string actorId);

    /// <summary>
    /// Determines whether the specified team currently possesses or most
    /// recently controlled the ball.
    /// </summary>
    /// <param name="teamId">
    /// The team to check.
    /// </param>
    /// <returns>
    /// True when possession belongs to the specified team.
    /// </returns>
    bool HasPossession(
        ETeamId teamId);
    
    /// <summary>
    /// Gets the team of the actor that most recently touched the ball.
    /// </summary>
    ETeamId LastTouchTeam
    {
        get;
    }
    
    /// <summary>
    /// Converts a world-space position into normalized team-relative field
    /// coordinates.
    /// </summary>
    /// <param name="teamId">
    /// The team whose perspective should be used.
    /// </param>
    /// <param name="worldPosition">
    /// The world-space position to convert.
    /// </param>
    /// <returns>
    /// A team-relative position where X is depth from zero at the defending
    /// goal to one at the attacking goal, and Y is lateral position from
    /// minus one on the team's left to one on the team's right.
    /// </returns>
    Vector2 GetTeamRelativeFieldPosition(
        ETeamId teamId,
        Vector2 worldPosition);

    /// <summary>
    /// Converts normalized team-relative field coordinates into a
    /// world-space position.
    /// </summary>
    /// <param name="teamId">
    /// The team whose perspective should be used.
    /// </param>
    /// <param name="teamRelativePosition">
    /// A position where X is depth from zero to one and Y is lateral
    /// position from minus one to one.
    /// </param>
    /// <returns>
    /// The corresponding world-space position.
    /// </returns>
    Vector2 GetWorldPositionFromTeamRelative(
        ETeamId teamId,
        Vector2 teamRelativePosition);

    /// <summary>
    /// Converts a formation position into a world-space position for the
    /// specified team.
    /// </summary>
    /// <param name="teamId">
    /// The team using the formation position.
    /// </param>
    /// <param name="formationPosition">
    /// The requested formation position.
    /// </param>
    /// <returns>
    /// The corresponding world-space formation position.
    /// </returns>
    Vector2 GetFormationWorldPosition(
        ETeamId teamId,
        EFormationPosition formationPosition);

    /// <summary>
    /// Returns the goal position attacked by the specified team.
    /// </summary>
    /// <param name="teamId">
    /// The attacking team.
    /// </param>
    /// <returns>
    /// The opposing goal's world-space position.
    /// </returns>
    Vector2 GetAttackingGoalPosition(
        ETeamId teamId);

    /// <summary>
    /// Returns the goal position defended by the specified team.
    /// </summary>
    /// <param name="teamId">
    /// The defending team.
    /// </param>
    /// <returns>
    /// The team's own goal position.
    /// </returns>
    Vector2 GetDefendingGoalPosition(
        ETeamId teamId);

    /// <summary>
    /// Determines whether a world-space position is inside the specified
    /// team's defending penalty box.
    /// </summary>
    /// <param name="teamId">
    /// The team defending the penalty box.
    /// </param>
    /// <param name="worldPosition">
    /// The world-space position being checked.
    /// </param>
    /// <returns>
    /// True when the position is inside the defending penalty box.
    /// </returns>
    bool IsInsideDefendingPenaltyBox(
        ETeamId teamId,
        Vector2 worldPosition);

    /// <summary>
    /// Determines whether a world-space position is inside the goal area
    /// defended by the specified team.
    /// </summary>
    /// <param name="teamId">
    /// The team defending the goal area.
    /// </param>
    /// <param name="worldPosition">
    /// The world-space position being checked.
    /// </param>
    /// <returns>
    /// True when the position is inside the specified goal area.
    /// </returns>
    bool IsInsideGoalArea(
        ETeamId teamId,
        Vector2 worldPosition);
}