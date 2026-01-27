using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    public BattleHandlerPvE battleHandler;

    [Header("Settings")]
    public bool enemyGoesFirst = true;

    private bool hasBattleStarted = false; // Prevents starting twice

    private void Update()
    {
        // Only start if we haven't started yet AND player presses Enter
        if (!hasBattleStarted && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        hasBattleStarted = true;
        Debug.Log("🏁 Starting Battle Sequence...");

        // 1. Setup the Teams
        BattleTeamData playerTeam = new BattleTeamData("Blue Team");
        BattleTeamData enemyTeam = new BattleTeamData("Red Team");

        // 2. Find everyone in the scene automatically
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

        // 3. Start the Battle!
        if (battleHandler != null)
        {
            battleHandler.StartBattlePVE(playerTeam, enemyTeam);
            Debug.Log($"Battle Started! Blue: {playerTeam.Members.Count}, Red: {enemyTeam.Members.Count}");
        }
    }
}