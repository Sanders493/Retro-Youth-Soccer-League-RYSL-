using TMPro;
using UnityEngine;

/// <summary>
/// Allows the human player to switch control between teammates during the match.
/// </summary>
public class PlayerSwitchManager : MonoBehaviour
{
    [SerializeField] private GameObject[] playerTeamMembers;
    [SerializeField] private KeyCode switchKey = KeyCode.Tab;
    [SerializeField] private TMP_Text activePlayerText;

    private int currentPlayerIndex;

    public int CurrentPlayerIndex
    {
        get; private set;
    }

    /// <summary>
    /// Sets the first player as the controlled player when the match starts.
    /// </summary>
    private void Start()
    {
        SetControlledPlayer(0);
    }

    /// <summary>
    /// Checks for player switch input.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SwitchToNextPlayer();
        }
    }

    /// <summary>
    /// Switches control to the next teammate in the player team array.
    /// </summary>
    public void SwitchToNextPlayer()
    {
        if (playerTeamMembers == null || playerTeamMembers.Length == 0) return;

        int nextPlayerIndex = (currentPlayerIndex + 1) % playerTeamMembers.Length;
        SetControlledPlayer(nextPlayerIndex);
    }

    /// <summary>
    /// Sets which teammate is controlled by the human player and which teammates use AI.
    /// </summary>
    /// <param name="newPlayerIndex">The index of the teammate becoming human-controlled.</param>
    private void SetControlledPlayer(int newPlayerIndex)
    {
        currentPlayerIndex = newPlayerIndex;
        CurrentPlayerIndex = currentPlayerIndex;

        for (int i = 0; i < playerTeamMembers.Length; i++)
        {
            if (playerTeamMembers[i] == null) continue;

            bool isControlledPlayer = i == currentPlayerIndex;

            PlayerMovement2D movement = playerTeamMembers[i].GetComponent<PlayerMovement2D>();
            PlayerKickController kickController = playerTeamMembers[i].GetComponent<PlayerKickController>();
            AIOpponentPlayer aiPlayer = playerTeamMembers[i].GetComponent<AIOpponentPlayer>();

            if (movement != null)
            {
                movement.enabled = isControlledPlayer;
            }

            if (kickController != null)
            {
                kickController.enabled = isControlledPlayer;
            }

            if (aiPlayer != null)
            {
                aiPlayer.enabled = !isControlledPlayer;
            }
        }

        UpdateActivePlayerText();
    }

    /// <summary>
    /// Updates the UI text that displays the currently controlled teammate.
    /// </summary>
    private void UpdateActivePlayerText()
    {
        if (activePlayerText != null)
        {
            activePlayerText.text = "Player " + (CurrentPlayerIndex + 1);
        }
    }
}
