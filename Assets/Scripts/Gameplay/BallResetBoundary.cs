using UnityEngine;

/// <summary>
/// Resets the soccer ball if it leaves the playable field area.
/// </summary>
public class BallResetBoundary : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private Transform resetPoint;

    [SerializeField] private float minimumX = -8f;
    [SerializeField] private float maximumX = 8f;
    [SerializeField] private float minimumY = -4.5f;
    [SerializeField] private float maximumY = 4.5f;

    private Rigidbody2D ballRigidbody;

    /// <summary>
    /// Gets the ball Rigidbody2D component.
    /// </summary>
    private void Awake()
    {
        if (ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Checks if the ball has left the field and resets it if needed.
    /// </summary>
    private void Update()
    {
        if (ball == null || resetPoint == null) return;

        if (IsBallOutOfBounds())
        {
            ResetBall();
        }
    }

    /// <summary>
    /// Checks whether the ball is outside the field boundaries.
    /// </summary>
    /// <returns>True if the ball is outside the field area.</returns>
    private bool IsBallOutOfBounds()
    {
        Vector3 ballPosition = ball.position;

        return ballPosition.x < minimumX ||
               ballPosition.x > maximumX ||
               ballPosition.y < minimumY ||
               ballPosition.y > maximumY;
    }

    /// <summary>
    /// Moves the ball back to the reset point and stops its movement.
    /// </summary>
    private void ResetBall()
    {
        ball.position = resetPoint.position;

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.angularVelocity = 0f;
        }
    }
}
