using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    [Header("PLAYER CONTROLLER (External)")]
    [SerializeField] private PlayerBattleController playerBattlerController;

    [Header("START ORDER")]
    [SerializeField] private bool enemyRedStarts = true;

    [Header("TEAM PHASE SETTINGS")]
    [SerializeField] private float perUnitTimeLimitSeconds = 20f;
    [SerializeField] private float betweenUnitDelay = 0.12f;
    [SerializeField] private float betweenTeamDelay = 0.2f;

    [Header("PLAYER TURN END RULE")]
    [SerializeField] private float endTurnAfterShootDelay = 0.1f;

    [Header("AI FALLBACK")]
    [SerializeField] private float aiThinkDelay = 0.25f;
    [SerializeField] private float aiAfterActionDelay = 0.2f;

    [Header("CAMERA FOLLOW SETTINGS")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cameraFollow;

    public ITurnParticipant CurrentActor { get; private set; }
    private bool awaitingPlayerAction;
    public string currentTeamName;

    protected override void Awake()
    {
        base.Awake();
        if (playerBattlerController == null) playerBattlerController = FindFirstObjectByType<PlayerBattleController>();
    }

    #region Public API

    public void StartBattlePVE(BattleTeamData blueTeam, BattleTeamData redTeam)
    {
        if (isBattleActive) return;

        BlueTeam = blueTeam;
        RedTeam = redTeam;

        isBattleActive = true;
        currentState = BattleState3D.Start;
        StartCoroutine(BattleLoop());
    }

    public void NotifyActionDone()
    {
        isActionDone = true;
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
                yield return HandleTeamPhase(RedTeam, BlueTeam, false);
                currentState = BattleState3D.BlueTeamTurn;
            }
            else
            {
                yield return HandleTeamPhase(BlueTeam, RedTeam, true);
                currentState = BattleState3D.RedTeamTurn;
            }

            if (betweenTeamDelay > 0f)
                yield return new WaitForSeconds(betweenTeamDelay);
        }
    }

    private IEnumerator HandleStart()
    {
        currentTeamName = "Init";
        yield return new WaitForSeconds(0.2f);
        SetPlayerControl(false);
    }

    private IEnumerator HandleTeamPhase(BattleTeamData actingTeam, BattleTeamData enemyTeam, bool isPlayerTeam)
    {
        if (actingTeam == null || enemyTeam == null) yield break;

        currentTeamName = actingTeam.TeamName;

        if (isPlayerTeam)
            SetPlayerCameraFollow(true);
        else
            SetEnemyCameraFollow();

        if (isPlayerTeam)
        {
            foreach (var actor in actingTeam.Members.Where(m => m.IsAlive))
            {
                if (!isBattleActive) yield break;
                if (CheckBattleFinished(out _)) yield break;

                CurrentActor = actor;
                yield return HandlePlayerUnitTurn(actor);
            }
        }
        else
        {
            yield return HandleEnemyUnitTurn(actingTeam);
        }

        if (isPlayerTeam) SetPlayerControl(false);
        CurrentActor = null;
    }

    private IEnumerator HandlePlayerUnitTurn(ITurnParticipant actor)
    {
        awaitingPlayerAction = true;
        HookPlayerShootEvent(true);
        SetPlayerControl(true);

        while (!isActionDone)
        {
            turnTimer += Time.deltaTime;

            if (perUnitTimeLimitSeconds > 0f && turnTimer >= perUnitTimeLimitSeconds)
            {
                Debug.LogWarning($"[TURN TIMEOUT] Player unit {actor.Name} timed out.");
                ForceEndPlayerUnitTurn();
                break;
            }

            if (CheckBattleFinished(out _)) break;
            yield return null;
        }

        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
    }

    private IEnumerator HandleEnemyUnitTurn(BattleTeamData actingTeam)
    {
        yield return new WaitForSeconds(aiThinkDelay);

        foreach (var enemy in actingTeam.Members.Where(m => m.IsAlive))
        {
            enemy.TakeTurn();
            yield return new WaitForSeconds(aiAfterActionDelay);
        }

        NotifyActionDone();
        yield break;
    }

    private IEnumerator HandleFinish(bool playerWin)
    {
        currentState = BattleState3D.Endbattle;
        isBattleActive = false;
        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        Debug.Log(playerWin ? "[BATTLE] Victory" : "[BATTLE] Defeat");
        yield return null;
    }

    #endregion

    #region Helpers

    private void SetPlayerCameraFollow(bool enable)
    {
        if (enable)
        {
            // FIX: Check if actor is a MonoBehaviour to get Transform
            Transform targetTransform = null;
            if (CurrentActor is MonoBehaviour mono)
            {
                targetTransform = mono.transform;
            }

            cameraFollow = targetTransform != null ? targetTransform : cameraFollow;

            if (cameraFollow != null && mainCamera != null)
            {
                mainCamera.transform.position = cameraFollow.position;
                mainCamera.transform.LookAt(cameraFollow);
            }
        }
    }

    private void SetEnemyCameraFollow()
    {
        // FIX: Check if actor is a MonoBehaviour to get Transform
        var aliveEnemy = RedTeam.Members.FirstOrDefault(m => m.IsAlive);
        if (aliveEnemy is MonoBehaviour enemyMono)
        {
            cameraFollow = enemyMono.transform;
            if (mainCamera != null)
            {
                mainCamera.transform.position = cameraFollow.position;
                mainCamera.transform.LookAt(cameraFollow);
            }
        }
    }

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
        if (playerBattlerController == null) return;
        playerBattlerController.EnableControl(enable);
    }

    private bool CheckBattleFinished(out bool playerWin)
    {
        playerWin = false;
        if (BlueTeam == null || RedTeam == null) return false;
        if (BlueTeam.IsDefeated && RedTeam.IsDefeated) return true;
        if (BlueTeam.IsDefeated) { playerWin = false; return true; }
        if (RedTeam.IsDefeated) { playerWin = true; return true; }
        return false;
    }

    protected override void OnTick(BattleState3D state) { }

    #endregion
}