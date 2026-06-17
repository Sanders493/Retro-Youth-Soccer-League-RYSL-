using System;
using UnityEngine;

/// <summary>
/// Controls soccer-ball possession, movement, kicking, and temporary
/// protection against the passer immediately reclaiming the ball.
/// </summary>
public class SoccerBall : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float friction = 0.97f;
    [SerializeField] private float minimumSpeed = 0.05f;

    [Header("Control")]
    [SerializeField] private float controlDistance = 0.6f;

    [Header("Pass Reclaim Protection")]
    [Tooltip(
        "How long the passer is prevented from immediately reclaiming " +
        "the ball.")]
    [SerializeField] private float passerReclaimDelay = 0.35f;

    [Tooltip(
        "How far the ball must travel before the passer may reclaim it " +
        "early.")]
    [SerializeField] private float minimumPassClearance = 0.75f;

    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private Collider2D controllerCollider;
    private Collider2D blockedPasserCollider;

    private GameObject blockedPasser;
    private float blockedPasserUntil;
    private Vector2 passStartPosition;

    /// <summary>
    /// Gets the ball's current physics velocity.
    /// </summary>
    public Vector2 CurrentVelocity
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
    /// Gets required physics components.
    /// </summary>
    private void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();

        ballCollider =
            GetComponent<Collider2D>();
    }

    /// <summary>
    /// Updates controlled movement, pass protection, and loose-ball
    /// friction.
    /// </summary>
    private void FixedUpdate()
    {
        UpdatePasserReclaimProtection();

        if (CurrentController != null)
        {
            UpdateControlledPosition();
            return;
        }

        rb.linearVelocity *= friction;

        if (rb.linearVelocity.magnitude
            < minimumSpeed)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        CurrentVelocity =
            rb.linearVelocity;
    }

    /// <summary>
    /// Kicks the ball in a chosen direction with a chosen power.
    /// </summary>
    /// <param name="direction">
    /// The direction the ball should move.
    /// </param>
    /// <param name="power">
    /// The strength of the kick.
    /// </param>
    /// <param name="sender">
    /// The actor that kicked the ball.
    /// </param>
    public void Kick(
        Vector2 direction,
        float power,
        GameObject sender)
    {
        if (sender == null)
        {
            Debug.LogError(
                "SoccerBall.Kick requires a valid player reference.");

            return;
        }

        if (direction.sqrMagnitude
            <= Mathf.Epsilon)
        {
            Debug.LogWarning(
                "SoccerBall.Kick received a zero direction.");

            return;
        }

        BeginPasserReclaimProtection(
            sender);

        RestoreControllerCollision();

        CurrentController = null;

        ControlChanged?.Invoke(
            null);

        rb.bodyType =
            RigidbodyType2D.Dynamic;

        rb.linearVelocity =
            Vector2.zero;
        
        rb.AddForce(
            direction.normalized * power,
            ForceMode2D.Impulse);

        CurrentVelocity =
            rb.linearVelocity;

        LastKicker =
            sender;

        Kicked?.Invoke(
            sender);
    }

    /// <summary>
    /// Keeps the controlled ball in front of its current controller.
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

        Vector2 direction =
            controller.FacingDirection;

        if (direction.sqrMagnitude
            <= 0.01f)
        {
            direction =
                Vector2.right;
        }

        Vector2 targetPosition =
            (Vector2)CurrentController.transform.position
            + direction.normalized * controlDistance;

        rb.linearVelocity =
            Vector2.zero;

        rb.MovePosition(
            targetPosition);

        CurrentVelocity =
            Vector2.zero;
    }

    /// <summary>
    /// Gives control of the ball to an actor and disables physical response
    /// between the ball and its controller.
    /// </summary>
    /// <param name="newController">
    /// The actor taking control.
    /// </param>
    public void SetController(
        GameObject newController)
    {
        RestoreControllerCollision();

        CurrentController = newController;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            rb.bodyType =
                RigidbodyType2D.Kinematic;
        }

        controllerCollider =
            newController != null
                ? newController.GetComponent<Collider2D>()
                : null;

        if (ballCollider != null
            && controllerCollider != null)
        {
            Physics2D.IgnoreCollision(
                ballCollider,
                controllerCollider,
                true);
        }
    }

    /// <summary>
    /// Checks whether a player is currently permitted to control the ball.
    /// </summary>
    /// <param name="player">
    /// The player attempting to gain control.
    /// </param>
    /// <returns>
    /// True when the player is permitted to control the ball.
    /// </returns>
    private bool CanTakeControl(
        GameObject player)
    {
        if (player == null)
            return false;

        if (player != blockedPasser)
            return true;

        bool delayFinished =
            Time.time >= blockedPasserUntil;

        bool clearanceReached =
            Vector2.Distance(
                transform.position,
                passStartPosition)
            >= minimumPassClearance;

        if (!delayFinished
            && !clearanceReached)
        {
            return false;
        }

        ClearPasserReclaimProtection();

        return true;
    }

    /// <summary>
    /// Prevents the passer from immediately reclaiming the released ball.
    /// </summary>
    /// <param name="passer">
    /// The actor releasing the ball.
    /// </param>
    private void BeginPasserReclaimProtection(
        GameObject passer)
    {
        ClearPasserReclaimProtection();

        blockedPasser =
            passer;

        blockedPasserUntil =
            Time.time + passerReclaimDelay;

        passStartPosition =
            transform.position;

        blockedPasserCollider =
            passer.GetComponent<Collider2D>();

        if (ballCollider != null
            && blockedPasserCollider != null)
        {
            Physics2D.IgnoreCollision(
                ballCollider,
                blockedPasserCollider,
                true);
        }
    }

    /// <summary>
    /// Clears protection once the timeout or minimum clearance is reached.
    /// </summary>
    private void UpdatePasserReclaimProtection()
    {
        if (blockedPasser == null)
            return;

        bool delayFinished =
            Time.time >= blockedPasserUntil;

        bool clearanceReached =
            Vector2.Distance(
                transform.position,
                passStartPosition)
            >= minimumPassClearance;

        if (delayFinished
            || clearanceReached)
        {
            ClearPasserReclaimProtection();
        }
    }

    /// <summary>
    /// Restores collision and clears temporary passer protection.
    /// </summary>
    private void ClearPasserReclaimProtection()
    {
        if (ballCollider != null
            && blockedPasserCollider != null)
        {
            Physics2D.IgnoreCollision(
                ballCollider,
                blockedPasserCollider,
                false);
        }

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
    /// Restores collision between the ball and its previous controller.
    /// </summary>
    private void RestoreControllerCollision()
    {
        if (ballCollider != null
            && controllerCollider != null)
        {
            Physics2D.IgnoreCollision(
                ballCollider,
                controllerCollider,
                false);
        }

        controllerCollider =
            null;
    }

    /// <summary>
    /// Removes full control of the ball from its current controller.
    /// </summary>
    public void ClearController()
    {
        if (CurrentController == null)
            return;

        RestoreControllerCollision();

        CurrentController =
            null;

        if (rb != null)
        {
            rb.bodyType =
                RigidbodyType2D.Dynamic;
        }

        ControlChanged?.Invoke(
            null);
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

        passerReclaimDelay =
            Mathf.Max(
                0f,
                passerReclaimDelay);

        minimumPassClearance =
            Mathf.Max(
                0f,
                minimumPassClearance);
    }
#endif
}