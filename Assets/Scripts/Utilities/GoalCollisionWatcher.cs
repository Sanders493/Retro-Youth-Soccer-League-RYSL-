using System;
using UnityEngine;

/// <summary>
/// Exists solely to call an event for when a trigger on its object was entered
/// </summary>
public class GoalCollisionWatcher : MonoBehaviour
{
    private bool isHomeGoal = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ScoreManager.Instance && collision.CompareTag("Ball"))
        {
            if (isHomeGoal)
            {
                ScoreManager.Instance.IncreaseHomeScore();
            }
            else
            {
                ScoreManager.Instance.IncreaseAwayScore();
            }
        }
    }

}
