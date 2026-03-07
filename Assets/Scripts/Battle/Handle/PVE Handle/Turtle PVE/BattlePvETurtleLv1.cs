using System.Collections;
using System.Linq;
using UnityEngine;

public class BattlePvETurtleLv1 : BattleCore
{
    [Header("Controllers")]
    public PlayerBattleController playerController;

    [Header("Settings")]
    public int maxPlayerTurn = 20;
    public float phaseDelay = 1f;

    private int playerTurnCount;
    private bool playerWin;

    private Coroutine phaseRoutine;

    private BattleController controller;

    // =========================
    // ENTRY
    // =========================

    public void BeginBattle(BattleController bc)
    {
        controller = bc;

        Debug.Log("BattlePvETurtleLv1 Begin");

        playerTurnCount = 0;
        playerWin = false;

        SetupTeams();

        StartBattle(BlueTeam, RedTeam, BattleState3D.RedTeamTurn);
    }

    // =========================
    // TEAM SETUP
    // =========================

    void SetupTeams()
    {
        BattleTeamData blue = new BattleTeamData("Player");
        BattleTeamData red = new BattleTeamData("Enemy");

        var participants =
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var p in participants)
        {
            if (p is ITurnParticipant actor)
            {
                if (p.CompareTag("Player"))
                    blue.AddMember(actor);

                else if (p.CompareTag("Enemy"))
                    red.AddMember(actor);
            }
        }

        BlueTeam = blue;
        RedTeam = red;

        Debug.Log($"Team Setup → Player:{BlueTeam.Members.Count} Enemy:{RedTeam.Members.Count}");
    }

    // =========================
    // CORE HOOKS
    // =========================

    protected override IEnumerator OnBattleStartIntro()
    {
        Debug.Log("Battle Intro");

        yield return new WaitForSeconds(0.5f);
    }

    protected override void OnStateEnter(BattleState3D state)
    {
        if (phaseRoutine != null)
            StopCoroutine(phaseRoutine);

        switch (state)
        {
            case BattleState3D.RedTeamTurn:
                phaseRoutine = StartCoroutine(EnemyPhase());
                break;

            case BattleState3D.BlueTeamTurn:
                phaseRoutine = StartCoroutine(PlayerPhase());
                break;

            case BattleState3D.Endbattle:
                Debug.Log("Battle State -> Endbattle");
                break;
        }
    }

    protected override void OnTick(BattleState3D state)
    {
        // optional timer logic
    }

    protected override void OnBattleFinished()
    {
        Debug.Log(playerWin ? "🏆 Victory" : "💀 Defeat");
    }

    // =========================
    // ENEMY PHASE
    // =========================

    IEnumerator EnemyPhase()
    {
        Debug.Log("=== Enemy Turn ===");

        foreach (var enemy in RedTeam.Members.Where(e => e != null && e.IsAlive))
        {
            enemy.TakeTurn();

            yield return new WaitForSeconds(2f);
        }

        if (CheckBattleEnd())
            yield break;

        yield return new WaitForSeconds(phaseDelay);

        SetState(BattleState3D.BlueTeamTurn);
    }

    // =========================
    // PLAYER PHASE
    // =========================

    IEnumerator PlayerPhase()
    {
        playerTurnCount++;

        Debug.Log($"=== Player Turn {playerTurnCount}/{maxPlayerTurn} ===");

        if (playerTurnCount > maxPlayerTurn)
        {
            playerWin = false;
            EndBattle();
            yield break;
        }

        playerController.EnableControl(true);

        while (!isActionDone)
            yield return null;

        playerController.EnableControl(false);

        if (CheckBattleEnd())
            yield break;

        yield return new WaitForSeconds(phaseDelay);

        SetState(BattleState3D.RedTeamTurn);
    }

    // =========================
    // END CONDITIONS
    // =========================

    bool CheckBattleEnd()
    {
        if (RedTeam.IsDefeated)
        {
            playerWin = true;
            EndBattle();
            return true;
        }

        if (BlueTeam.IsDefeated)
        {
            playerWin = false;
            EndBattle();
            return true;
        }

        return false;
    }
}