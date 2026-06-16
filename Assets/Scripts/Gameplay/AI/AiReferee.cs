using TMPro;
using UnityEngine;

/// <summary>
/// Detects soccer penalty situations and displays a referee penalty message.
/// </summary>
public class AIReferee : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private TMP_Text refereeMessageText;

    [SerializeField] private float penaltyBoxMinimumX = -7f;
    [SerializeField] private float penaltyBoxMaximumX = -4f;
    [SerializeField] private float penaltyBoxMinimumY = -2f;
    [SerializeField] private float penaltyBoxMaximumY = 2f;

    [SerializeField] private float messageDuration = 2f;

    private bool penaltyCalled;

    public bool PenaltyCalled
    {
        get; private set;
    }

    /// <summary>
    /// Hides referee message at the start of the scene.
    /// </summary>
    private void Start()
    {
        if (refereeMessageText != null)
        {
            refereeMessageText.text = "";
        }
    }

    /// <summary>
    /// Checks if a penalty should be called.
    /// </summary>
    private void Update()
    {
        if (ball == null || penaltyCalled) return;

        if (IsBallInsidePenaltyBox())
        {
            CallPenalty();
        }
    }

    /// <summary>
    /// Checks if the ball is inside the penalty box area.
    /// </summary>
    /// <returns>True if the ball is inside the penalty box.</returns>
    private bool IsBallInsidePenaltyBox()
    {
        Vector3 ballPosition = ball.position;

        return ballPosition.x >= penaltyBoxMinimumX &&
               ballPosition.x <= penaltyBoxMaximumX &&
               ballPosition.y >= penaltyBoxMinimumY &&
               ballPosition.y <= penaltyBoxMaximumY;
    }

    /// <summary>
    /// Calls a penalty and displays the referee message.
    /// </summary>
    private void CallPenalty()
    {
        penaltyCalled = true;
        PenaltyCalled = true;

        if (refereeMessageText != null)
        {
            refereeMessageText.text = "Penalty Called!";
        }

        Invoke(nameof(ClearPenaltyMessage), messageDuration);
    }

    /// <summary>
    /// Clears the penalty message after a short delay.
    /// </summary>
    private void ClearPenaltyMessage()
    {
        if (refereeMessageText != null)
        {
            refereeMessageText.text = "";
        }
    }

    /// <summary>
    /// Resets the referee so another penalty can be called.
    /// </summary>
    public void ResetPenaltyCall()
    {
        penaltyCalled = false;
        PenaltyCalled = false;
    }
}
