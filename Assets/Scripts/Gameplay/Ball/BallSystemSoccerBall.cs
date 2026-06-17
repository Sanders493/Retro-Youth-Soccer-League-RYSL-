using System;
using UnityEngine;

/// <summary>
/// Controls soccer-ball possession, controlled positioning, loose-ball
/// movement, kicking, and temporary possession protection.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class SoccerBall : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Range(0f, 1f)]
    private float friction = 0.97f;

    [SerializeField]
    private float minimumSpeed = 0.05f;

    [Header("Controlled Ball")]
    [Tooltip(
        "The distance the controlled ball remains in front of its owner.")]
    [SerializeField]
    private float controlDistance = 0.6f;

    [Header("New Possession Protection")]
    [Tooltip(
        "How long a new controller is protected from being immediately " +
        "dispossessed.")]
    [SerializeField]
    private float controllerProtectionDuration = 0.35f;

    [Header("Post-Kick Protection")]
    [Tooltip(
        "How long nobody may collect the ball immediately after a kick.")]
    [SerializeField]
    private float postKickPickupDelay = 0.1f;

    [Tooltip(
        "How long the kicker is prevented from reclaiming the ball.")]
    [SerializeField]
    private float passerReclaimDelay = 0.35f;

    [Tooltip(
        "How far the ball must travel before the kicker may reclaim it.")]
    [SerializeField]
    private float minimumPassClearance = 0.75f;

    [Header("Post-Steal Protection")]
    [Tooltip(
        "How long a dispossessed actor is prevented from immediately " +
        "taking the ball back.")]
    [SerializeField]
    private float stolenBallReclaimDelay = 0.5f;

    [Tooltip(
        "How far the ball must move after a steal before the dispossessed " +
        "actor may reclaim it.")]
    [SerializeField]
    private float minimumStealClearance = 0.5f;

    [Header("Debug")]
    [SerializeField]
    private bool logPossessionChanges;

    [SerializeField]
    private bool logRejectedControlRequests;

    private Rigidbody2D ballRigidbody;
    private Collider2D ballCollider;
    private Collider2D controllerCollider;

    private float controllerProtectedUntil;
    private float pickupBlockedUntil;

    private GameObject blockedPasser;
    private Collider2D blockedPasserCollider;
    private float blockedPasserUntil;
    private Vector2 passStartPosition;

    private GameObject blockedStolenFromActor;
    private Collider2D blockedStolenFromCollider;
    private float blockedStolenFromUntil;
    private Vector2 stealStartPosition;

    /// <summary>
    /// Gets the ball's current physics velocity.
    /// </summary>
    public Vector2 CurrentVelocity
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the actor that most recently kicked the ball.
    /// </summary>
    public GameObject LastKicker
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the actor currently controlling the ball.
    /// </summary>
    public GameObject CurrentController
    {
        get;
        private set;
    }

    /// <summary>
    /// Invoked when the ball is kicked.
    /// </summary>
    public event Action<GameObject> Kicked;

    /// <summary>
    /// Invoked when full control of the ball changes.
    /// </summary>
    public event Action<GameObject> ControlChanged;

    /// <summary>
    /// Retrieves required physics components.
    /// </summary>
    private void Awake()
    {
        ballRigidbody =
            GetComponent<Rigidbody2D>();

        ballCollider =
            GetComponent<Collider2D>();
    }

    /// <summary>
    /// Updates protection states, controlled positioning, and loose-ball
    /// friction.
    /// </summary>
    private void FixedUpdate()
    {
        UpdateTemporaryProtection();

        if (CurrentController != null)
        {
            UpdateControlledPosition();
            return;
        }

        UpdateLooseBallMovement();
    }

    /// <summary>
    /// Gives control of the ball to an actor when the actor is eligible.
    /// </summary>
    /// <param name="newController">
    /// The actor attempting to control the ball.
    /// </param>
    /// <returns>
    /// True when control was successfully assigned.
    /// </returns>
    public bool SetController(
        GameObject newController)
    {
        if (!CanTakeControl(
                newController,
                out string rejectionReason))
        {
            LogRejectedControlRequest(
                newController,
                rejectionReason);

            return false;
        }

        if (CurrentController == newController)
            return true;

        GameObject previousController =
            CurrentController;

        bool isSteal =
            previousController != null
            && previousController != newController;

        RestoreControllerCollision();

        if (isSteal)
        {
            BeginStolenBallProtection(
                previousController);
        }

        CurrentController =
            newController;

        controllerProtectedUntil =
            Time.time
            + controllerProtectionDuration;

        SetControlledPhysicsState();

        controllerCollider =
            GetActorCollider(
                newController);

        SetCollisionIgnored(
            controllerCollider,
            true);

        NotifyControlChanged(
            newController,
            isSteal
                ? $"stolen from {previousController.name}"
                : "control acquired");

        return true;
    }

    /// <summary>
    /// Removes control from the current actor and returns the ball to loose
    /// physics.
    /// </summary>
    public void ClearController()
    {
        if (CurrentController == null)
            return;

        GameObject previousController =
            CurrentController;

        RestoreControllerCollision();

        CurrentController =
            null;

        controllerProtectedUntil =
            0f;

        SetLoosePhysicsState();

        NotifyControlChanged(
            null,
            $"released by {previousController.name}");
    }

    /// <summary>
    /// Kicks the ball in a chosen direction using the supplied impulse.
    /// </summary>
    /// <param name="direction">
    /// The direction the ball should travel.
    /// </param>
    /// <param name="power">
    /// The impulse applied to the ball.
    /// </param>
    /// <param name="sender">
    /// The actor performing the kick.
    /// </param>
    public void Kick(
        Vector2 direction,
        float power,
        GameObject sender)
    {
        if (sender == null)
        {
            Debug.LogError(
                $"{name}: Kick requires a valid sender.",
                this);

            return;
        }

        if (CurrentController != sender)
        {
            Debug.LogWarning(
                $"{name}: {sender.name} attempted to kick but does not " +
                "control the ball.",
                this);

            return;
        }

        if (direction.sqrMagnitude
            <= Mathf.Epsilon)
        {
            Debug.LogWarning(
                $"{name}: Kick received a zero direction.",
                this);

            return;
        }

        GameObject previousController =
            CurrentController;

        RestoreControllerCollision();

        BeginPasserReclaimProtection(
            sender);

        CurrentController =
            null;

        controllerProtectedUntil =
            0f;

        pickupBlockedUntil =
            Time.time
            + postKickPickupDelay;

        SetLoosePhysicsState();

        ballRigidbody.linearVelocity =
            Vector2.zero;

        ballRigidbody.angularVelocity =
            0f;

        ballRigidbody.AddForce(
            direction.normalized
            * Mathf.Max(0f, power),
            ForceMode2D.Impulse);

        CurrentVelocity =
            ballRigidbody.linearVelocity;

        LastKicker =
            sender;

        NotifyControlChanged(
            null,
            $"kicked by {previousController.name}");

        if (logPossessionChanges)
        {
            Debug.Log(
                $"{name}: Kick sender={sender.name}, " +
                $"direction={direction.normalized}, " +
                $"power={power:F2}, " +
                $"velocity={CurrentVelocity}.",
                this);
        }

        Kicked?.Invoke(
            sender);
    }

    /// <summary>
    /// Checks whether an actor is temporarily blocked from taking control.
    /// </summary>
    /// <param name="actor">
    /// The actor being checked.
    /// </param>
    /// <returns>
    /// True when a temporary possession rule currently blocks the actor.
    /// </returns>
    public bool IsControlBlockedFor(
        GameObject actor)
    {
        if (actor == null)
            return false;

        if (CurrentController == null
            && Time.time < pickupBlockedUntil)
        {
            return true;
        }

        if (CurrentController != null
            && CurrentController != actor
            && Time.time < controllerProtectedUntil)
        {
            return true;
        }

        if (actor == blockedPasser
            && !HasProtectionExpired(
                blockedPasserUntil,
                passStartPosition,
                minimumPassClearance))
        {
            return true;
        }

        if (actor == blockedStolenFromActor
            && !HasProtectionExpired(
                blockedStolenFromUntil,
                stealStartPosition,
                minimumStealClearance))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether an actor may currently take control.
    /// </summary>
    private bool CanTakeControl(
        GameObject actor,
        out string rejectionReason)
    {
        rejectionReason =
            string.Empty;

        if (actor == null)
        {
            rejectionReason =
                "actor is null";

            return false;
        }

        if (CurrentController == actor)
            return true;

        if (CurrentController == null
            && Time.time < pickupBlockedUntil)
        {
            rejectionReason =
                "global post-kick pickup delay is active";

            return false;
        }

        if (CurrentController != null
            && Time.time < controllerProtectedUntil)
        {
            rejectionReason =
                $"current controller {CurrentController.name} is protected";

            return false;
        }

        if (actor == blockedPasser)
        {
            if (!HasProtectionExpired(
                    blockedPasserUntil,
                    passStartPosition,
                    minimumPassClearance))
            {
                rejectionReason =
                    "passer reclaim protection is active";

                return false;
            }

            ClearPasserReclaimProtection();
        }

        if (actor == blockedStolenFromActor)
        {
            if (!HasProtectionExpired(
                    blockedStolenFromUntil,
                    stealStartPosition,
                    minimumStealClearance))
            {
                rejectionReason =
                    "post-steal reclaim protection is active";

                return false;
            }

            ClearStolenBallProtection();
        }

        return true;
    }

    /// <summary>
    /// Keeps a controlled ball in front of its current controller.
    /// </summary>
    private void UpdateControlledPosition()
    {
        PlayerActor controller =
            CurrentController.GetComponent<PlayerActor>();

        if (controller == null)
        {
            ClearController();
            return;
        }

        Vector2 facingDirection =
            controller.FacingDirection;

        if (facingDirection.sqrMagnitude
            <= 0.01f)
        {
            facingDirection =
                Vector2.right;
        }

        Vector2 targetPosition =
            (Vector2)CurrentController.transform.position
            + facingDirection.normalized
            * controlDistance;

        ballRigidbody.linearVelocity =
            Vector2.zero;

        ballRigidbody.angularVelocity =
            0f;

        ballRigidbody.MovePosition(
            targetPosition);

        CurrentVelocity =
            Vector2.zero;
    }

    /// <summary>
    /// Applies friction and stops a sufficiently slow loose ball.
    /// </summary>
    private void UpdateLooseBallMovement()
    {
        ballRigidbody.linearVelocity *=
            friction;

        if (ballRigidbody.linearVelocity.magnitude
            < minimumSpeed)
        {
            ballRigidbody.linearVelocity =
                Vector2.zero;
        }

        CurrentVelocity =
            ballRigidbody.linearVelocity;
    }

    /// <summary>
    /// Updates temporary passer and stolen-ball protection.
    /// </summary>
    private void UpdateTemporaryProtection()
    {
        if (blockedPasser != null
            && HasProtectionExpired(
                blockedPasserUntil,
                passStartPosition,
                minimumPassClearance))
        {
            ClearPasserReclaimProtection();
        }

        if (blockedStolenFromActor != null
            && HasProtectionExpired(
                blockedStolenFromUntil,
                stealStartPosition,
                minimumStealClearance))
        {
            ClearStolenBallProtection();
        }
    }

    /// <summary>
    /// Checks whether a protection delay and movement requirement have both
    /// been completed.
    /// </summary>
    private bool HasProtectionExpired(
        float protectedUntil,
        Vector2 startPosition,
        float requiredClearance)
    {
        bool delayFinished =
            Time.time >= protectedUntil;

        bool clearanceReached =
            Vector2.Distance(
                transform.position,
                startPosition)
            >= requiredClearance;

        return delayFinished
            && clearanceReached;
    }

    /// <summary>
    /// Begins temporary protection against the kicker reclaiming the ball.
    /// </summary>
    private void BeginPasserReclaimProtection(
        GameObject passer)
    {
        ClearPasserReclaimProtection();

        blockedPasser =
            passer;

        blockedPasserUntil =
            Time.time
            + passerReclaimDelay;

        passStartPosition =
            transform.position;

        blockedPasserCollider =
            GetActorCollider(
                passer);

        SetCollisionIgnored(
            blockedPasserCollider,
            true);
    }

    /// <summary>
    /// Begins temporary protection against the dispossessed actor taking the
    /// ball back.
    /// </summary>
    private void BeginStolenBallProtection(
        GameObject dispossessedActor)
    {
        ClearStolenBallProtection();

        blockedStolenFromActor =
            dispossessedActor;

        blockedStolenFromUntil =
            Time.time
            + stolenBallReclaimDelay;

        stealStartPosition =
            transform.position;

        blockedStolenFromCollider =
            GetActorCollider(
                dispossessedActor);

        SetCollisionIgnored(
            blockedStolenFromCollider,
            true);
    }

    /// <summary>
    /// Clears temporary passer protection and restores collision.
    /// </summary>
    private void ClearPasserReclaimProtection()
    {
        SetCollisionIgnored(
            blockedPasserCollider,
            false);

        blockedPasser =
            null;

        blockedPasserCollider =
            null;

        blockedPasserUntil =
            0f;

        passStartPosition =
            Vector2.zero;
    }

    /// <summary>
    /// Clears temporary post-steal protection and restores collision.
    /// </summary>
    private void ClearStolenBallProtection()
    {
        SetCollisionIgnored(
            blockedStolenFromCollider,
            false);

        blockedStolenFromActor =
            null;

        blockedStolenFromCollider =
            null;

        blockedStolenFromUntil =
            0f;

        stealStartPosition =
            Vector2.zero;
    }

    /// <summary>
    /// Restores collision between the ball and the previous controller.
    /// </summary>
    private void RestoreControllerCollision()
    {
        SetCollisionIgnored(
            controllerCollider,
            false);

        controllerCollider =
            null;
    }

    /// <summary>
    /// Ignores or restores collision between the ball and another collider.
    /// </summary>
    private void SetCollisionIgnored(
        Collider2D otherCollider,
        bool ignore)
    {
        if (ballCollider == null
            || otherCollider == null)
        {
            return;
        }

        Physics2D.IgnoreCollision(
            ballCollider,
            otherCollider,
            ignore);
    }

    /// <summary>
    /// Gets the primary collider attached to an actor.
    /// </summary>
    private Collider2D GetActorCollider(
        GameObject actor)
    {
        return actor != null
            ? actor.GetComponent<Collider2D>()
            : null;
    }

    /// <summary>
    /// Configures the ball for controlled movement.
    /// </summary>
    private void SetControlledPhysicsState()
    {
        ballRigidbody.linearVelocity =
            Vector2.zero;

        ballRigidbody.angularVelocity =
            0f;

        ballRigidbody.bodyType =
            RigidbodyType2D.Kinematic;

        CurrentVelocity =
            Vector2.zero;
    }

    /// <summary>
    /// Configures the ball for normal loose-ball physics.
    /// </summary>
    private void SetLoosePhysicsState()
    {
        ballRigidbody.bodyType =
            RigidbodyType2D.Dynamic;
    }

    /// <summary>
    /// Invokes the control-change event and optionally logs the change.
    /// </summary>
    private void NotifyControlChanged(
        GameObject newController,
        string reason)
    {
        if (logPossessionChanges)
        {
            Debug.Log(
                $"{name}: Controller=" +
                $"{newController?.name ?? "None"}, " +
                $"reason={reason}.",
                this);
        }

        ControlChanged?.Invoke(
            newController);
    }

    /// <summary>
    /// Logs a rejected possession request when debugging is enabled.
    /// </summary>
    private void LogRejectedControlRequest(
        GameObject actor,
        string reason)
    {
        if (!logRejectedControlRequests)
            return;

        Debug.Log(
            $"{name}: Control request rejected. " +
            $"Actor={actor?.name ?? "None"}, " +
            $"CurrentController=" +
            $"{CurrentController?.name ?? "None"}, " +
            $"Reason={reason}.",
            this);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Restricts designer-configured values to valid ranges.
    /// </summary>
    private void OnValidate()
    {
        friction =
            Mathf.Clamp01(
                friction);

        minimumSpeed =
            Mathf.Max(
                0f,
                minimumSpeed);

        controlDistance =
            Mathf.Max(
                0f,
                controlDistance);

        controllerProtectionDuration =
            Mathf.Max(
                0f,
                controllerProtectionDuration);

        postKickPickupDelay =
            Mathf.Max(
                0f,
                postKickPickupDelay);

        passerReclaimDelay =
            Mathf.Max(
                0f,
                passerReclaimDelay);

        minimumPassClearance =
            Mathf.Max(
                0f,
                minimumPassClearance);

        stolenBallReclaimDelay =
            Mathf.Max(
                0f,
                stolenBallReclaimDelay);

        minimumStealClearance =
            Mathf.Max(
                0f,
                minimumStealClearance);
    }
#endif
}