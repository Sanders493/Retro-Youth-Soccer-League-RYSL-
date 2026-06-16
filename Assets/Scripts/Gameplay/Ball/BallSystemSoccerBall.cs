using System;
using UnityEngine;

public class SoccerBall : MonoBehaviour
{
    [SerializeField] private float friction = 0.97f;
    [SerializeField] private float minimumSpeed = 0.05f;

    private Rigidbody2D rb;

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

    /// <summary>
    /// Gets required Rigidbody2D component.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Applies friction so the ball slows down over time.
    /// </summary>
    private void FixedUpdate()
    {
        rb.velocity *= friction;

        if (rb.velocity.magnitude < minimumSpeed)
        {
            rb.velocity = Vector2.zero;
        }

        CurrentVelocity = rb.velocity;
    }

    /// <summary>
    /// Kicks the ball in a chosen direction with a chosen power.
    /// </summary>
    /// <param name="direction">The direction the ball should move.</param>
    /// <param name="power">The strength of the kick.</param>
    /// <param name="player">The actor that kicked the ball.</param>
    public void Kick(Vector2 direction, float power, GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("SoccerBall.Kick requires a valid player reference.");
            return;
        }

        CurrentController = null;
        ControlChanged?.Invoke(null);

        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);

        LastKicker = player;
        Kicked?.Invoke(player);
    }

    /// <summary>
    /// Gives a player full control of the ball.
    /// </summary>
    /// <param name="player">The player gaining control.</param>
    public void SetController(GameObject player)
    {
        if (CurrentController == player)
            return;

        CurrentController = player;
        ControlChanged?.Invoke(player);
    }

    /// <summary>
    /// Removes full control of the ball from its current controller.
    /// </summary>
    public void ClearController()
    {
        if (CurrentController == null)
            return;

        CurrentController = null;
        ControlChanged?.Invoke(null);
    }
}