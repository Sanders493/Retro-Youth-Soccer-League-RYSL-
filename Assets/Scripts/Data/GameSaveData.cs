using System;

/// <summary>
/// Stores all data that should persist after closing and reopening the game.
/// </summary>
[Serializable]
public class GameSaveData
{
    public string SelectedTeamName = "Madrid Meteors";
    public int Coins = 0;
    public int UnlockedLevel = 1;
    public int MatchesWon = 0;
    public int GoalsScored = 0;
}
