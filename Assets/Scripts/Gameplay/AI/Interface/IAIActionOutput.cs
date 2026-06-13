using UnityEngine;

/// <summary>
/// Sends AI action requests to external gameplay systems.
/// </summary>
public interface IAIActionOutput
{
    /// <summary>
    /// Requests that an actor move toward a world position.
    /// </summary>
    /// <param name="actorId">The identifier of the actor performing the action.</param>
    /// <param name="targetPosition">The requested movement destination.</param>
    void RequestMove(string actorId, Vector2 targetPosition);

    /// <summary>
    /// Requests that an actor stop moving.
    /// </summary>
    /// <param name="actorId">The identifier of the actor performing the action.</param>
    void RequestStop(string actorId);

    /// <summary>
    /// Requests that an actor pass the ball toward another actor.
    /// </summary>
    /// <param name="actorId">The identifier of the actor making the pass.</param>
    /// <param name="targetActorId">The identifier of the intended receiver.</param>
    void RequestPass(string actorId, string targetActorId);

    /// <summary>
    /// Requests that an actor pass the ball toward a world position.
    /// </summary>
    /// <param name="actorId">The identifier of the actor making the pass.</param>
    /// <param name="targetPosition">The intended pass destination.</param>
    void RequestPass(string actorId, Vector2 targetPosition);

    /// <summary>
    /// Requests that an actor shoot the ball toward a world position.
    /// </summary>
    /// <param name="actorId">The identifier of the actor taking the shot.</param>
    /// <param name="targetPosition">The intended shot destination.</param>
    void RequestShoot(string actorId, Vector2 targetPosition);

    /// <summary>
    /// Requests that an actor attempt to take possession of the ball.
    /// </summary>
    /// <param name="actorId">The identifier of the actor attempting the action.</param>
    void RequestTakeBall(string actorId);
}