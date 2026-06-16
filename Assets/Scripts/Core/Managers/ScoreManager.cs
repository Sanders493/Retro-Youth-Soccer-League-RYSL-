using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public int homeScore { get; private set; }
    public int awayScore {get; private set; }

    /// <summary>
    /// Called whenever a goal is scored
    /// 
    /// Vector2Int Info: X refers to Home, Y refers to Away
    /// </summary>
    public event Action<Vector2Int> OnGoalScored;

    public void IncreaseHomeScore()
    {
        homeScore++;
        OnGoalScored?.Invoke(new(homeScore, awayScore));
    }

    public void IncreaseAwayScore()
    {
        awayScore++;
        OnGoalScored?.Invoke(new(homeScore, awayScore));
    }
}
