using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq; // Thêm cái này để dùng được .First()

public class LevelSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 4)] public int playerCount = 1;

    [Header("Spawning Positions")]
    public Transform[] playerSpawnPoints;
    public Transform[] playerContainers;

    [Header("Prefabs")]
    public GameObject[] playerPrefabs;
    public GameObject playerCanvasPrefab;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;
    public Transform enemyContainer;

    [Header("Visual Effects")]
    public Action OnSpawningComplete;
    private PlayerUISetup uiSetup;


    private void Awake()
    {
        uiSetup = GetComponent<PlayerUISetup>();
        if (uiSetup == null) uiSetup = gameObject.AddComponent<PlayerUISetup>();
    }

    private void Start()
    {
        // Lần đầu chạy game: Thực hiện chuỗi Intro
        StartCoroutine(FullInitialSpawnSequence());
    }

    // Chuỗi khởi tạo đầu trận (Chỉ gọi 1 lần duy nhất)
    private IEnumerator FullInitialSpawnSequence()
    {
        // 1. Không Spawn Enemy ngay từ đầu, Spawn Enemy được thực hiện trong lượt
        // 2. Spawn Players (ẩn mesh)
        yield return StartCoroutine(SpawnPlayersRoutine(true));

        // 3. Báo cho BattleHandler biết đã spawn player vào vị trí xong để bắt đầu Intro    
        OnSpawningComplete?.Invoke();
    }

    // BattleHandler sẽ gọi hàm này khi Intro kết thúc để Spawn Enemy với hiệu ứng
    public IEnumerator SpawnEnemyRoutine()
    {
        // 1. Spawn Enemy (ẩn đi)
        // 2. Chạy hiệu ứng Spawn Enemy
        // 3. kích hoạt Enemy sau khi hiệu ứng xong
        yield break;
    }

    // FIX LỖI: Thêm bool startActive vào tham số
    private IEnumerator SpawnPlayersRoutine(bool startActive)
    {
        Debug.Log("<color=cyan>🎮 System: Initializing Spawn Sequence...</color>");

        for (int i = 0; i < playerCount; i++)
        {
            if (i >= playerSpawnPoints.Length || i >= playerContainers.Length) continue;

            Transform mapPoint = playerSpawnPoints[i];
            Transform currentContainer = playerContainers[i];
            GameObject selectedPrefab = playerPrefabs[UnityEngine.Random.Range(0, playerPrefabs.Length)];

            GameObject spawnedPlayer = Instantiate(selectedPrefab, mapPoint.position, mapPoint.rotation);

            // GIỮ OBJECT ACTIVE: Để các script khác (Camera/Battle) không bị Null
            spawnedPlayer.SetActive(true);
            spawnedPlayer.transform.SetParent(currentContainer);

            // CHỈ ẨN RENDERER: Để "tàng hình" nhân vật chờ Intro diễn
            var renderers = spawnedPlayer.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = false;

            // KHÓA LOGIC: Player không thể bấm di chuyển/bắn
            PlayerBattleController controller = spawnedPlayer.GetComponent<PlayerBattleController>();
            if (controller != null)
            {
                controller.EnableControl(false);
            }

            // SETUP UI: Tự động ẩn đồng hồ/text theo logic đã viết trong PlayerUISetup
            if (playerCanvasPrefab != null)
            {
                GameObject canvasObj = Instantiate(playerCanvasPrefab, currentContainer);
                uiSetup.SetupUI(spawnedPlayer, canvasObj, i + 1);
            }

            Debug.Log($"✅ {spawnedPlayer.name} initialized. Renderer: DISABLED, Logic: LOCKED.");
            yield return null; // Nghỉ 1 frame để mượt
        }
    }

    // FIX LỖI: Thêm bool startActive vào tham số
    private void SpawnEnemies(bool startActive)
    {
        if (enemyPrefabs.Length == 0 || enemySpawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            // FIX LỖI: UnityEngine.Random để tránh lầm lẫn với System.Random
            GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            enemy.SetActive(startActive);
            if (enemyContainer != null) enemy.transform.SetParent(enemyContainer);
        }
    }
}