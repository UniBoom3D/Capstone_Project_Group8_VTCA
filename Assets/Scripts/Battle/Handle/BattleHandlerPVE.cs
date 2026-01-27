using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    [Header("PLAYER CONTROLLER")]
    [SerializeField] private PlayerBattleController playerBattlerController;

    [Header("RULES")]
    [SerializeField] private int limitedTurnForPlayer = 20;   // Player tối đa 20 lượt (phase xanh)
    [SerializeField] private float phaseDelay = 1f;           // ~1s giữa các phase

    [Header("PLAYER TURN")]
    [SerializeField] private float endTurnAfterShootDelay = 0.1f;

    [Header("CAMERA")]
    [SerializeField] private Camera mainCamera;

    public ITurnParticipant CurrentActor { get; private set; }
    public string currentTeamName;

    private bool awaitingPlayerAction;
    private int playerTurnCount;
    private bool playerWinResult;
    private Coroutine phaseRoutine;

    // =========================
    // Public API
    // =========================
    public void StartBattlePVE(BattleTeamData blueTeam, BattleTeamData redTeam)
    {
        if (isBattleActive) return;

        playerTurnCount = 0;
        playerWinResult = false;

        if (playerBattlerController == null)
            playerBattlerController = FindFirstObjectByType<PlayerBattleController>();

        // Enemy starts first by default in PVE loop
        StartBattle(blueTeam, redTeam, BattleState3D.RedTeamTurn);
    }

    // =========================
    // Core hooks (minimal)
    // =========================
    protected override IEnumerator OnBattleStartIntro()
    {
        currentTeamName = "Start";

        // TODO: SpawnPlayerBlueTeam();
        // TODO: Cinemachine intro sequence

        SetPlayerControl(false);
        HookPlayerShootEvent(false);

        yield return new WaitForSeconds(0.2f);
    }

    protected override void OnStateEnter(BattleState3D state)
    {
        if (phaseRoutine != null) StopCoroutine(phaseRoutine);

        switch (state)
        {
            case BattleState3D.RedTeamTurn:
                phaseRoutine = StartCoroutine(EnemyPhase());
                break;

            case BattleState3D.BlueTeamTurn:
                phaseRoutine = StartCoroutine(PlayerPhase());
                break;

            case BattleState3D.Endbattle:
                Cleanup();
                break;
        }
    }

    protected override void OnTick(BattleState3D state)
    {
        // PvE chung: không xử lý logic cụ thể tại đây (để ChallengeTurtle làm).
        // Chỉ giữ core running.
    }

    protected override void OnBattleFinished()
    {
        Debug.Log(playerWinResult ? "[BATTLE] Victory (Blue)" : "[BATTLE] Defeat (Red)");
        Cleanup();
    }

    // =========================
    // Phases
    // =========================
    private IEnumerator EnemyPhase()
    {
        if (!isBattleActive) yield break;

        currentTeamName = RedTeam.TeamName;
        Debug.Log("=== Enemy turn ===");

        // Focus camera: enemy gần nhất so với player mục tiêu
        var playerTarget = GetFirstAliveTransform(BlueTeam);
        var enemyFocus = GetNearestEnemyToTarget(playerTarget);
        FocusCamera(enemyFocus);

        // TODO: EnemyTeamAction_AllAtOnce()
        // - Spawn enemy (nếu cần)
        // - Move
        // - Attack
        // (all in one phase)
        // Debug/log trong ChallengeTurtle

        yield return null;

        // Win check
        if (RedTeam.IsDefeated) { playerWinResult = true; EndBattle(); yield break; }
        if (BlueTeam.IsDefeated) { playerWinResult = false; EndBattle(); yield break; }

        if (phaseDelay > 0f) yield return new WaitForSeconds(phaseDelay);
        SetState(BattleState3D.BlueTeamTurn);
    }

    private IEnumerator PlayerPhase()
    {
        if (!isBattleActive) yield break;

        currentTeamName = BlueTeam.TeamName;
        playerTurnCount++;

        Debug.Log($"=== Player turn {playerTurnCount}/{limitedTurnForPlayer} ===");

        // Turn limit rule
        if (playerTurnCount > limitedTurnForPlayer && !RedTeam.IsDefeated)
        {
            playerWinResult = false;
            EndBattle();
            yield break;
        }

        foreach (var actor in BlueTeam.Members.Where(m => m != null && m.IsAlive))
        {
            if (!isBattleActive) yield break;

            CurrentActor = actor;

            Debug.Log($"👉 It is now {actor.Name}'s Turn!");

            FocusCamera(GetPlayerCameraFollow(actor));

            isActionDone = false;
            awaitingPlayerAction = true;
            bool cheatUsed = false; // Prevents pressing O multiple times

            HookPlayerShootEvent(true);
            SetPlayerControl(true);

            // --- WAITING LOOP ---
            while (!isActionDone && isBattleActive)
            {
                // 🛠️ CHEAT: Press 'O' to skip turn in 5 seconds
                if (!cheatUsed && Input.GetKeyDown(KeyCode.O))
                {
                    cheatUsed = true;
                    Debug.Log("⏳ Cheat Activated: Ending turn in 5 seconds...");

                    // 1. Lock controls immediately so you can't shoot/move
                    SetPlayerControl(false);
                    HookPlayerShootEvent(false);
                    awaitingPlayerAction = false;

                    // 2. Start the 5-second countdown to force end the turn
                    StartCoroutine(EndUnitTurnAfterDelay(5f));
                }

                yield return null;
            }
            // -------------------

            // Cleanup (safe to call even if cheat handled it already)
            SetPlayerControl(false);
            HookPlayerShootEvent(false);
            awaitingPlayerAction = false;

            CurrentActor = null;

            if (RedTeam.IsDefeated) { playerWinResult = true; EndBattle(); yield break; }
            if (BlueTeam.IsDefeated) { playerWinResult = false; EndBattle(); yield break; }
        }

        if (phaseDelay > 0f) yield return new WaitForSeconds(phaseDelay);
        SetState(BattleState3D.RedTeamTurn);
    }
    // =========================
    // Shoot hook -> end unit turn
    // =========================
    private void HookPlayerShootEvent(bool hook)
    {
        if (playerBattlerController == null) return;

        playerBattlerController.OnShoot -= OnPlayerShoot;
        if (hook) playerBattlerController.OnShoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(Projectile projectile)
    {
        if (!awaitingPlayerAction) return;

        awaitingPlayerAction = false;
        StartCoroutine(EndUnitTurnAfterDelay(endTurnAfterShootDelay));
    }

    private IEnumerator EndUnitTurnAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        MarkActionDone(); // core API
    }

    // =========================
    // Camera helpers
    // =========================
    private void FocusCamera(Transform follow)
    {
        if (mainCamera == null || follow == null) return;

        // TODO: chuyển sang Cinemachine follow/lookAt khi bạn setup xong
        mainCamera.transform.position = follow.position;
        mainCamera.transform.LookAt(follow);
    }

    private Transform GetPlayerCameraFollow(ITurnParticipant actor)
    {
        if (actor is not MonoBehaviour mono) return null;

        var t = mono.transform.Find("CameraFollowPlayer");
        return t != null ? t : mono.transform;
    }

    private Transform GetFirstAliveTransform(BattleTeamData team)
    {
        if (team?.Members == null) return null;

        var alive = team.Members.FirstOrDefault(m => m != null && m.IsAlive);
        return alive is MonoBehaviour mb ? mb.transform : null;
    }

    private Transform GetNearestEnemyToTarget(Transform target)
    {
        if (RedTeam?.Members == null) return null;

        // fallback: enemy sống đầu tiên
        if (target == null)
        {
            var first = RedTeam.Members.FirstOrDefault(m => m != null && m.IsAlive);
            return first is MonoBehaviour fb ? fb.transform : null;
        }

        float best = float.MaxValue;
        Transform bestTf = null;

        foreach (var e in RedTeam.Members.Where(m => m != null && m.IsAlive))
        {
            if (e is not MonoBehaviour mono) continue;

            float d = Vector3.Distance(mono.transform.position, target.position);
            if (d < best)
            {
                best = d;
                bestTf = mono.transform;
            }
        }

        return bestTf;
    }

    private void SetPlayerControl(bool enable)
    {
        if (playerBattlerController == null) return;
        playerBattlerController.EnableControl(enable);
    }

    private void Cleanup()
    {
        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
        CurrentActor = null;

        if (phaseRoutine != null)
        {
            StopCoroutine(phaseRoutine);
            phaseRoutine = null;
        }
    }
}
