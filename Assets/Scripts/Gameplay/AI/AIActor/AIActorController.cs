using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Executes behavior-tree assignments for one AI-controlled actor and
/// provides runtime debugging for its current action.
/// </summary>
public sealed class AIActorController :
    MonoBehaviour
{
    [Header("Debug Logging")]
    [SerializeField]
    private bool logAssignments;

    [SerializeField]
    private bool logExecutedActions;

    [SerializeField]
    private bool logRejectedActions;

    [Header("Debug Gizmos")]
    [SerializeField]
    private bool showAIDebugGizmos = true;

    [SerializeField]
    private float actorGizmoRadius = 0.3f;

    [SerializeField]
    private float targetGizmoRadius = 0.25f;

    [SerializeField]
    private Vector3 labelOffset =
        new Vector3(0f, 0.6f, 0f);

    private string actorId;
    private IAIActionOutput actionOutput;
    private GameState gameState;

    /// <summary>
    /// Gets the actor's currently assigned action.
    /// </summary>
    public ActorAssignment CurrentAssignment
    {
        get;
        private set;
    }

    /// <summary>
    /// Configures this controller for an AI actor.
    /// </summary>
    /// <param name="actor">
    /// The actor controlled by this component.
    /// </param>
    /// <param name="state">
    /// The current match state.
    /// </param>
    public void Initialize(
        IAIActor actor,
        GameState state)
    {
        if (actor == null)
        {
            Debug.LogError(
                $"{name}: Cannot initialize with a null actor.",
                this);

            enabled = false;
            return;
        }

        if (state == null)
        {
            Debug.LogError(
                $"{name}: Cannot initialize with a null GameState.",
                this);

            enabled = false;
            return;
        }

        if (actor.ActionOutput == null)
        {
            Debug.LogError(
                $"{name}: Actor {actor.ActorId} does not provide an " +
                $"IAIActionOutput.",
                this);

            enabled = false;
            return;
        }

        actorId =
            actor.ActorId;

        actionOutput =
            actor.ActionOutput;

        gameState =
            state;

        enabled = true;
    }

    /// <summary>
    /// Assigns a new action to the controlled actor.
    /// </summary>
    /// <param name="assignment">
    /// The assignment to execute.
    /// </param>
    public void SetAssignment(
        ActorAssignment assignment)
    {
        if (assignment == null)
        {
            LogRejectedAction(
                "Set assignment",
                "assignment is null");

            return;
        }

        if (assignment.ActorId != actorId)
        {
            LogRejectedAction(
                "Set assignment",
                $"assignment belongs to {assignment.ActorId}");

            return;
        }

        CurrentAssignment =
            assignment;

        if (logAssignments)
        {
            Debug.Log(
                BuildAssignmentMessage(
                    "Assigned"),
                this);
        }
    }

    /// <summary>
    /// Executes the actor's current assignment.
    /// </summary>
    public void ExecuteAssignment()
    {
        if (CurrentAssignment == null)
            return;

        if (actionOutput == null)
        {
            LogRejectedAction(
                "Execute assignment",
                "action output is null");

            CurrentAssignment = null;
            return;
        }

        switch (CurrentAssignment.ActionType)
        {
            case EAIActionType.HoldPosition:
                ExecuteHoldPosition();
                break;

            case EAIActionType.Move:
                ExecuteMove();
                break;

            case EAIActionType.Pass:
                ExecutePass();
                break;

            case EAIActionType.Shoot:
                ExecuteShoot();
                break;

            case EAIActionType.TakeBall:
                ExecuteTakeBall();
                break;

            case EAIActionType.Clear:
                ExecuteClearance();
                break;
            case EAIActionType.Dive:
                ExecuteDive();
                break;

            case EAIActionType.Throw:
                ExecuteThrow();
                break;

            default:
                LogRejectedAction(
                    "Execute assignment",
                    $"unsupported action type " +
                    $"{CurrentAssignment.ActionType}");

                actionOutput.RequestStop(
                    actorId);

                CurrentAssignment = null;
                break;
        }
    }

    /// <summary>
    /// Stops the actor at its current position.
    /// </summary>
    private void ExecuteHoldPosition()
    {
        actionOutput.RequestStop(
            actorId);

        LogExecutedAction(
            "Hold position requested.");
    }

    /// <summary>
    /// Moves the actor toward the assignment's world-space target.
    /// </summary>
    private void ExecuteMove()
    {
        Vector2 targetPosition =
            CurrentAssignment.TargetPosition;

        actionOutput.RequestMove(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Move requested toward {targetPosition}.");
    }

    /// <summary>
    /// Executes a pass toward the assigned actor or world-space target.
    /// </summary>
    private void ExecutePass()
    {
        ActorAssignment assignment =
            CurrentAssignment;

        if (!TryGetBallOwningActor(
                out IAIActor actor,
                out string rejectionReason))
        {
            RejectAndClear(
                "Pass",
                rejectionReason);

            return;
        }

        Vector2 targetPosition =
            assignment.TargetPosition;

        string targetActorId =
            assignment.TargetActorId;

        if (!string.IsNullOrWhiteSpace(
                targetActorId))
        {
            IAIActor targetActor =
                gameState.GetActor(
                    targetActorId);

            if (targetActor == null)
            {
                RejectAndClear(
                    "Pass",
                    $"target actor {targetActorId} was not found");

                return;
            }

            if (!targetActor.IsActive)
            {
                RejectAndClear(
                    "Pass",
                    $"target actor {targetActorId} is inactive");

                return;
            }

            if (targetActor.TeamId
                != actor.TeamId)
            {
                RejectAndClear(
                    "Pass",
                    $"target actor {targetActorId} is not a teammate");

                return;
            }

            targetPosition =
                targetActor.Position;

            gameState.BeginPass(
                targetActor.ActorId,
                targetPosition);
        }
        else
        {
            gameState.BeginPass(
                targetPosition);
        }

        actionOutput.RequestPass(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Pass requested. " +
            $"Receiver=" +
            $"{targetActorId ?? string.Empty}, " +
            $"Target={targetPosition}.");

        CurrentAssignment = null;
    }

    /// <summary>
    /// Executes a shot toward the assignment's world-space target.
    /// </summary>
    private void ExecuteShoot()
    {
        if (!TryGetBallOwningActor(
                out _,
                out string rejectionReason))
        {
            RejectAndClear(
                "Shot",
                rejectionReason);

            return;
        }

        Vector2 targetPosition =
            CurrentAssignment.TargetPosition;

        actionOutput.RequestShoot(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Shot requested toward {targetPosition}.");

        CurrentAssignment = null;
    }

    /// <summary>
    /// Executes a clearance toward the assignment's world-space target.
    /// </summary>
    private void ExecuteClearance()
    {
        if (!TryGetBallOwningActor(
                out _,
                out string rejectionReason))
        {
            RejectAndClear(
                "Clearance",
                rejectionReason);

            return;
        }

        Vector2 targetPosition =
            CurrentAssignment.TargetPosition;

        actionOutput.RequestClearance(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Clearance requested toward {targetPosition}.");

        CurrentAssignment = null;
    }

    /// <summary>
    /// Attempts to take a loose ball or steal it from an opponent.
    /// </summary>
    private void ExecuteTakeBall()
    {
        if (!TryGetActor(
                out IAIActor actor,
                out string rejectionReason))
        {
            RejectAndClear(
                "Take ball",
                rejectionReason);

            return;
        }

        if (actor.HasBall)
        {
            RejectAndClear(
                "Take ball",
                "actor already owns the ball");

            return;
        }

        IAIActor previousOwner =
            gameState.BallOwner;

        if (previousOwner != null
            && previousOwner.TeamId == actor.TeamId)
        {
            RejectAndClear(
                "Take ball",
                $"teammate {previousOwner.ActorId} owns the ball");

            return;
        }

        if (gameState.IsBallControlBlockedFor(
                actor))
        {
            RejectAndClear(
                "Take ball",
                "temporary ball-control protection blocks this actor");

            return;
        }

        actionOutput.RequestTakeBall(
            actorId);

        IAIActor ownerAfterRequest =
            gameState.BallOwner;

        bool succeeded =
            ownerAfterRequest != null
            && ownerAfterRequest.ActorId == actorId;

        if (succeeded)
        {
            LogExecutedAction(
                $"Take ball succeeded. " +
                $"PreviousOwner=" +
                $"{previousOwner?.ActorId ?? "None"}.");
        }
        else
        {
            LogRejectedAction(
                "Take ball",
                $"request did not transfer control; " +
                $"previous owner=" +
                $"{previousOwner?.ActorId ?? "None"}, " +
                $"current owner=" +
                $"{ownerAfterRequest?.ActorId ?? "None"}");
        }

        CurrentAssignment = null;
    }
    /// <summary>
    /// Requests a goalkeeper dive toward the assigned interception point.
    /// </summary>
    private void ExecuteDive()
    {
        if (!TryGetActor(
                out IAIActor actor,
                out string rejectionReason))
        {
            RejectAndClear(
                "Dive",
                rejectionReason);

            return;
        }

        if (!actor.IsGoalkeeper)
        {
            RejectAndClear(
                "Dive",
                "actor is not a goalkeeper");

            return;
        }

        Vector2 targetPosition =
            CurrentAssignment.TargetPosition;

        actionOutput.RequestDive(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Dive requested toward {targetPosition}.");

        CurrentAssignment =
            null;
    }

    /// <summary>
    /// Requests a goalkeeper throw toward the assigned destination.
    /// </summary>
    private void ExecuteThrow()
    {
        if (!TryGetBallOwningActor(
                out IAIActor actor,
                out string rejectionReason))
        {
            RejectAndClear(
                "Throw",
                rejectionReason);

            return;
        }

        if (!actor.IsGoalkeeper)
        {
            RejectAndClear(
                "Throw",
                "actor is not a goalkeeper");

            return;
        }

        Vector2 targetPosition =
            CurrentAssignment.TargetPosition;

        actionOutput.RequestThrow(
            actorId,
            targetPosition);

        LogExecutedAction(
            $"Throw requested toward {targetPosition}.");

        CurrentAssignment =
            null;
    }

    /// <summary>
    /// Gets the controlled actor from the current game state.
    /// </summary>
    private bool TryGetActor(
        out IAIActor actor,
        out string rejectionReason)
    {
        actor = null;
        rejectionReason = string.Empty;

        if (gameState == null)
        {
            rejectionReason =
                "GameState is null";

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                actorId))
        {
            rejectionReason =
                "actor ID is empty";

            return false;
        }

        actor =
            gameState.GetActor(
                actorId);

        if (actor == null)
        {
            rejectionReason =
                $"actor {actorId} was not found";

            return false;
        }

        if (!actor.IsActive)
        {
            rejectionReason =
                $"actor {actorId} is inactive";

            return false;
        }
        
        if (!actor.IsAIControlled)
        {
            rejectionReason =
                $"actor {actorId} is not AI-controlled";

            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the controlled actor and verifies that it currently owns the
    /// ball.
    /// </summary>
    private bool TryGetBallOwningActor(
        out IAIActor actor,
        out string rejectionReason)
    {
        if (!TryGetActor(
                out actor,
                out rejectionReason))
        {
            return false;
        }

        if (!actor.HasBall)
        {
            rejectionReason =
                "actor reports that it does not have the ball";

            return false;
        }

        IAIActor ballOwner =
            gameState.BallOwner;

        if (ballOwner == null)
        {
            rejectionReason =
                "GameState reports no ball owner";

            return false;
        }

        if (ballOwner.ActorId != actorId)
        {
            rejectionReason =
                $"GameState reports {ballOwner.ActorId} as ball owner";

            return false;
        }

        return true;
    }

    /// <summary>
    /// Rejects an action and clears the current one-shot assignment.
    /// </summary>
    private void RejectAndClear(
        string actionName,
        string reason)
    {
        LogRejectedAction(
            actionName,
            reason);

        CurrentAssignment = null;
    }

    /// <summary>
    /// Clears the actor's current assignment without clearing movement state.
    /// </summary>
    public void ClearAssignment()
    {
        CurrentAssignment =
            null;
    }

    /// <summary>
    /// Builds a standardized assignment debug message.
    /// </summary>
    private string BuildAssignmentMessage(
        string prefix)
    {
        if (CurrentAssignment == null)
        {
            return $"{name}: {prefix}. No assignment.";
        }

        return
            $"{name}: {prefix}. " +
            $"Actor={actorId}, " +
            $"TreeSelection={GetTreeSelectionText()}, " +
            $"Action={CurrentAssignment.ActionType}, " +
            $"TargetActor=" +
            $"{CurrentAssignment.TargetActorId ?? string.Empty}, " +
            $"TargetPosition=" +
            $"{CurrentAssignment.TargetPosition}.";
    }

    /// <summary>
    /// Gets a readable representation of the selector choices that produced
    /// the current assignment.
    /// </summary>
    private string GetTreeSelectionText()
    {
        if (CurrentAssignment == null)
            return "No Selection";

        return CurrentAssignment.GetTreeSelectionText();
    }

    /// <summary>
    /// Logs a requested action when execution debugging is enabled.
    /// </summary>
    private void LogExecutedAction(
        string message)
    {
        if (!logExecutedActions)
            return;

        Debug.Log(
            $"{name}: Actor={actorId}, " +
            $"TreeSelection={GetTreeSelectionText()}, " +
            $"{message}",
            this);
    }

    /// <summary>
    /// Logs an action rejection when rejection debugging is enabled.
    /// </summary>
    private void LogRejectedAction(
        string actionName,
        string reason)
    {
        if (!logRejectedActions)
            return;

        Debug.Log(
            $"{name}: Actor={actorId}, " +
            $"Action={actionName} rejected. " +
            $"TreeSelection={GetTreeSelectionText()}, " +
            $"Reason={reason}.",
            this);
    }

    /// <summary>
    /// Draws Scene-view debug information for the current assignment.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showAIDebugGizmos)
            return;

        Vector3 actorPosition =
            transform.position;

        if (CurrentAssignment == null)
        {
            Gizmos.color =
                Color.gray;

            Gizmos.DrawWireSphere(
                actorPosition,
                actorGizmoRadius);

            DrawDebugLabel(
                actorPosition,
                $"Actor: {GetDisplayedActorId()}\n" +
                "AI: No Assignment");

            return;
        }

        Color actionColor =
            GetActionColor(
                CurrentAssignment.ActionType);

        Gizmos.color =
            actionColor;

        Gizmos.DrawWireSphere(
            actorPosition,
            actorGizmoRadius);

        DrawAssignmentGizmo(
            actorPosition,
            actionColor);

        DrawDebugLabel(
            actorPosition,
            BuildGizmoLabel());
    }

    /// <summary>
    /// Draws the action-specific Scene-view gizmo.
    /// </summary>
    private void DrawAssignmentGizmo(
        Vector3 actorPosition,
        Color actionColor)
    {
        switch (CurrentAssignment.ActionType)
        {
            case EAIActionType.Move:
            case EAIActionType.Pass:
            case EAIActionType.Shoot:
            case EAIActionType.Clear:
                DrawTargetGizmo(
                    actorPosition,
                    CurrentAssignment.TargetPosition,
                    actionColor);
                break;

            case EAIActionType.HoldPosition:
                Gizmos.DrawWireCube(
                    actorPosition,
                    Vector3.one
                    * actorGizmoRadius
                    * 1.5f);
                break;

            case EAIActionType.TakeBall:
                Vector3 ballPosition =
                    gameState != null
                        ? gameState.BallPosition
                        : CurrentAssignment.TargetPosition;

                DrawTargetGizmo(
                    actorPosition,
                    ballPosition,
                    actionColor);

                Gizmos.DrawWireSphere(
                    ballPosition,
                    targetGizmoRadius
                    * 1.5f);
                break;
        }
    }

    /// <summary>
    /// Builds the label displayed above the actor in the Scene view.
    /// </summary>
    private string BuildGizmoLabel()
    {
        string label =
            $"Actor: {GetDisplayedActorId()}" +
            $"\nTree: {GetTreeSelectionText()}" +
            $"\nAction: {CurrentAssignment.ActionType}";

        if (!string.IsNullOrWhiteSpace(
                CurrentAssignment.TargetActorId))
        {
            label +=
                $"\nTarget Actor: " +
                $"{CurrentAssignment.TargetActorId}";
        }

        if (UsesTargetPosition(
                CurrentAssignment.ActionType))
        {
            label +=
                $"\nTarget Position: " +
                $"{CurrentAssignment.TargetPosition}";
        }

        return label;
    }

    /// <summary>
    /// Checks whether an action uses a world-space target.
    /// </summary>
    private bool UsesTargetPosition(
        EAIActionType actionType)
    {
        return actionType == EAIActionType.Move
            || actionType == EAIActionType.Pass
            || actionType == EAIActionType.Shoot
            || actionType == EAIActionType.TakeBall
            || actionType == EAIActionType.Clear;
    }

    /// <summary>
    /// Draws a line and marker from the actor to an assignment target.
    /// </summary>
    private void DrawTargetGizmo(
        Vector3 actorPosition,
        Vector3 targetPosition,
        Color color)
    {
        Gizmos.color =
            color;

        Gizmos.DrawLine(
            actorPosition,
            targetPosition);

        Gizmos.DrawWireSphere(
            targetPosition,
            targetGizmoRadius);
    }

    /// <summary>
    /// Gets the Scene-view color associated with an action.
    /// </summary>
    private Color GetActionColor(
        EAIActionType actionType)
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

            case EAIActionType.Clear:
                return new Color(
                    1f,
                    0.5f,
                    0f);

            default:
                return Color.gray;
        }
    }

    /// <summary>
    /// Gets an actor ID suitable for Scene-view display.
    /// </summary>
    private string GetDisplayedActorId()
    {
        return string.IsNullOrWhiteSpace(
                actorId)
            ? "Uninitialized"
            : actorId;
    }

    /// <summary>
    /// Draws an editor label describing the current AI action.
    /// </summary>
    private void DrawDebugLabel(
        Vector3 actorPosition,
        string label)
    {
#if UNITY_EDITOR
        Handles.Label(
            actorPosition + labelOffset,
            label);
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts debug-gizmo values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        actorGizmoRadius =
            Mathf.Max(
                0.01f,
                actorGizmoRadius);

        targetGizmoRadius =
            Mathf.Max(
                0.01f,
                targetGizmoRadius);
    }
#endif
}