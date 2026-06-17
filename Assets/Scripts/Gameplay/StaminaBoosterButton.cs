using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the stamina booster button UI during a match.
/// </summary>
public class StaminaBoosterButton : MonoBehaviour
{
    [SerializeField] private PowerUpSystem powerUpSystem;
    [SerializeField] private Button staminaButton;
    [SerializeField] private TMP_Text buttonText;

    /// <summary>
    /// Sets up the stamina booster button when the scene starts.
    /// </summary>
    private void Start()
    {
        UpdateButtonVisuals();
    }

    /// <summary>
    /// Attempts to use the stamina booster when the button is clicked.
    /// </summary>
    public void OnStaminaBoosterButtonClicked()
    {
        if (powerUpSystem == null) return;

        powerUpSystem.TryUseStaminaBooster();
        UpdateButtonVisuals();
    }

    /// <summary>
    /// Updates the stamina booster button text and interactable state.
    /// </summary>
    public void UpdateButtonVisuals()
    {
        if (powerUpSystem == null) return;

        if (staminaButton != null)
        {
            staminaButton.interactable = !powerUpSystem.StaminaBoosterUsedThisMatch;
        }

        if (buttonText != null)
        {
            if (powerUpSystem.StaminaBoosterUsedThisMatch)
            {
                buttonText.text = "Used";
            }
            else
            {
                buttonText.text = "Stamina Boost - " + powerUpSystem.StaminaBoosterCost + " Coins";
            }
        }
    }
}
