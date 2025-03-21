using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour {
    void LateUpdate() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            QuitGame();
        }
    }

    void QuitGame() {
        Debug.LogWarning("Quitting Game!");
        Application.Quit();
    }
}
