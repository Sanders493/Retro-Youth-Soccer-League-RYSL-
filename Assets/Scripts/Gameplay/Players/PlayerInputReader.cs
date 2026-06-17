using UnityEngine;

/// <summary>
/// Reads the input from the keyboard and creates a vector according to it
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public EActionInputType ActionInput { get; private set; }

    /// <summary>
    /// Reads movement and action inputs each frame and exposes them to gameplay systems
    /// </summary>
    public void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        MovementInput = new Vector2(horizontal, vertical).normalized;

        if (Input.GetKeyDown(KeyCode.D))
            ActionInput = EActionInputType.ShootPressed;
        else if (Input.GetKeyDown(KeyCode.S))
            ActionInput = EActionInputType.PassPressed;
        else
            ActionInput = EActionInputType.None;
    }
}