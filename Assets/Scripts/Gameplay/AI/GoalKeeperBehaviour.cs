using UnityEngine;

/// <summary>
/// Controls goalkeeper positioning and ball interception inside the
/// defending penalty box.
/// </summary>
public sealed class GoalkeeperBehavior : IAIBehavior
{
    private readonly float goalOffset;
    private readonly float horizontalBallInfluence;
    private readonly int priority;

    /// <summary>
    /// Creates a goalkeeper behavior.
    /// </summary>
    /// <param name="goalOffset">
    /// The normalized distance the goalkeeper remains in front of goal.
    /// </param>
    /// <param name="horizontalBallInfluence">
    /// How strongly the goalkeeper shifts horizontally toward the ball.
    /// </param>
    /// <param name="priority">
    /// The priority of goalkeeper behavior.
    /// </param>
    public GoalkeeperBehavior(
        float goalOffset = 0.08f,
        float horizontalBallInfluence = 0.4f,
        int priority = 70)
    {
        this.goalOffset =
            Mathf.Clamp01(goalOffset);

        this.horizontalBallInfluence =
            Mathf.Clamp01(horizontalBallInfluence);

        this.priority = priority;
    }

    /// <summary>
    /// Determines whether goalkeeper behavior applies to the actor.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>
    /// True when the actor is an active AI goalkeeper.
    /// </returns>
    public bool CanExecute(AIBehaviorContext context)
    {
        return context != null
            && context.Actor != null
            && context.GameState != null
            && context.GameState.IsMatchActive
            && context.Actor.IsActive
            && context.Actor.IsAIControlled
            && context.Actor.PlayerRole == EPlayerRole.Goalkeeper;
    }

    /// <summary>
    /// Returns the priority of goalkeeper behavior.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The configured behavior priority.</returns>
    public int GetPriority(AIBehaviorContext context)
    {
        return priority;
    }

    /// <summary>
    /// Creates either an interception assignment or a goal-protection
    /// movement assignment.
    /// </summary>
    /// <param name="context">The actor's current behavior context.</param>
    /// <returns>The resulting goalkeeper assignment.</returns>
    public ActorAssignment CreateAssignment(AIBehaviorContext context)
    {
        if (ShouldInterceptBall(context))
        {
            return new ActorAssignment(
                context.Actor.ActorId,
                EAIActionType.Move,
                context.GameState.BallPosition,
                priority: priority);
        }

        Vector2 targetPosition =
            GetGoalProtectionPosition(context);

        return new ActorAssignment(
            context.Actor.ActorId,
            EAIActionType.Move,
            targetPosition,
            priority: priority);
    }

    /// <summary>
    /// Determines whether the goalkeeper should move directly toward the
    /// ball.
    /// </summary>
    private bool ShouldInterceptBall(
        AIBehaviorContext context)
    {
        if (context.Actor.HasBall)
            return false;

        return context.GameState.IsInsideDefendingPenaltyBox(
            context.Actor.TeamId,
            context.GameState.BallPosition);
    }

    /// <summary>
    /// Calculates a position in front of goal that shifts horizontally
    /// toward the ball.
    /// </summary>
    private Vector2 GetGoalProtectionPosition(
        AIBehaviorContext context)
    {
        Vector2 ballFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                context.GameState.BallPosition);

        float horizontalPosition =
            ballFieldPosition.x * horizontalBallInfluence;

        Vector2 goalkeeperFieldPosition = new Vector2(
            horizontalPosition,
            goalOffset);

        return context.GameState.GetWorldPositionFromTeamRelative(
            context.Actor.TeamId,
            goalkeeperFieldPosition);
    }
}