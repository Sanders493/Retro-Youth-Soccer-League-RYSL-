using TMPro;
using UnityEngine;

/// <summary>
/// Displays kid-friendly soccer game messages and plays crowd sound effects.
/// </summary>
public class AudioTextMessageManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private AudioSource crowdAudioSource;
    [SerializeField] private AudioClip crowdCheersClip;
    [SerializeField] private AudioClip crowdBoosClip;

    [SerializeField] private float messageDuration = 2f;

    public string CurrentMessage
    {
        get; private set;
    }

    /// <summary>
    /// Clears the message text when the scene starts.
    /// </summary>
    private void Start()
    {
        ClearMessage();
    }

    /// <summary>
    /// Shows a teamwork message.
    /// </summary>
    public void ShowGreatTeamworkMessage()
    {
        ShowMessage("Great teamwork!");
    }

    /// <summary>
    /// Shows a practice encouragement message.
    /// </summary>
    public void ShowKeepPracticingMessage()
    {
        ShowMessage("Keep practicing!");
    }

    /// <summary>
    /// Shows a contribution encouragement message.
    /// </summary>
    public void ShowEveryPlayerContributesMessage()
    {
        ShowMessage("Every player contributes!");
    }

    /// <summary>
    /// Shows a win message and plays crowd cheers.
    /// </summary>
    public void ShowWinMessage()
    {
        ShowMessage("You win!");
        PlayCrowdCheers();
    }

    /// <summary>
    /// Shows a lose message and plays crowd boos.
    /// </summary>
    public void ShowLoseMessage()
    {
        ShowMessage("You lose!");
        PlayCrowdBoos();
    }

    /// <summary>
    /// Shows a goal message and plays crowd cheers.
    /// </summary>
    public void ShowGoalMessage()
    {
        ShowMessage("Goal! Great teamwork!");
        PlayCrowdCheers();
    }

    /// <summary>
    /// Plays the crowd cheer sound effect.
    /// </summary>
    public void PlayCrowdCheers()
    {
        if (crowdAudioSource == null || crowdCheersClip == null) return;

        crowdAudioSource.PlayOneShot(crowdCheersClip);
    }

    /// <summary>
    /// Plays the crowd boo sound effect.
    /// </summary>
    public void PlayCrowdBoos()
    {
        if (crowdAudioSource == null || crowdBoosClip == null) return;

        crowdAudioSource.PlayOneShot(crowdBoosClip);
    }

    /// <summary>
    /// Displays a message on the TMP text UI.
    /// </summary>
    /// <param name="message">The message that should appear on screen.</param>
    public void ShowMessage(string message)
    {
        CurrentMessage = message;

        if (messageText != null)
        {
            messageText.text = CurrentMessage;
        }

        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), messageDuration);
    }

    /// <summary>
    /// Clears the current TMP message.
    /// </summary>
    public void ClearMessage()
    {
        CurrentMessage = "";

        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}
