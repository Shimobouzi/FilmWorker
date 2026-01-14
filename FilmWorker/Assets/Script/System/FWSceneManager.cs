using UnityEngine;
using UnityEngine.SceneManagement;

public class FWSceneManager : MonoBehaviour
{
    public static FWSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void TitleSceneLoad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void MainSceneLoad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main1");
    }

    public void ReplayMainSceneLoad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main1");
    }

    public void TutorialSceneLoad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial");
    }

    public void ReplayTutorialSceneLoad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial");
    }


}
