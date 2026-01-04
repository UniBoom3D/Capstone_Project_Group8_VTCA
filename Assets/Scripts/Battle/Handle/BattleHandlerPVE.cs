using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

public class BattleHandlerPVE : BattleHandler_TurnBaseCore
{
    [Header("PLAYER CONTROLLER (External)")]
    [SerializeField] private PlayerBattleController playerController; // class bạn đưa

    [Header("START ORDER")]
    [SerializeField] private bool enemyRedStarts = true;

    [Header("TEAM PHASE SETTINGS")]
    [SerializeField] private float perUnitTimeLimitSeconds = 20f; // time limit cho mỗi member
    [SerializeField] private float betweenUnitDelay = 0.12f;
    [SerializeField] private float betweenTeamDelay = 0.2f;

    [Header("PLAYER TURN END RULE")]
    [Tooltip("Sau khi bắn (OnShoot), chờ thêm chút rồi mới end unit turn (tránh end quá gấp).")]
    [SerializeField] private float endTurnAfterShootDelay = 0.1f;

    [Header("AI FALLBACK (if enemy has no AI controller)")]
    [SerializeField] private float aiThinkDelay = 0.25f;
    [SerializeField] private float aiAfterActionDelay = 0.2f;

    // Runtime
    public ITurnParticipant CurrentActor { get; private set; }
    private Coroutine battleRoutine;

    // Flag để tránh double-end turn nếu OnShoot bị gọi nhiều lần
    private bool awaitingPlayerAction;

    protected virtual void Awake()
    {
        if (playerController == null) playerController = FindFirstObjectByType<PlayerBattleController>();
    }

    #region Public API

    public void StartBattlePVE(BattleTeamData blueTeam, BattleTeamData redTeam)
    {
        if (isBattleActive) return;

        BlueTeam = blueTeam;
        RedTeam = redTeam;

        isBattleActive = true;
        currentState = BattleState3D.Start;

        if (battleRoutine != null) StopCoroutine(battleRoutine);
        battleRoutine = StartCoroutine(BattleLoop());
    }

    public void NotifyActionDone()
    {
        isPlayerActionDone = true;
    }

    #endregion

    #region Loop

    private IEnumerator BattleLoop()
    {
        yield return HandleStart();

        currentState = enemyRedStarts ? BattleState3D.RedTeamTurn : BattleState3D.BlueTeamTurn;

        while (isBattleActive)
        {
            if (CheckBattleFinished(out bool playerWin))
            {
                yield return HandleFinish(playerWin);
                yield break;
            }

            if (currentState == BattleState3D.RedTeamTurn)
            {
                yield return HandleTeamPhase(
                    actingTeam: RedTeam,
                    enemyTeam: BlueTeam,
                    isPlayerTeam: false
                );

                currentState = BattleState3D.BlueTeamTurn;
            }
            else // BlueTeamTurn
            {
                yield return HandleTeamPhase(
                    actingTeam: BlueTeam,
                    enemyTeam: RedTeam,
                    isPlayerTeam: true
                );

                currentState = BattleState3D.RedTeamTurn;
            }

            if (betweenTeamDelay > 0f)
                yield return new WaitForSeconds(betweenTeamDelay);
        }
    }

    private IEnumerator HandleStart()
    {
        currentTeamName = "Init";
        SetActiveCamera(_startCamera);

        // đảm bảo player bị khóa trước khi battle bắt đầu
        SetPlayerControl(false);

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator HandleTeamPhase(BattleTeamData actingTeam, BattleTeamData enemyTeam, bool isPlayerTeam)
    {
        if (actingTeam == null || enemyTeam == null) yield break;

        currentTeamName = actingTeam.TeamName;

        SetActiveCamera(isPlayerTeam ? _blueTeamCamera : _redTeamCamera);

        // Snapshot danh sách sống tại đầu phase
        List<ITurnParticipant> phaseOrder = actingTeam.Members
            .Where(m => m != null && m.IsAlive)
            .ToList();

        if (phaseOrder.Count == 0) yield break;

        foreach (var actor in phaseOrder)
        {
            if (!isBattleActive) yield break;
            if (CheckBattleFinished(out _)) yield break;
            if (actor == null || !actor.IsAlive) continue;

            CurrentActor = actor;

            // reset per-unit
            isPlayerActionDone = false;
            turnTimer = 0f;

            Debug.Log($"--- [{actingTeam.TeamName}] {actor.Name}'s Turn ---");

            if (isPlayerTeam)
            {
                // ✅ Player unit turn: bật điều khiển và đợi OnShoot để kết thúc
                yield return HandlePlayerUnitTurn(actor, actingTeam, enemyTeam);
            }
            else
            {
                // ✅ Enemy unit turn: đơn giản gọi TakeTurn() (sau này thay bằng enemy controller/AI)
                yield return HandleEnemyUnitTurn(actor, actingTeam, enemyTeam);
            }

            if (betweenUnitDelay > 0f)
                yield return new WaitForSeconds(betweenUnitDelay);
        }

        // End of phase cleanup
        if (isPlayerTeam) SetPlayerControl(false);

        CurrentActor = null;
    }

    private IEnumerator HandlePlayerUnitTurn(ITurnParticipant actor, BattleTeamData myTeam, BattleTeamData enemyTeam)
    {
        // Quy ước hiện tại: playerController điều khiển 1 nhân vật player.
        // Nếu BlueTeam có nhiều nhân vật player sau này, bạn sẽ cần mapping actor -> controller tương ứng.
        // Hiện tại handler chỉ xử lý “đến lượt player thì bật control và chờ bắn”.

        awaitingPlayerAction = true;

        HookPlayerShootEvent(true);
        SetPlayerControl(true);

        // Nếu bạn muốn actor.TakeTurn() tự set UI/stance thì gọi ở đây cũng được,
        // nhưng vì controller do người khác xử lý, tạm không can thiệp:
        // actor.TakeTurn();

        while (!isPlayerActionDone)
        {
            turnTimer += Time.deltaTime;

            if (perUnitTimeLimitSeconds > 0f && turnTimer >= perUnitTimeLimitSeconds)
            {
                Debug.LogWarning($"[TURN TIMEOUT] Player unit {actor.Name} timed out. Force end.");
                ForceEndPlayerUnitTurn();
                break;
            }

            if (CheckBattleFinished(out _))
                break;

            yield return null;
        }

        // Cleanup
        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
    }

    private IEnumerator HandleEnemyUnitTurn(ITurnParticipant actor, BattleTeamData myTeam, BattleTeamData enemyTeam)
    {
        // Fallback AI trong handler
        yield return new WaitForSeconds(aiThinkDelay);

        actor.TakeTurn();

        yield return new WaitForSeconds(aiAfterActionDelay);

        NotifyActionDone();

        // Nếu sau này enemy action có anim, bạn sẽ chuyển sang:
        // enemyController sẽ gọi NotifyActionDone() khi anim xong.
        yield break;
    }

    private IEnumerator HandleFinish(bool playerWin)
    {
        currentState = BattleState3D.Finish;
        isBattleActive = false;

        SetPlayerControl(false);
        HookPlayerShootEvent(false);

        SetActiveCamera(_startCamera);

        Debug.Log(playerWin ? "[BATTLE] Victory" : "[BATTLE] Defeat");
        yield return null;
    }

    #endregion

    #region Player Shoot Hook

    private void HookPlayerShootEvent(bool hook)
    {
        if (playerController == null) return;

        // tránh double subscribe
        playerController.OnShoot -= OnPlayerShoot;
        if (hook) playerController.OnShoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(Projectile projectile)
    {
        // Chỉ xử lý nếu đang “đợi action” trong player unit turn
        if (!awaitingPlayerAction) return;

        // nếu projectile bắn nhiều lần spam, chỉ ăn 1 lần
        awaitingPlayerAction = false;

        // Kết thúc lượt sau 1 chút (để controller kịp spawn projectile)
        StartCoroutine(EndTurnAfterDelay(endTurnAfterShootDelay));
    }

    private IEnumerator EndTurnAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        NotifyActionDone();
    }

    private void ForceEndPlayerUnitTurn()
    {
        awaitingPlayerAction = false;
        NotifyActionDone();
    }

    private void SetPlayerControl(bool enable)
    {
        if (playerController == null) return;
        playerController.EnableControl(enable);
    }

    #endregion

    #region Helpers

    private bool CheckBattleFinished(out bool playerWin)
    {
        playerWin = false;
        if (BlueTeam == null || RedTeam == null) return false;

        bool blueDefeated = BlueTeam.IsDefeated;
        bool redDefeated = RedTeam.IsDefeated;

        if (blueDefeated && redDefeated)
        {
            playerWin = false;
            return true;
        }

        if (blueDefeated) { playerWin = false; return true; }
        if (redDefeated) { playerWin = true; return true; }

        return false;
    }

    private void SetActiveCamera(CinemachineCamera cam)
    {
        if (_startCamera != null) _startCamera.gameObject.SetActive(cam == _startCamera);
        if (_blueTeamCamera != null) _blueTeamCamera.gameObject.SetActive(cam == _blueTeamCamera);
        if (_redTeamCamera != null) _redTeamCamera.gameObject.SetActive(cam == _redTeamCamera);
    }

    #endregion
}
