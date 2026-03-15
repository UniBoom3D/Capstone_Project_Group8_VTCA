using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [Header("Player Spawning")]
    [Tooltip("List of possible Player Prefabs to spawn")]
    public GameObject[] playerPrefabs;
    [Tooltip("List of possible places the player can start")]
    public Transform[] playerSpawnPoints;
    public Transform playerContainer;

    [Header("Enemy Spawning")]
    [Tooltip("List of possible Enemy Prefabs to spawn (Mix and match!)")]
    public GameObject[] enemyPrefabs;
    [Tooltip("List of places enemies will spawn")]
    public Transform[] enemySpawnPoints;
    public Transform enemyContainer;

    private void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefabs.Length == 0 || playerSpawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Missing Player Prefabs or Spawn Points in the Inspector!");
            return;
        }

        // 1. Pick a random Player Prefab
        GameObject selectedPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Length)];

        // 2. Pick a random Spawn Point
        Transform spawnPoint = playerSpawnPoints[Random.Range(0, playerSpawnPoints.Length)];

        // 3. Spawn them!
        GameObject player = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);

        if (playerContainer != null) player.transform.SetParent(playerContainer);

        Debug.Log($"🦸 Spawned {selectedPrefab.name} at {spawnPoint.name}!");
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Missing Enemy Prefabs or Spawn Points in the Inspector!");
            return;
        }

        // Loop through every spawn point and put a random enemy there
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            // Pick a random Enemy Prefab for THIS specific spot
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject enemy = Instantiate(randomEnemyPrefab, spawnPoint.position, spawnPoint.rotation);

            if (enemyContainer != null) enemy.transform.SetParent(enemyContainer);
        }

        Debug.Log($"👾 {enemySpawnPoints.Length} Random Enemies Spawned!");
    }
}