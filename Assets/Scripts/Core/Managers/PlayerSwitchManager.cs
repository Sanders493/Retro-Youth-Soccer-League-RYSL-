using TMPro;
using UnityEngine;

/// <summary>
/// Allows the human player to switch control between active PlayerActor teammates.
/// </summary>
public class PlayerSwitchManager : MonoBehaviour
{
    [SerializeField] private PlayerActor[] playerTeamActors;
    [SerializeField] private KeyCode switchKey = KeyCode.Tab;
    [SerializeField] private TMP_Text activePlayerText;

    private int currentPlayerIndex;

    public int CurrentPlayerIndex
    {
        get; private set;
    }

    /// <summary>
    /// Sets the first player actor as the human-controlled player when the match starts.
    /// </summary>
    private void Start()
    {
        SetControlledPlayer(0);
    }

    /// <summary>
    /// Checks for player switch input during gameplay.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SwitchToNextPlayer();
        }
    }

    /// <summary>
    /// Switches human control to the next available player actor.
    /// </summary>
    public void SwitchToNextPlayer()
    {
        if (playerTeamActors == null || playerTeamActors.Length == 0)
        {
            Debug.LogWarning("No player team actors assigned.");
            return;
        }

        int nextIndex = GetNextValidActorIndex();

        if (nextIndex == currentPlayerIndex)
        {
            return;
        }

        SetControlledPlayer(nextIndex);
    }

    /// <summary>
    /// Finds the next valid active teammate actor.
    /// </summary>
    /// <returns>The index of the next valid actor.</returns>
    private int GetNextValidActorIndex()
    {
        for (int i = 1; i <= playerTeamActors.Length; i++)
        {
            int nextIndex = (currentPlayerIndex + i) % playerTeamActors.Length;

            if (playerTeamActors[nextIndex] != null && playerTeamActors[nextIndex].IsActive)
            {
                return nextIndex;
            }
        }

        return currentPlayerIndex;
    }

    /// <summary>
    /// Sets one actor as human-controlled and returns all other teammates to AI control.
    /// </summary>
    /// <param name="newPlayerIndex">The index of the actor receiving human control.</param>
    private void SetControlledPlayer(int newPlayerIndex)
    {
        if (playerTeamActors == null || playerTeamActors.Length == 0)
        {
            return;
        }

        currentPlayerIndex = Mathf.Clamp(newPlayerIndex, 0, playerTeamActors.Length - 1);
        CurrentPlayerIndex = currentPlayerIndex;

        for (int i = 0; i < playerTeamActors.Length; i++)
        {
            if (playerTeamActors[i] == null)
            {
                continue;
            }

            bool isControlledPlayer = i == currentPlayerIndex;

            playerTeamActors[i].Initialize(
                playerTeamActors[i].ActorId,
                playerTeamActors[i].TeamId,
                playerTeamActors[i].FormationPosition,
                playerTeamActors[i].IsActive,
                !isControlledPlayer,
                false
            );
        }

        UpdateActivePlayerText();
    }

    /// <summary>
    /// Updates the UI text that displays the current controlled actor.
    /// </summary>
    private void UpdateActivePlayerText()
    {
        if (activePlayerText == null || playerTeamActors == null || playerTeamActors.Length == 0)
        {
            return;
        }

        PlayerActor currentActor = playerTeamActors[currentPlayerIndex];

        if (currentActor == null)
        {
            activePlayerText.text = "Active Player: None";
            return;
        }

        activePlayerText.text = "Active Player: " + currentActor.ActorId;
    }
}
