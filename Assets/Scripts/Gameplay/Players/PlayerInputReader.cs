using UnityEngine;

/// <summary>
/// Reads player input without directly performing gameplay actions.
/// </summary>
public sealed class PlayerInputReader : MonoBehaviour
{
    [Header("Movement Keys")]
    [SerializeField] private KeyCode moveUpKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode moveDownKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode moveLeftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode moveRightKey = KeyCode.RightArrow;

    [Header("Action Keys")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode passKey = KeyCode.S;
    [SerializeField] private KeyCode shootTakeBallKey = KeyCode.D;
    [SerializeField] private KeyCode switchPlayerKey = KeyCode.A;

    public Vector2 MovementInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool PassPressed { get; private set; }
    public bool ShootTakeBallPressed { get; private set; }
    public bool SwitchPlayerPressed { get; private set; }

    public KeyCode MoveUpKey => moveUpKey;
    public KeyCode MoveDownKey => moveDownKey;
    public KeyCode MoveLeftKey => moveLeftKey;
    public KeyCode MoveRightKey => moveRightKey;
    public KeyCode SprintKey => sprintKey;
    public KeyCode PassKey => passKey;
    public KeyCode ShootTakeBallKey => shootTakeBallKey;
    public KeyCode SwitchPlayerKey => switchPlayerKey;

    /// <summary>
    /// Reads the current keyboard input state.
    /// </summary>
    private void Update()
    {
        ReadMovementInput();
        ReadActionInput();
    }

    /// <summary>
    /// Reads movement from the configured arrow keys.
    /// </summary>
    private void ReadMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(moveLeftKey))
            horizontal -= 1f;

        if (Input.GetKey(moveRightKey))
            horizontal += 1f;

        if (Input.GetKey(moveDownKey))
            vertical -= 1f;

        if (Input.GetKey(moveUpKey))
            vertical += 1f;

        MovementInput = new Vector2(
            horizontal,
            vertical).normalized;
    }

    /// <summary>
    /// Reads the configured gameplay action keys.
    /// </summary>
    private void ReadActionInput()
    {
        SprintHeld = Input.GetKey(sprintKey);
        PassPressed = Input.GetKeyDown(passKey);
        ShootTakeBallPressed =
            Input.GetKeyDown(shootTakeBallKey);
        SwitchPlayerPressed =
            Input.GetKeyDown(switchPlayerKey);
    }
}