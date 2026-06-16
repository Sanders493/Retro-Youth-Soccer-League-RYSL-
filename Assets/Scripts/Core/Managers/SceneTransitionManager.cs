using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles transition between match scene and results screen
/// </summary>
public class SceneTransitionManager : MonoBehaviour {

    [SerializeField] private string resultsSceneName = "EndScene";

    /// <summary>
    /// subscribes to the countdown action in CountdownClock. 
    /// </summary>
    private void Start() {
        CountdownClock.Instance.onCountdownFinished += EndGame;
    }

    /// <summary>
    /// When the countdown reaches 0, load the results screen. 
    /// </summary>
    private void EndGame() {
        SceneManager.LoadScene(resultsSceneName);
    }

    private void OnDestroy() {
        if (CountdownClock.Instance != null) {
            CountdownClock.Instance.onCountdownFinished -= EndGame;
        }
    }
    
}