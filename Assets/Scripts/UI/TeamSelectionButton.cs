using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles player team selection button interactions.
/// </summary>
public class PlayerSelectionButton : MonoBehaviour
{
    [SerializeField] private TeamSelectionManager teamSelectionManager;

    [SerializeField] private string teamName;

    [SerializeField] private TMP_Text selectedTeamText;

    [SerializeField] private Button selectButton;

    public string TeamName
    {
        get; private set;
    }

    /// <summary>
    /// Initializes the team name for this button.
    /// </summary>
    private void Start()
    {
        TeamName = teamName;
    }

    /// <summary>
    /// Selects this team and updates the Team Selection Manager.
    /// </summary>
    public void SelectTeam()
    {
        if (teamSelectionManager == null)
        {
            Debug.LogWarning("TeamSelectionManager is missing.");
            return;
        }

        TeamName = teamName;

        teamSelectionManager.SetSelectedTeam(TeamName);

        UpdateSelectedTeamText();
    }

    /// <summary>
    /// Updates the selected team UI text.
    /// </summary>
    private void UpdateSelectedTeamText()
    {
        if (selectedTeamText != null)
        {
            selectedTeamText.text = "Selected Team: " + TeamName;
        }
    }

    /// <summary>
    /// Disables the team selection button.
    /// </summary>
    public void DisableButton()
    {
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }
    }

    /// <summary>
    /// Enables the team selection button.
    /// </summary>
    public void EnableButton()
    {
        if (selectButton != null)
        {
            selectButton.interactable = true;
        }
    }
}
