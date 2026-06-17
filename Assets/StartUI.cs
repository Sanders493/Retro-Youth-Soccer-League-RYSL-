using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUI : MonoBehaviour
{
    [SerializeField] private string StartSceneName = "Placeholder";
    public void StartGame()
    {
        SceneManager.LoadScene(StartSceneName);
    }
}
