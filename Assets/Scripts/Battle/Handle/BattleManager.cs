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

        // 1. Setup the Base Teams
        BattleTeamData playerTeam = new BattleTeamData("Blue Team");
        BattleTeamData enemyTeam = new BattleTeamData("Red Team");

        // 2. Find everyone in the scene automatically
        var allParticipants = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var p in allParticipants)
        {
            if (p is ITurnParticipant participant)
            {
                // Put players in the Blue Team
                if (p.CompareTag("Player"))
                {
                    playerTeam.AddMember(participant);
                }
                // Put enemies in the Red Team
                else if (p.CompareTag("Enemy"))
                {
                    enemyTeam.AddMember(participant);
                }
            }
        }

        // 3. Pack them into a List
        List<BattleTeamData> teamsInBattle = new List<BattleTeamData>();

        if (enemyGoesFirst)
        {
            teamsInBattle.Add(enemyTeam);
            teamsInBattle.Add(playerTeam);
        }
        else
        {
            teamsInBattle.Add(playerTeam);
            teamsInBattle.Add(enemyTeam);
        }

        // 🟢 NEW: Print detailed Team Information to the Console
        Debug.Log("====================================");
        Debug.Log("📋 BATTLE ROSTER:");
        foreach (var team in teamsInBattle)
        {
            Debug.Log($"🛡️ {team.TeamName} (Total Members: {team.Members.Count})");
            foreach (var member in team.Members)
            {
                // We can pull the Name and HP directly from the ITurnParticipant interface!
                Debug.Log($"   -> {member.Name} | HP: {member.HP}");
            }
        }
        Debug.Log("====================================");

        // 4. Start the Battle with the List!
        if (battleHandler != null)
        {
            battleHandler.StartBattlePVE(teamsInBattle);
        }
        else
        {
            Debug.LogError("❌ BattleManager is missing the BattleHandlerPvE reference! Drag it into the Inspector.");
        }
    }
}