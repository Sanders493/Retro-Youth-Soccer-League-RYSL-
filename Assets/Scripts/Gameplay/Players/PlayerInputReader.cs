using UnityEngine;

/// <summary>
/// Reads the input from the keyboard and creates a vector according to it
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }

    /// <summary>
    /// Update the MovementInput vector everytime the user presses a movement key
    /// </summary>
    public void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        MovementInput = new Vector2(horizontal, vertical).normalized;
    }
}