using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    [Header("TEAMS (DYNAMIC)")]
    [Tooltip("Managed by BattleManager script.")]
    public List<BattleTeamData> battleTeams = new List<BattleTeamData>();
    private int currentTeamIndex = 0;

    [Header("ACTIVE ACTOR")]
    private PlayerBattleController _activePlayerInTurn;

    [Header("RULES")]
    [SerializeField] private int limitedTurnForPlayer = 20;
    [SerializeField] private float phaseDelay = 1f;
    [SerializeField] private float aiTurnWaitTime = 3.0f;

    [Header("PLAYER TURN CONFIG")]
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
    public void StartBattlePVE(List<BattleTeamData> teamsInBattle)
    {
        if (isBattleActive) return;

        battleTeams = teamsInBattle;
        playerTurnCount = 0;
        playerWinResult = false;
        currentTeamIndex = 0;

        // Tắt toàn bộ UI của tất cả Player trước khi vào trận
        foreach (var team in battleTeams)
        {
            foreach (var member in team.Members)
            {
                if (member is PlayerBattleController p && p.turnTimer != null)
                {
                    p.turnTimer.gameObject.SetActive(false);
                }
            }
        }

        isBattleActive = true;
        StartCoroutine(OnBattleStartIntro());
        phaseRoutine = StartCoroutine(MasterBattleLoop());
    }

    protected override IEnumerator OnBattleStartIntro()
    {
        currentTeamName = "Start";
        yield return new WaitForSeconds(0.2f);
    }

    protected override void OnStateEnter(BattleState3D state)
    {
        if (state == BattleState3D.Endbattle) Cleanup();
    }

    protected override void OnTick(BattleState3D state) { }

    protected override void OnBattleFinished()
    {
        Debug.Log(playerWinResult ? "[BATTLE] Victory (Player Team)" : "[BATTLE] Defeat");
        Cleanup();
    }

    // =========================
    // 🟢 DYNAMIC MASTER LOOP
    // =========================
    private IEnumerator MasterBattleLoop()
    {
        yield return new WaitForSeconds(0.2f);

        while (isBattleActive)
        {
            var currentTeam = battleTeams[currentTeamIndex];
            currentTeamName = currentTeam.TeamName;

            bool isPlayerTeam = currentTeam.Members.Any(m => m is PlayerBattleController);
            if (isPlayerTeam)
            {
                playerTurnCount++;
                if (playerTurnCount > limitedTurnForPlayer)
                {
                    playerWinResult = false;
                    EndBattle();
                    yield break;
                }
            }

            foreach (var actor in currentTeam.Members.Where(m => m != null && m.IsAlive))
            {
                if (!isBattleActive) yield break;

                CurrentActor = actor;
                if (actor is MonoBehaviour monoActor)
                    FocusCamera(monoActor.transform);

                // Xác định PlayerTarget
                PlayerBattleController playerTarget = actor as PlayerBattleController;
                if (playerTarget == null && actor is MonoBehaviour m)
                    playerTarget = m.GetComponent<PlayerBattleController>();

                if (playerTarget != null)
                {
                    yield return StartCoroutine(HandlePlayerTurn(playerTarget));
                }
                else
                {
                    yield return StartCoroutine(HandleAITurn(actor));
                }

                if (CheckBattleEndCondition()) yield break;
            }

            if (phaseDelay > 0f) yield return new WaitForSeconds(phaseDelay);
            currentTeamIndex = (currentTeamIndex + 1) % battleTeams.Count;
        }
    }

    // =========================
    // Turn Logic Handlers
    // =========================
    private IEnumerator HandlePlayerTurn(PlayerBattleController player)
    {
        isActionDone = false;
        awaitingPlayerAction = true;
        _activePlayerInTurn = player;

        // Kích hoạt Timer riêng của nhân vật
        if (_activePlayerInTurn.turnTimer != null)
        {
            _activePlayerInTurn.turnTimer.gameObject.SetActive(true);
            _activePlayerInTurn.turnTimer.StartNewTurn();
        }

        HookPlayerShootEvent(true);
        _activePlayerInTurn.EnableControl(true);

        while (!isActionDone && isBattleActive)
        {
            // Logic Cheat phím O
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("⏳ Cheat: Ending turn in 5s...");
                _activePlayerInTurn.EnableControl(false);
                HookPlayerShootEvent(false);
                awaitingPlayerAction = false;
                StartCoroutine(EndUnitTurnAfterDelay(5f));
                break;
            }
            yield return null;
        }

        // Dọn dẹp sau khi xong lượt
        if (_activePlayerInTurn != null)
        {
            if (_activePlayerInTurn.turnTimer != null)
            {
                _activePlayerInTurn.turnTimer.StopTimer();
                _activePlayerInTurn.turnTimer.gameObject.SetActive(false);
            }
            _activePlayerInTurn.EnableControl(false);
        }

        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
        _activePlayerInTurn = null;
    }

    public void ForceEndTurn()
    {
        if (awaitingPlayerAction && isBattleActive && _activePlayerInTurn != null)
        {
            Debug.Log($"⏰ [TIMEOUT] {_activePlayerInTurn.name} hết giờ!");
            _activePlayerInTurn.EnableControl(false);
            if (_activePlayerInTurn.turnTimer != null) _activePlayerInTurn.turnTimer.StopTimer();

            awaitingPlayerAction = false;
            isActionDone = true;
        }
    }

    private IEnumerator HandleAITurn(ITurnParticipant aiActor)
    {
        aiActor.TakeTurn();
        yield return new WaitForSeconds(aiTurnWaitTime);
        CurrentActor = null;
    }

    private bool CheckBattleEndCondition()
    {
        int activeTeamsCount = battleTeams.Count(t => !t.IsDefeated);
        BattleTeamData winnerTeam = battleTeams.FirstOrDefault(t => !t.IsDefeated);

        if (activeTeamsCount <= 1)
        {
            playerWinResult = winnerTeam != null && winnerTeam.Members.Any(m => m is PlayerBattleController);
            EndBattle();
            return true;
        }
        return false;
    }

    // =========================
    // Shoot hook
    // =========================
    private void HookPlayerShootEvent(bool hook)
    {
        if (_activePlayerInTurn == null) return;
        _activePlayerInTurn.OnShoot -= OnPlayerShoot;
        if (hook) _activePlayerInTurn.OnShoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(Projectile projectile)
    {
        if (!awaitingPlayerAction) return;

        if (_activePlayerInTurn != null && _activePlayerInTurn.turnTimer != null)
            _activePlayerInTurn.turnTimer.StopTimer();

        awaitingPlayerAction = false;

        CameraFollowPlayer camControl = Object.FindFirstObjectByType<CameraFollowPlayer>();
        if (camControl != null && projectile != null)
        {
            camControl.SetProjectileTarget(projectile.transform);
        }
        StartCoroutine(WaitUntilProjectileDestroyed(projectile));
    }

    private IEnumerator EndUnitTurnAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        isActionDone = true;
    }
    private IEnumerator WaitUntilProjectileDestroyed(Projectile projectile)
    {
        while (projectile != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        // Xác nhận hành động đã xong để Master Loop chuyển sang Actor tiếp theo
        isActionDone = true;
    }

    // =========================
    // Helpers
    // =========================
    private void FocusCamera(Transform follow)
    {
        //if (mainCamera == null || follow == null) return;
        //mainCamera.transform.position = follow.position - (follow.forward * 5f) + (Vector3.up * 3f);
        //mainCamera.transform.LookAt(follow);
        CameraFollowPlayer camControl = Object.FindFirstObjectByType<CameraFollowPlayer>();
        if (camControl != null)
        {
            camControl.SetTarget(follow);
        }
    }

    private void Cleanup()
    {
        if (_activePlayerInTurn != null) _activePlayerInTurn.EnableControl(false);
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