using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    
    [Header("Audio")]
    public AudioClip hurtSfx;
    public AudioClip coinSfx;

    private int score = 0;
    private const int POINTS_PER_COIN = 10;
    private AudioSource audioSource;

    void Start() {
        UpdateScoreUI();
        UpdateHealthUI(3); // Começa com vida máxima
        audioSource = GetComponent<AudioSource>();
    }

    public void AddScore() {
        score += POINTS_PER_COIN;
        audioSource.clip = coinSfx;
        audioSource.Play();
        UpdateScoreUI();
    }

    public void UpdateHealth(int health) {
        audioSource.clip = hurtSfx;
        audioSource.Play();
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