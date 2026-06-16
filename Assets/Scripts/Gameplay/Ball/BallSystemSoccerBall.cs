using System;
using UnityEngine;

public class SoccerBall : MonoBehaviour
{
    [SerializeField] private float friction = 0.97f;
    [SerializeField] private float minimumSpeed = 0.05f;

    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private Collider2D controllerCollider;

    public Vector2 CurrentVelocity
    {
        get; private set;
    }

    public event Action<GameObject> Kicked;
    public event Action<GameObject> ControlChanged;

    public GameObject LastKicker
    {
        get; private set;
    }

    public GameObject CurrentController
    {
        get; private set;
    }
    [SerializeField] private float controlDistance = 0.6f;

    /// <summary>
    /// Gets required physics components.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Applies friction so the ball slows down over time.
    /// </summary>

    private void FixedUpdate()
    {
        if (CurrentController != null)
        {
            UpdateControlledPosition();
            return;
        }

        rb.linearVelocity *= friction;

        if (rb.linearVelocity.magnitude < minimumSpeed)
            rb.linearVelocity = Vector2.zero;

        CurrentVelocity = rb.linearVelocity;
    }
    /// <summary>
    /// Kicks the ball in a chosen direction with a chosen power.
    /// </summary>
    /// <param name="direction">The direction the ball should move.</param>
    /// <param name="power">The strength of the kick.</param>
    /// <param name="sender">The actor that kicked the ball.</param>
    public void Kick(Vector2 direction, float power, GameObject sender) 
    {
        if (sender == null)
        {
            Debug.LogError(
                "SoccerBall.Kick requires a valid player reference.");

            return;
        }

        RestoreControllerCollision();

        CurrentController = null;
        ControlChanged?.Invoke(null);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(
            direction.normalized * power,
            ForceMode2D.Impulse);

        LastKicker = sender;
        Kicked?.Invoke(sender);
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

        if (direction.sqrMagnitude <= 0.01f)
            direction = Vector2.right;

        Vector2 targetPosition =
            (Vector2)CurrentController.transform.position
            + direction.normalized * controlDistance;

        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(targetPosition);

        CurrentVelocity = Vector2.zero;
    }
    /// <summary>
    /// Gives a player full control of the ball.
    /// </summary>
    /// <param name="player">The player gaining control.</param>
    public void SetController(GameObject player)
    {
        if (player == null || CurrentController == player)
            return;

        RestoreControllerCollision();

        rb.linearVelocity = Vector2.zero;
        CurrentVelocity = Vector2.zero;

        CurrentController = player;
        controllerCollider =
            player.GetComponent<Collider2D>();

        if (ballCollider != null
            && controllerCollider != null)
        {
            Physics2D.IgnoreCollision(
                ballCollider,
                controllerCollider,
                true);
        }

        ControlChanged?.Invoke(player);
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

        controllerCollider = null;
    }
    /// <summary>
    /// Removes full control of the ball from its current controller.
    /// </summary>
    public void ClearController()
    {
        if (CurrentController == null)
            return;

        RestoreControllerCollision();

        CurrentController = null;
        ControlChanged?.Invoke(null);
    }
}


