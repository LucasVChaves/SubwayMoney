using UnityEngine;

public class TrainSpawner : MonoBehaviour {
    [Header("Train Spawn Settings")]
    public GameObject[] trainPrefabs;
    public float spawnInterval = 3f;
    public float spawnZ = 20f;
    public float minX = -3f;
    public float maxX = 3f;
    public float trainSpeed = 15f;
    // Fiz todos os prefabs de ré e assim eh mais facil doq girar todos
    public float trainRotation = 90f;

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float coinSpawnChance = 0.5f;
    public float coinHeight = 1.5f;
    public float coinSpacing = 1f;
    public int maxCoinsPerTrain = 3;

    private float nextSpawnTime;
    private bool isInitialized = false;

    void Start() {
        if (trainPrefabs == null) {
            Debug.LogError("Train prefabs are not assigned to TrainSpawner!");
            enabled = false;
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
        if (trainPrefabs == null) {
            Debug.LogError("Train prefab is missing!");
            enabled = false;
            return;
        }

        try {
            int lane = Random.Range(0, 3); // 0 = esquerda, 1 = centro, 2 = direita
            float xPos = lane * 3f - 3f;

            Vector3 spawnPosition = new Vector3(xPos, 0f, spawnZ);

            GameObject train = new GameObject();
            int trainToSpawn = Random.Range(0, trainPrefabs.Length);
            // Por algum motivo o modelo da locotimotiva sozinha ta girado todo erradoe eu nao consigo desvirar, tentei ate fazer no blender e re-exportar e nao funcionou
            if (trainToSpawn == 3) {
                train = Instantiate(trainPrefabs[trainToSpawn], spawnPosition, Quaternion.Euler(-trainRotation, trainRotation*2, 0));
                
            } else if (trainToSpawn == 4) {
                train = Instantiate(trainPrefabs[trainToSpawn], spawnPosition, Quaternion.Euler(0, 0, 0));
            } else {
                train = Instantiate(trainPrefabs[trainToSpawn], spawnPosition, Quaternion.Euler(0, trainRotation, 0));
            }
            
            if (train == null) {
                Debug.LogError("Failed to instantiate train!");
                return;
            }
            
            train.tag = "Train";
            train.layer = LayerMask.NameToLayer("Ground");

            TrainMovement trainMovement = train.GetComponent<TrainMovement>();
            if (trainMovement == null) {
                trainMovement = train.AddComponent<TrainMovement>();
                Debug.Log("Added TrainMovement component to train");
            }
            

            trainMovement.speed = trainSpeed;
            trainMovement.trainHeight = 1f;
            trainMovement.destroyZ = -30f;
            
            Debug.Log($"Train spawned at lane {lane} (X: {xPos}) with speed {trainSpeed}");

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
            int numCoins = Random.Range(1, maxCoinsPerTrain + 1);
            
            float totalLength = (numCoins - 1) * coinSpacing;
            
            Vector3 startPos = train.transform.position + Vector3.up * coinHeight;
            startPos.z -= totalLength / 2f; // Centraliza as moedas no trem
            
            for (int i = 0; i < numCoins; i++) {
                Vector3 coinPos = startPos + Vector3.forward * (i * coinSpacing);
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                
                if (coin != null) {
                    coin.tag = "Coin";

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