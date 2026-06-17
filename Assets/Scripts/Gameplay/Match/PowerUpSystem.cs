using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages match power-ups, including buying and activating a stamina booster.
/// </summary>
public class PowerUpSystem : MonoBehaviour
{
    [Header("Stamina Booster Settings")]
    [SerializeField] private int staminaBoosterCost = 10;
    [SerializeField] private float staminaBoostDuration = 30f;
    [SerializeField] private float boostedMoveSpeed = 8f;

    [Header("References")]
    [SerializeField] private PlayerInputReader playerMovement;
    [SerializeField] private SaveSystem saveSystem;

    [Header("UI")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text powerUpMessageText;
    [SerializeField] private TMP_Text timerText;

    private float originalMoveSpeed;
    private bool staminaBoosterUsedThisMatch;
    private bool staminaBoosterActive;

    public bool StaminaBoosterUsedThisMatch
    {
        get; private set;
    }

    public bool StaminaBoosterActive
    {
        get; private set;
    }

    public int StaminaBoosterCost
    {
        get; private set;
    }

    /// <summary>
    /// Stores player movement speed and prepares the stamina booster system.
    /// </summary>
    private void Start()
    {
        StaminaBoosterCost = staminaBoosterCost;

        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.MoveSpeed;
        }

        UpdateCoinText();
        ClearTimerText();
    }

    /// <summary>
    /// Attempts to buy and activate the stamina booster.
    /// </summary>
    public void TryUseStaminaBooster()
    {
        if (staminaBoosterUsedThisMatch)
        {
            ShowPowerUpMessage("Stamina booster already used this match!");
            return;
        }

        if (saveSystem == null || saveSystem.CurrentSaveData == null)
        {
            ShowPowerUpMessage("Save system missing!");
            return;
        }

        if (saveSystem.CurrentSaveData.Coins < staminaBoosterCost)
        {
            ShowPowerUpMessage("Not enough coins!");
            return;
        }

        int updatedCoins = saveSystem.CurrentSaveData.Coins - staminaBoosterCost;
        saveSystem.SaveCoins(updatedCoins);

        staminaBoosterUsedThisMatch = true;
        StaminaBoosterUsedThisMatch = true;

        UpdateCoinText();
        StartCoroutine(ActivateStaminaBooster());
    }

    /// <summary>
    /// Activates the stamina booster for a limited amount of time.
    /// </summary>
    /// <returns>Waits until the stamina booster expires.</returns>
    private IEnumerator ActivateStaminaBooster()
    {
        staminaBoosterActive = true;
        StaminaBoosterActive = true;

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(boostedMoveSpeed);
        }

        ShowPowerUpMessage("Stamina boost activated!");

        float timeRemaining = staminaBoostDuration;

        while (timeRemaining > 0f)
        {
            UpdateTimerText(timeRemaining);
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        EndStaminaBooster();
    }

    /// <summary>
    /// Ends the stamina booster and restores normal player speed.
    /// </summary>
    private void EndStaminaBooster()
    {
        staminaBoosterActive = false;
        StaminaBoosterActive = false;

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(originalMoveSpeed);
        }

        ClearTimerText();
        ShowPowerUpMessage("Stamina boost expired!");
    }

    /// <summary>
    /// Updates the coin text using the saved coin amount.
    /// </summary>
    private void UpdateCoinText()
    {
        if (coinText == null || saveSystem == null || saveSystem.CurrentSaveData == null) return;

        coinText.text = "Coins: " + saveSystem.CurrentSaveData.Coins;
    }

    /// <summary>
    /// Updates the stamina booster timer text.
    /// </summary>
    /// <param name="timeRemaining">The remaining boost time in seconds.</param>
    private void UpdateTimerText(float timeRemaining)
    {
        if (timerText == null) return;

        timerText.text = "Boost: " + Mathf.CeilToInt(timeRemaining) + "s";
    }

    /// <summary>
    /// Clears the stamina booster timer text.
    /// </summary>
    private void ClearTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "";
        }
    }

    /// <summary>
    /// Shows a power-up message on the screen.
    /// </summary>
    /// <param name="message">The message shown to the player.</param>
    private void ShowPowerUpMessage(string message)
    {
        if (powerUpMessageText != null)
        {
            powerUpMessageText.text = message;
        }
    }
}
