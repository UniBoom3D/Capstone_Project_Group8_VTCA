using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BattleController : MonoBehaviour
{
    [Header("References")]
    public BattleHandlerPvE battleHandler;

    private bool hasBattleStarted = false;

    private IEnumerator Start()
    {
        // 1. Đợi LevelSpawner làm việc (Spawn rùa và đặt Tag)
        yield return new WaitForSeconds(0.4f);

        if (!hasBattleStarted)
        {
            PrepareAndStartBattle();
        }
    }

    private void PrepareAndStartBattle()
    {
        hasBattleStarted = true;

        BattleTeamData blue = new BattleTeamData("Blue Team (Players)");
        BattleTeamData red = new BattleTeamData("Red Team (Enemies)");

        var allParticipants = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var p in allParticipants)
        {
            if (p is ITurnParticipant participant)
            {
                if (p.CompareTag("Player")) blue.AddMember(participant);
                else if (p.CompareTag("Enemy")) red.AddMember(participant);
            }
        }

        // Gửi thẳng cho Đạo diễn qua Instance
        if (BattleHandlerPvE.Instance != null)
        {
            // Gọi hàm của Core hoặc hàm PVE nhưng phải gán được vào Blue/Red
            BattleHandlerPvE.Instance.StartBattlePVE(blue, red);
        }
    }
}