using UnityEngine;

public class TrainSpawner : MonoBehaviour {
    [Header("Train Spawn Settings")]
    public GameObject trainPrefab;
    public float spawnInterval = 3f; // Tempo entre spawns de trens
    public float spawnZ = 20f; // Posição Z onde os trens spawnam
    public float minX = -3f; // Posição X mínima (faixa da esquerda)
    public float maxX = 3f; // Posição X máxima (faixa da direita)
    public float trainSpeed = 15f; // Velocidade dos trens

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float coinSpawnChance = 0.5f; // Chance de spawnar moedas (0-1)
    public float coinHeight = 1.5f; // Altura das moedas em relação ao trem
    public float coinSpacing = 1f; // Espaçamento entre moedas
    public int maxCoinsPerTrain = 3; // Número máximo de moedas por trem

    private float nextSpawnTime;
    private bool isInitialized = false;

    void Start() {
        // Verifica se os prefabs necessários estão atribuídos
        if (trainPrefab == null) {
            Debug.LogError("Train prefab is not assigned to TrainSpawner!");
            enabled = false; // Desativa o script se não tiver o prefab
            return;
        }

        nextSpawnTime = Time.time + spawnInterval;
        isInitialized = true;
        Debug.Log("TrainSpawner initialized successfully");
    }

    void Update() {
        if (!isInitialized) return;

        if (Time.time >= nextSpawnTime) {
            SpawnTrain();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnTrain() {
        if (trainPrefab == null) {
            Debug.LogError("Train prefab is missing!");
            enabled = false;
            return;
        }

        try {
            // Escolhe uma das três faixas aleatoriamente
            int lane = Random.Range(0, 3); // 0 = esquerda, 1 = centro, 2 = direita
            float xPos = lane * 3f - 3f; // Converte a faixa em posição X (-3, 0, ou 3)

            Vector3 spawnPosition = new Vector3(xPos, 0f, spawnZ);
            
            GameObject train = Instantiate(trainPrefab, spawnPosition, Quaternion.identity);
            if (train == null) {
                Debug.LogError("Failed to instantiate train!");
                return;
            }
            
            // Garante que o trem tem a tag correta
            train.tag = "Train";
            train.layer = LayerMask.NameToLayer("Ground");
            
            // Configura o TrainMovement
            TrainMovement trainMovement = train.GetComponent<TrainMovement>();
            if (trainMovement == null) {
                trainMovement = train.AddComponent<TrainMovement>();
                Debug.Log("Added TrainMovement component to train");
            }
            
            // Configura a velocidade e altura
            trainMovement.speed = trainSpeed;
            trainMovement.trainHeight = 1f;
            trainMovement.destroyZ = -30f;
            
            Debug.Log($"Train spawned at lane {lane} (X: {xPos}) with speed {trainSpeed}");

            // Chance de spawnar moedas
            if (Random.value < coinSpawnChance && coinPrefab != null) {
                SpawnCoinsOnTrain(train);
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Error spawning train: {e.Message}");
            enabled = false;
        }
    }

    void SpawnCoinsOnTrain(GameObject train) {
        if (train == null || coinPrefab == null) {
            Debug.LogWarning("Cannot spawn coins: train or coin prefab is missing");
            return;
        }

        try {
            // Número aleatório de moedas (1 a maxCoinsPerTrain)
            int numCoins = Random.Range(1, maxCoinsPerTrain + 1);
            
            // Calcula o comprimento total das moedas
            float totalLength = (numCoins - 1) * coinSpacing;
            
            // Posição inicial da primeira moeda
            Vector3 startPos = train.transform.position + Vector3.up * coinHeight;
            startPos.z -= totalLength / 2f; // Centraliza as moedas no trem
            
            // Spawna as moedas
            for (int i = 0; i < numCoins; i++) {
                Vector3 coinPos = startPos + Vector3.forward * (i * coinSpacing);
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                
                if (coin != null) {
                    // Configura a tag da moeda
                    coin.tag = "Coin";
                    
                    // Faz a moeda ser filho do trem para mover junto
                    coin.transform.SetParent(train.transform);
                    
                    Debug.Log($"Coin spawned at {coinPos}");
                }
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Error spawning coins: {e.Message}");
        }
    }
} 