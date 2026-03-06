using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattlePvETurtleLv1 : BattleHandlerPvE
{
    [Header("Map")]
    [SerializeField] private Transform mapPrefab;

    [Header("Spawn Containers")]
    [SerializeField] private Transform spawnPlayerContainer;
    [SerializeField] private Transform spawnEnemyContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject turtleEnemyPrefab;

    private List<Transform> playerSpawnPoints = new();
    private List<Transform> enemySpawnPoints = new();

    protected override void Awake()
    {
        base.Awake();

        CacheSpawnPoints();
    }

    private void Start()
    {
        Debug.Log("🏝 Loading Turtle Map");

        SpawnPlayer();
        SpawnEnemies();
    }

    // =========================
    // CACHE SPAWN POINTS
    // =========================

    void CacheSpawnPoints()
    {
        if (spawnPlayerContainer != null)
        {
            foreach (Transform child in spawnPlayerContainer)
                playerSpawnPoints.Add(child);
        }

        if (spawnEnemyContainer != null)
        {
            foreach (Transform child in spawnEnemyContainer)
                enemySpawnPoints.Add(child);
        }

        Debug.Log($"Player Spawns: {playerSpawnPoints.Count}");
        Debug.Log($"Enemy Spawns: {enemySpawnPoints.Count}");
    }

    // =========================
    // SPAWN PLAYER
    // =========================

    void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoints.Count == 0)
        {
            Debug.LogWarning("Player spawn failed");
            return;
        }

        Transform spawn = playerSpawnPoints[0];

        GameObject player = Instantiate(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );

        player.tag = "Player";

        Debug.Log("🟦 Player Spawned");
    }

    // =========================
    // SPAWN ENEMIES
    // =========================

    void SpawnEnemies()
    {
        if (turtleEnemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab missing");
            return;
        }

        int count = enemySpawnPoints.Count;

        for (int i = 0; i < count; i++)
        {
            Transform spawn = enemySpawnPoints[i];

            GameObject enemy = Instantiate(
                turtleEnemyPrefab,
                spawn.position,
                spawn.rotation
            );

            enemy.tag = "Enemy";

            Debug.Log($"🐢 Turtle Enemy Spawned {i}");
        }
    }

    // =========================
    // ENEMY PHASE
    // =========================

    protected override IEnumerator EnemyPhase()
    {
        Debug.Log("🐢 Turtle Battle Phase");

        yield return base.EnemyPhase();
    }
}