using UnityEngine;

/// <summary>
/// Executes assignments for one AI-controlled actor.
/// </summary>
public sealed class AIActorController : MonoBehaviour
{
    private string actorId;
    private IAIActionOutput actionOutput;

    public ActorAssignment CurrentAssignment { get; private set; }

    /// <summary>
    /// Configures this controller for an actor.
    /// </summary>
    /// <param name="actor">The actor controlled by this component.</param>
    public void Initialize(IAIActor actor)
    {
        if (actor == null)
        {
            Debug.LogError(
                $"{name}: Cannot initialize with a null actor.",
                this);

            return;
        }

        actorId = actor.ActorId;
        actionOutput = actor.ActionOutput;

        if (actionOutput == null)
        {
            Debug.LogError(
                $"{name}: Actor does not provide an IAIActionOutput.",
                this);
        }
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
        if (CurrentAssignment == null || actionOutput == null)
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
        if (!string.IsNullOrWhiteSpace(
                CurrentAssignment.TargetActorId))
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