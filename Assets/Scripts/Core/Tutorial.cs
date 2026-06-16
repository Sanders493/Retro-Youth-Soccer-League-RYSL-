using TMPro;
using UnityEngine;

/// <summary>
/// Manages practice mode instructions for movement, passing, and shooting.
/// </summary>
public class PracticeModeManager : MonoBehaviour
{
    private enum PracticeType
    {
        Movement,
        Passing,
        Shooting
    }

    [SerializeField] private PracticeType currentPracticeType;
    [SerializeField] private TMP_Text instructionText;

    public string CurrentInstruction
    {
        get; private set;
    }

    /// <summary>
    /// Sets the starting practice mode when the scene begins.
    /// </summary>
    private void Start()
    {
        SetPracticeMode(currentPracticeType);
    }

    /// <summary>
    /// Changes practice mode to movement practice.
    /// </summary>
    public void SetMovementPractice()
    {
        SetPracticeMode(PracticeType.Movement);
    }

    /// <summary>
    /// Changes practice mode to passing practice.
    /// </summary>
    public void SetPassingPractice()
    {
        SetPracticeMode(PracticeType.Passing);
    }

    /// <summary>
    /// Changes practice mode to shooting practice.
    /// </summary>
    public void SetShootingPractice()
    {
        SetPracticeMode(PracticeType.Shooting);
    }

    /// <summary>
    /// Updates the active practice mode and displays the correct instructions.
    /// </summary>
    /// <param name="practiceType">The selected practice type.</param>
    private void SetPracticeMode(PracticeType practiceType)
    {
        currentPracticeType = practiceType;

        switch (currentPracticeType)
        {
            case PracticeType.Movement:
                CurrentInstruction = "Movement Practice: Use WASD or Arrow Keys to move your chibi player around the field!";
                break;

            case PracticeType.Passing:
                CurrentInstruction = "Passing Practice: Move near the ball and press J to pass to your teammate!";
                break;

            case PracticeType.Shooting:
                CurrentInstruction = "Shooting Practice: Move near the ball and press K to shoot toward the goal!";
                break;
        }

        UpdateInstructionText();
    }

    /// <summary>
    /// Updates the tutorial instruction text on the UI.
    /// </summary>
    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        instructionText.text = CurrentInstruction;
    }
}
