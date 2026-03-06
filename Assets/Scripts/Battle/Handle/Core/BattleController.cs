using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [Header("References")]
    public BattleHandlerPvE battleHandler;

    [Header("Scene Loading")]
    public string mapSceneName;   // Nhập tên scene map
    public bool loadMapAdditive = true;

    [Header("Settings")]
    public bool enemyGoesFirst = true;

    private bool hasBattleStarted = false;

    private void Update()
    {
        if (!hasBattleStarted && Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(StartGame());
        }
    }

    private IEnumerator StartGame()
    {
        hasBattleStarted = true;

        Debug.Log("🏁 [BattleController] Starting Battle Sequence...");

        // 1️⃣ Load Map Scene (Additive)
        if (loadMapAdditive && !string.IsNullOrEmpty(mapSceneName))
        {
            Scene mapScene = SceneManager.GetSceneByName(mapSceneName);

            // Check nếu scene đã tồn tại
            if (mapScene.isLoaded)
            {
                Debug.Log($"[BattleController] Map scene already loaded: {mapSceneName}");
            }
            else
            {
                Debug.Log($"[BattleController] Loading Map Scene: {mapSceneName}");

                AsyncOperation loadOp = SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);

                while (!loadOp.isDone)
                {
                    yield return null;
                }

                Debug.Log($"[BattleController] Map Loaded Successfully: {mapSceneName}");
            }
        }

        // 2️⃣ Setup Teams
        BattleTeamData playerTeam = new BattleTeamData("Blue Team");
        BattleTeamData enemyTeam = new BattleTeamData("Red Team");

        var allParticipants = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var p in allParticipants)
        {
            if (p is ITurnParticipant participant)
            {
                if (p.CompareTag("Player"))
                {
                    playerTeam.AddMember(participant);
                }
                else if (p.CompareTag("Enemy"))
                {
                    enemyTeam.AddMember(participant);
                }
            }
        }

        Debug.Log($"[BattleController] Participants Found -> Player: {playerTeam.Members.Count}, Enemy: {enemyTeam.Members.Count}");

        // 3️⃣ Start Battle
        if (battleHandler != null)
        {
            battleHandler.StartBattlePVE(playerTeam, enemyTeam);

            Debug.Log($"⚔️ Battle Started! Blue: {playerTeam.Members.Count}, Red: {enemyTeam.Members.Count}");
        }
        else
        {
            Debug.LogError("[BattleController] BattleHandler is missing!");
        }
    }
}