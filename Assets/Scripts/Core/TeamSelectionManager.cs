using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles selecting one of two teams before starting a soccer match.
/// </summary>
public class TeamSelectionManager : MonoBehaviour
{
    [SerializeField] private string matchSceneName = "MatchScene";

    [SerializeField] private string teamOneName = "Madrid Meteors";
    [SerializeField] private string teamTwoName = "Valencia Stars";

    [SerializeField] private TMP_Text selectedTeamText;

    public string SelectedTeamName
    {
        get; private set;
    }

    /// <summary>
    /// Sets a default selected team when the team selection screen starts.
    /// </summary>
    private void Start()
    {
        SelectTeamOne();
    }

    /// <summary>
    /// Selects the first team option.
    /// </summary>
    public void SelectTeamOne()
    {
        SelectedTeamName = teamOneName;
        UpdateSelectedTeamText();
    }

    /// <summary>
    /// Selects the second team option.
    /// </summary>
    public void SelectTeamTwo()
    {
        SelectedTeamName = teamTwoName;
        UpdateSelectedTeamText();
    }

    /// <summary>
    /// Starts the match scene after saving the selected team name.
    /// </summary>
    public void StartMatch()
    {
        PlayerPrefs.SetString("SelectedTeamName", SelectedTeamName);
        SceneManager.LoadScene(matchSceneName);
    }

    /// <summary>
    /// Updates the UI text to show the currently selected team.
    /// </summary>
    private void UpdateSelectedTeamText()
    {
        if (selectedTeamText == null) return;

        selectedTeamText.text = "Selected Team: " + SelectedTeamName;
    }
}
