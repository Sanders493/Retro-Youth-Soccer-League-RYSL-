using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads player game data using a JSON file.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    [SerializeField] private string saveFileName = "soccer_save_data.json";

    public GameSaveData CurrentSaveData
    {
        get; private set;
    }

    private string SaveFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, saveFileName);
        }
    }

    /// <summary>
    /// Loads save data when the game starts.
    /// </summary>
    private void Awake()
    {
        LoadGame();
    }

    /// <summary>
    /// Saves the current game data to a JSON file.
    /// </summary>
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSaveData, true);
        File.WriteAllText(SaveFilePath, json);

        Debug.Log("Game saved to: " + SaveFilePath);
    }

    /// <summary>
    /// Loads saved game data from a JSON file. Creates new data if no save file exists.
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            CurrentSaveData = new GameSaveData();
            SaveGame();
            return;
        }

        string json = File.ReadAllText(SaveFilePath);
        CurrentSaveData = JsonUtility.FromJson<GameSaveData>(json);
    }

    /// <summary>
    /// Updates the selected team and saves the game.
    /// </summary>
    /// <param name="teamName">The selected team name.</param>
    public void SaveSelectedTeam(string teamName)
    {
        CurrentSaveData.SelectedTeamName = teamName;
        SaveGame();
    }

    /// <summary>
    /// Updates the player's coins and saves the game.
    /// </summary>
    /// <param name="coins">The total amount of coins.</param>
    public void SaveCoins(int coins)
    {
        CurrentSaveData.Coins = coins;
        SaveGame();
    }

    /// <summary>
    /// Updates the player's unlocked level and saves the game.
    /// </summary>
    /// <param name="level">The highest unlocked level.</param>
    public void SaveUnlockedLevel(int level)
    {
        CurrentSaveData.UnlockedLevel = level;
        SaveGame();
    }

    /// <summary>
    /// Deletes all save data and creates a new blank save file.
    /// </summary>
    public void ResetSaveData()
    {
        CurrentSaveData = new GameSaveData();
        SaveGame();
    }
}
