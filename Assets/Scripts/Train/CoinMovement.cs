using UnityEngine;

public class CoinMovement : MonoBehaviour {
    private bool isCollected = false;

    private void Start() {
        // Garante que a moeda tem um collider trigger
        BoxCollider coinCollider = GetComponent<BoxCollider>();
        if (coinCollider == null) {
            coinCollider = gameObject.AddComponent<BoxCollider>();
        }
        coinCollider.isTrigger = true;
        coinCollider.size = new Vector3(0.5f, 0.5f, 0.5f);

        // Garante que a tag está correta
        gameObject.tag = "Coin";
    }

    private void OnTriggerEnter(Collider other) {
        if (isCollected) return;

        if (other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) {
                isCollected = true;
                player.CollectCoin(gameObject);
            }
        }
    }

    private void OnDestroy() {
        isCollected = true;
    }
} 