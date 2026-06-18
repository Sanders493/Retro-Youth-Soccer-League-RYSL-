/// <summary>
/// Defines the match state and operations required by gameplay systems.
/// </summary>
public interface IMatchManager
{
    bool IsMatchOver
    {
        get;
    }

    int PlayerScore
    {
        get;
    }

    int OpponentScore
    {
        get;
    }

    /// <summary>
    /// Resets the match timer, scores, and match state.
    /// </summary>
    void ResetMatch();

    /// <summary>
    /// Starts the match timer.
    /// </summary>
    void StartMatchTimer();

    /// <summary>
    /// Pauses the match timer.
    /// </summary>
    void PauseMatchTimer();

    /// <summary>
    /// Resumes the match timer.
    /// </summary>
    void ResumeMatchTimer();

    /// <summary>
    /// Adds one goal to the player's score.
    /// </summary>
    void AddPlayerGoal();

    /// <summary>
    /// Adds one goal to the opponent's score.
    /// </summary>
    void AddOpponentGoal();
}