using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        MovementInput = new Vector2(horizontal, vertical).normalized;
    }
}