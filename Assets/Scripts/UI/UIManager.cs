using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Header("UI Elements")]
    public Text scoreText;
    public Text healthText;
    
    private int score = 0;
    private const int POINTS_PER_COIN = 10;

    void Start() {
        UpdateScoreUI();
        UpdateHealthUI(3); // Começa com vida máxima
    }

    public void AddScore() {
        score += POINTS_PER_COIN;
        UpdateScoreUI();
    }

    public void UpdateHealth(int health) {
        UpdateHealthUI(health);
    }

    private void UpdateScoreUI() {
        if (scoreText != null) {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateHealthUI(int health) {
        if (healthText != null) {
            healthText.text = $"Health: {health}";
        }
    }
} 