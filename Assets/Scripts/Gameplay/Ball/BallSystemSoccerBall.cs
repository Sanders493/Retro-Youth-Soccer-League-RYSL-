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
    public void Kick(Vector2 direction, float power)
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
    }
}
