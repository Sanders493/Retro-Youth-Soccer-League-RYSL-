using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages team selection before the match starts.
/// Stores the selected team and loads the match scene.
/// </summary>
public class TeamSelectionManager : MonoBehaviour
{
    [SerializeField] private string matchSceneName = "MatchScene";

    [SerializeField] private TMP_Text selectedTeamText;

    [SerializeField] private SaveSystem saveSystem;

    public string SelectedTeamName
    {
        get; private set;
    }

    /// <summary>
    /// Initializes the team selection screen.
    /// </summary>
    private void Start()
    {
        SelectedTeamName = "";

        if (selectedTeamText != null)
        {
            selectedTeamText.text = "Select Your Team";
        }
    }

    /// <summary>
    /// Sets the player's selected team.
    /// </summary>
    /// <param name="teamName">The team selected by the player.</param>
    public void SetSelectedTeam(string teamName)
    {
        SelectedTeamName = teamName;

        UpdateSelectedTeamText();

        SaveSelectedTeam();
    }

    /// <summary>
    /// Saves the selected team using the save system.
    /// </summary>
    private void SaveSelectedTeam()
    {
        if (saveSystem == null) return;

        saveSystem.SaveSelectedTeam(SelectedTeamName);
    }

    /// <summary>
    /// Starts the match scene if a team has been selected.
    /// </summary>
    public void StartMatch()
    {
        if (string.IsNullOrEmpty(SelectedTeamName))
        {
            Debug.LogWarning("Player must select a team before starting the match.");
            return;
        }

        SceneManager.LoadScene(matchSceneName);
    }

    /// <summary>
    /// Updates the selected team UI text.
    /// </summary>
    private void UpdateSelectedTeamText()
    {
        if (selectedTeamText != null)
        {
            selectedTeamText.text = "Selected Team: " + SelectedTeamName;
        }
    }

    /// <summary>
    /// Clears the currently selected team.
    /// </summary>
    public void ClearSelectedTeam()
    {
        SelectedTeamName = "";

        if (selectedTeamText != null)
        {
            selectedTeamText.text = "Select Your Team";
        }
    }
}
