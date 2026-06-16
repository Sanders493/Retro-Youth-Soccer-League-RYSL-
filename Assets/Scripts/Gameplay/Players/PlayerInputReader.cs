using UnityEngine;

/// <summary>
/// Reads player input without directly performing gameplay actions.
/// </summary>
public sealed class PlayerInputReader : MonoBehaviour
{
    [Header("Movement Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    [Header("Action Keys")]
    [SerializeField] private KeyCode passKey = KeyCode.S;
    [SerializeField] private KeyCode shootKey = KeyCode.D;
    [SerializeField] private KeyCode takeBallKey = KeyCode.D;

    public Vector2 MovementInput { get; private set; }
    public bool PassPressed { get; private set; }
    public bool ShootPressed { get; private set; }
    public bool TakeBallPressed { get; private set; }

    public KeyCode PassKey => passKey;
    public KeyCode ShootKey => shootKey;
    public KeyCode TakeBallKey => takeBallKey;

    /// <summary>
    /// Reads the current input state.
    /// </summary>
    private void Update()
    {
        float horizontal =
            Input.GetAxisRaw(horizontalAxis);

        float vertical =
            Input.GetAxisRaw(verticalAxis);

        MovementInput = new Vector2(
            horizontal,
            vertical).normalized;

        PassPressed =
            Input.GetKeyDown(passKey);

        ShootPressed =
            Input.GetKeyDown(shootKey);

        TakeBallPressed =
            Input.GetKeyDown(takeBallKey);
    }
}