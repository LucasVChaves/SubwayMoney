using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    
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
            scoreText.text = $"SCORE: {score}";
        }
    }

    private void UpdateHealthUI(int health) {
        if (healthText != null) {
            healthText.text = $"HEALTH: {health}";
        }
    }
} 