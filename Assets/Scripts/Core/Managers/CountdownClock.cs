using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class CountdownClock : MonoBehaviour
{
    //in seconds
    private int timeRemaining;
    /// <summary>
    /// Returns the time remaining in a nice and easy to comprehend format with a dedicated minutes and seconds place
    /// </summary>
    public TimeSpan TimeRemaining => TimeSpan.FromSeconds(timeRemaining);

    private WaitForSeconds oneSecondDelay;
    private Coroutine countdownCoroutine;
    public bool CountdownExists => countdownCoroutine != null;

    /// <summary>
    /// Is invoked once the countdown is finished
    /// </summary>
    public Action onCountdownFinished;

    private void Awake()
    {
        oneSecondDelay = new WaitForSeconds(1);
    }

    /// <summary>
    /// Call to begin the contdown clock
    /// </summary>
    /// <param name="minutes">How many minutes will be on the clock</param>
    public void BeginCountdown(float minutes)
    {
        timeRemaining = Mathf.RoundToInt(minutes * 60);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }



    private IEnumerator CountdownCoroutine()
    {
        while(timeRemaining > 0)
        {
            timeRemaining--;
            yield return oneSecondDelay;
        }
        onCountdownFinished?.Invoke();
        countdownCoroutine = null;
    }

    /// <summary>
    /// Starts up the countdown if it isn't active
    /// </summary>
    public void ResumeCountdown()
    {
        if(countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }
        else
        {
            Debug.LogWarning("Unable to resume the countdown as one is already running", gameObject);
        }    
    }


    /// <summary>
    /// Stops the countdown if one is active
    /// </summary>
    public void PauseCountdown()
    {
        if(countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        else
        {
            Debug.LogWarning("Unable to non-existent countdown. Was a countdown started prior to this event calling", gameObject);
        }
    }

}
