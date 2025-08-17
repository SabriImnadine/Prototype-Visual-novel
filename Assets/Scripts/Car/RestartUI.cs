using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartUI : MonoBehaviour
{
    public GameObject gameOverPanel;   
    [SerializeField] string vnSceneName = "SampleScene"; 

    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnBackToVN()
    {
        Time.timeScale = 1f;
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.LoadSceneWithFade(vnSceneName);
        else
            SceneManager.LoadScene(vnSceneName);
    }
}
