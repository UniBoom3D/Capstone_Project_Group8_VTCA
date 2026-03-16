using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 4)]
    public int playerCount = 1;

    [Header("Spawning Positions (From Map)")]
    public Transform[] playerSpawnPoints;

    [Header("Player Containers (Hierarchy)")]
    public Transform[] playerContainers;

    [Header("Prefabs")]
    public GameObject[] playerPrefabs;
    public GameObject playerCanvasPrefab;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;
    public Transform enemyContainer;

    private PlayerUISetup uiSetup;

    private void Awake()
    {
        // Tự động thêm hoặc tìm script Setup UI
        uiSetup = GetComponent<PlayerUISetup>();
        if (uiSetup == null) uiSetup = gameObject.AddComponent<PlayerUISetup>();
    }

    private void Start()
    {
        SpawnPlayers();
        SpawnEnemies();
    }

    private void SpawnPlayers()
    {
        Debug.Log($"<color=cyan>🎮 System: Initializing Battle with {playerCount} player(s).</color>");

        for (int i = 0; i < playerCount; i++)
        {
            if (i >= playerSpawnPoints.Length || i >= playerContainers.Length) continue;

            Transform mapPoint = playerSpawnPoints[i];
            Transform currentContainer = playerContainers[i];

            // 1. Spawn Player tại vị trí map
            GameObject selectedPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Length)];
            GameObject spawnedPlayer = Instantiate(selectedPrefab, mapPoint.position, mapPoint.rotation);
            spawnedPlayer.transform.SetParent(currentContainer);
            spawnedPlayer.name = $"Player_Unit_{i + 1}";
            // 2. Spawn Canvas UI cho Player
            if (playerCanvasPrefab != null)
            {
                GameObject canvasObj = Instantiate(playerCanvasPrefab, currentContainer);
                canvasObj.name = $"Canvas_Player_{i + 1}";

                // 3. Gọi để thiết lập kết nối
                uiSetup.SetupUI(spawnedPlayer, canvasObj, i + 1);
            }

            Debug.Log($"✅ Done: {spawnedPlayer.name} spawned & setup.");
        }
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || enemySpawnPoints.Length == 0) return;
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(randomEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            if (enemyContainer != null) enemy.transform.SetParent(enemyContainer);
        }
    }
}