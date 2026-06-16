using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Executes assignments for one AI-controlled actor.
/// </summary>
public sealed class AIActorController : MonoBehaviour
{
    [Header("Debug Gizmos")]
    [SerializeField] private bool showAIDebugGizmos = true;
    [SerializeField] private float actorGizmoRadius = 0.3f;
    [SerializeField] private float targetGizmoRadius = 0.25f;
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 0.6f, 0f);

    [SerializeField] private string actorId;
    [SerializeField] private IAIActionOutput actionOutput; 
    [SerializeField] private GameState gameState;
    public ActorAssignment CurrentAssignment { get; private set; }

    /// <summary>
    /// Configures this controller for an actor.
    /// </summary>
    /// <param name="actor">The actor controlled by this component.</param>
    /// <param name="state">The current match game state.</param>
    public void Initialize(
        IAIActor actor,
        GameState state)
    {
        if (actor == null)
        {
            Debug.LogError(
                $"{name}: Cannot initialize with a null actor.",
                this);

            return;
        }

        if (state == null)
        {
            Debug.LogError(
                $"{name}: Cannot initialize with a null game state.",
                this);

            return;
        }

        actorId = actor.ActorId;
        actionOutput = actor.ActionOutput;
        gameState = state;

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
        if (gameState == null)
        {
            Debug.LogError(
                $"{name}: Cannot execute pass without a game state.",
                this);

            return;
        }

        if (!string.IsNullOrWhiteSpace(
                CurrentAssignment.TargetActorId))
        {
            Vector2 targetPosition =
                CurrentAssignment.TargetPosition;

            gameState.BeginPass(
                CurrentAssignment.TargetActorId,
                targetPosition);

            actionOutput.RequestPass(
                actorId,
                CurrentAssignment.TargetActorId);

            return;
        }

        gameState.BeginPass(
            CurrentAssignment.TargetPosition);

        actionOutput.RequestPass(
            actorId,
            CurrentAssignment.TargetPosition);
    }

    /// <summary>
    /// Draws debug information for the actor's current AI assignment.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showAIDebugGizmos)
            return;

        Vector3 actorPosition = transform.position;

        if (CurrentAssignment == null)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(actorPosition, actorGizmoRadius);

            DrawDebugLabel(actorPosition, "AI: No Assignment");
            return;
        }

        Color actionColor = GetActionColor(
            CurrentAssignment.ActionType);

        Gizmos.color = actionColor;
        Gizmos.DrawWireSphere(actorPosition, actorGizmoRadius);

        string label = $"AI: {CurrentAssignment.ActionType}";

        switch (CurrentAssignment.ActionType)
        {
            case EAIActionType.Move:
            case EAIActionType.Shoot:
                DrawTargetGizmo(
                    actorPosition,
                    CurrentAssignment.TargetPosition,
                    actionColor);
                break;

            case EAIActionType.Pass:
                if (!string.IsNullOrWhiteSpace(
                        CurrentAssignment.TargetActorId))
                {
                    label +=
                        $"\nTarget: {CurrentAssignment.TargetActorId}";
                }
                else
                {
                    DrawTargetGizmo(
                        actorPosition,
                        CurrentAssignment.TargetPosition,
                        actionColor);
                }

                break;

            case EAIActionType.HoldPosition:
                Gizmos.DrawWireCube(
                    actorPosition,
                    Vector3.one * actorGizmoRadius * 1.5f);
                break;

            case EAIActionType.TakeBall:
                Gizmos.DrawWireSphere(
                    actorPosition,
                    actorGizmoRadius * 1.75f);
                break;
        }

        DrawDebugLabel(actorPosition, label);
    }

    /// <summary>
    /// Draws a line and marker from the actor to an assignment target.
    /// </summary>
    /// <param name="actorPosition">The actor's current world position.</param>
    /// <param name="targetPosition">The assignment's target position.</param>
    /// <param name="color">The color used for the target gizmo.</param>
    private void DrawTargetGizmo(
        Vector3 actorPosition,
        Vector3 targetPosition,
        Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(actorPosition, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, targetGizmoRadius);
    }

    /// <summary>
    /// Returns the debug color associated with an AI action.
    /// </summary>
    /// <param name="actionType">The action whose color is requested.</param>
    /// <returns>The color used to visualize the action.</returns>
    private Color GetActionColor(EAIActionType actionType)
    {
        switch (actionType)
        {
            case EAIActionType.HoldPosition:
                return Color.white;

            case EAIActionType.Move:
                return Color.cyan;

            case EAIActionType.Pass:
                return Color.yellow;

            case EAIActionType.Shoot:
                return Color.red;

            case EAIActionType.TakeBall:
                return Color.magenta;

            default:
                return Color.gray;
        }
    }

    /// <summary>
    /// Draws an editor label describing the current AI action.
    /// </summary>
    /// <param name="actorPosition">The actor's current world position.</param>
    /// <param name="label">The text displayed above the actor.</param>
    private void DrawDebugLabel(
        Vector3 actorPosition,
        string label)
    {
#if UNITY_EDITOR
        Handles.Label(actorPosition + labelOffset, label);
#endif
    }
}