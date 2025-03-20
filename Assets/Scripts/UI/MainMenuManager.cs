using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    public string mainSceneName;
    public void PlayButton() {
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
