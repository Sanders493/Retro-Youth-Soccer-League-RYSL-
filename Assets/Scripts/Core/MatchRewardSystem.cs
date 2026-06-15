using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gives coin rewards based on goals scored and manages a one-time stamina booster during a match.
/// </summary>
public class MatchRewardSystem : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField] private int maxCoinsPerMatch = 20;
    [SerializeField] private int coinsPerGoal = 1;

    [Header("Stamina Booster Settings")]
    [SerializeField] private int staminaBoosterCost = 10;
    [SerializeField] private float boostDuration = 30f;
    [SerializeField] private float boostedMoveSpeed = 8f;

    [Header("References")]
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private SaveSystem saveSystem;

    [Header("UI")]
    [SerializeField] private Button staminaBoosterButton;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text rewardMessageText;
    [SerializeField] private TMP_Text boosterMessageText;

    private int playerGoalsScored;
    private int coinsEarnedThisMatch;
    private float originalMoveSpeed;
    private bool boosterPurchasedThisMatch;
    private bool boosterActive;

    public int PlayerGoalsScored
    {
        get; private set;
    }

    public int CoinsEarnedThisMatch
    {
        get; private set;
    }

    public bool BoosterPurchasedThisMatch
    {
        get; private set;
    }

    public bool BoosterActive
    {
        get; private set;
    }

    /// <summary>
    /// Stores the player's starting movement speed and prepares the booster button.
    /// </summary>
    private void Start()
    {
        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.MoveSpeed;
        }

        UpdateCoinText();
        UpdateBoosterButton();
    }

    /// <summary>
    /// Adds one goal to the player's match goal count.
    /// Call this when the player scores a goal.
    /// </summary>
    public void AddPlayerGoal()
    {
        playerGoalsScored++;
        PlayerGoalsScored = playerGoalsScored;

        ShowRewardMessage("Goal scored! Goals: " + playerGoalsScored);
    }

    /// <summary>
    /// Calculates and gives coins at the end of the match based on the player's goals.
    /// </summary>
    public void GiveEndMatchRewards()
    {
        coinsEarnedThisMatch = Mathf.Min(playerGoalsScored * coinsPerGoal, maxCoinsPerMatch);
        CoinsEarnedThisMatch = coinsEarnedThisMatch;

        if (saveSystem != null)
        {
            int updatedCoins = saveSystem.CurrentSaveData.Coins + coinsEarnedThisMatch;
            saveSystem.SaveCoins(updatedCoins);
        }

        UpdateCoinText();
        ShowRewardMessage("Match reward: +" + coinsEarnedThisMatch + " coins!");
    }

    /// <summary>
    /// Attempts to buy and activate the stamina booster once during the match.
    /// </summary>
    public void BuyStaminaBooster()
    {
        if (boosterPurchasedThisMatch)
        {
            ShowBoosterMessage("Stamina booster already used this match!");
            return;
        }

        if (saveSystem == null || saveSystem.CurrentSaveData.Coins < staminaBoosterCost)
        {
            ShowBoosterMessage("Not enough coins!");
            return;
        }

        int updatedCoins = saveSystem.CurrentSaveData.Coins - staminaBoosterCost;
        saveSystem.SaveCoins(updatedCoins);

        boosterPurchasedThisMatch = true;
        BoosterPurchasedThisMatch = true;

        UpdateCoinText();
        UpdateBoosterButton();

        StartCoroutine(ActivateStaminaBoost());
    }

    /// <summary>
    /// Activates the stamina boost for a limited time, then restores the player's original speed.
    /// </summary>
    /// <returns>Waits for the booster duration before ending the effect.</returns>
    private IEnumerator ActivateStaminaBoost()
    {
        boosterActive = true;
        BoosterActive = true;

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(boostedMoveSpeed);
        }

        ShowBoosterMessage("Stamina boost active for 30 seconds!");

        yield return new WaitForSeconds(boostDuration);

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(originalMoveSpeed);
        }

        boosterActive = false;
        BoosterActive = false;

        ShowBoosterMessage("Stamina boost expired!");
    }

    /// <summary>
    /// Updates the booster button so it cannot be used more than once per match.
    /// </summary>
    private void UpdateBoosterButton()
    {
        if (staminaBoosterButton != null)
        {
            staminaBoosterButton.interactable = !boosterPurchasedThisMatch;
        }
    }

    /// <summary>
    /// Updates the coin UI text using the save system coin amount.
    /// </summary>
    private void UpdateCoinText()
    {
        if (coinText == null || saveSystem == null || saveSystem.CurrentSaveData == null) return;

        coinText.text = "Coins: " + saveSystem.CurrentSaveData.Coins;
    }

    /// <summary>
    /// Displays a reward message on the UI.
    /// </summary>
    /// <param name="message">The reward message shown to the player.</param>
    private void ShowRewardMessage(string message)
    {
        if (rewardMessageText != null)
        {
            rewardMessageText.text = message;
        }
    }

    /// <summary>
    /// Displays a stamina booster message on the UI.
    /// </summary>
    /// <param name="message">The stamina booster message shown to the player.</param>
    private void ShowBoosterMessage(string message)
    {
        if (boosterMessageText != null)
        {
            boosterMessageText.text = message;
        }
    }
}
