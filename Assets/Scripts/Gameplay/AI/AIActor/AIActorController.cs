/// <summary>
/// Executes assignments for one AI-controlled actor through the gameplay action interface.
/// </summary>
public sealed class AIActorController
{
    private readonly string actorId;
    private readonly IAIActionOutput actionOutput;

    public ActorAssignment CurrentAssignment { get; private set; }

    /// <summary>
    /// Creates a controller for a single AI-controlled actor.
    /// </summary>
    /// <param name="actorId">The unique identifier of the controlled actor.</param>
    /// <param name="actionOutput">
    /// The interface used to send requests to external gameplay systems.
    /// </param>
    public AIActorController(string actorId, IAIActionOutput actionOutput)
    {
        this.actorId = actorId;
        this.actionOutput = actionOutput;
    }

    /// <summary>
    /// Assigns a new action to the controlled actor.
    /// </summary>
    /// <param name="assignment">The assignment to execute.</param>
    public void SetAssignment(ActorAssignment assignment)
    {
        if (assignment == null || assignment.ActorId != actorId)
            return;

        CurrentAssignment = assignment;
    }

    /// <summary>
    /// Executes the actor's current assignment.
    /// </summary>
    public void ExecuteAssignment()
    {
        if (CurrentAssignment == null)
            return;

        switch (CurrentAssignment.ActionType)
        {
            case EAIActionType.HoldPosition:
                actionOutput.RequestStop(actorId);
                break;

            case EAIActionType.Move:
                actionOutput.RequestMove(
                    actorId,
                    CurrentAssignment.TargetPosition);
                break;

            case EAIActionType.Pass:
                ExecutePass();
                break;

            case EAIActionType.Shoot:
                actionOutput.RequestShoot(
                    actorId,
                    CurrentAssignment.TargetPosition);
                break;

            case EAIActionType.TakeBall:
                actionOutput.RequestTakeBall(actorId);
                break;

            default:
                actionOutput.RequestStop(actorId);
                break;
        }
    }

    /// <summary>
    /// Executes the current pass assignment.
    /// </summary>
    private void ExecutePass()
    {
        if (!string.IsNullOrWhiteSpace(CurrentAssignment.TargetActorId))
        {
            actionOutput.RequestPass(
                actorId,
                CurrentAssignment.TargetActorId);

            return;
        }

        actionOutput.RequestPass(
            actorId,
            CurrentAssignment.TargetPosition);
    }
}