using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    [Header("PLAYER CONTROLLER (External)")]
    [SerializeField] private PlayerBattleController playerController;

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

    [Header("CAMERA FOLLOW SETTINGS")]
    [SerializeField] private Camera mainCamera;  // The main camera for the scene
    [SerializeField] private Transform cameraFollow;  // Camera follow target

    // Runtime state
    public ITurnParticipant CurrentActor { get; private set; }
    private bool awaitingPlayerAction;

    public string currentTeamName; // Add this line to keep track of the current team's name

    protected override void Awake()
    {
        base.Awake();
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

        // Start battle routine
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
                yield return HandleTeamPhase(RedTeam, BlueTeam, false); // Enemy team's turn
                currentState = BattleState3D.BlueTeamTurn;
            }
            else // BlueTeamTurn
            {
                yield return HandleTeamPhase(BlueTeam, RedTeam, true); // Player team's turn
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

        // Lock player control before battle starts
        SetPlayerControl(false);
    }

    private IEnumerator HandleTeamPhase(BattleTeamData actingTeam, BattleTeamData enemyTeam, bool isPlayerTeam)
    {
        if (actingTeam == null || enemyTeam == null) yield break;

        currentTeamName = actingTeam.TeamName;

        // Update camera follow target based on the current team
        if (isPlayerTeam)
        {
            // Player team: Set camera to follow each individual player unit
            SetPlayerCameraFollow(true);
        }
        else
        {
            // Enemy team: Set camera to follow the enemy team as a whole
            SetEnemyCameraFollow();
        }

        // Process turns for each team member
        if (isPlayerTeam)
        {
            // BlueTeam (Player team) - Individual turns
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
            // RedTeam (Enemy team) - All units move and attack together
            yield return HandleEnemyUnitTurn(actingTeam);
        }

        // End of phase cleanup
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

    private IEnumerator HandleEnemyUnitTurn(BattleTeamData actingTeam)
    {
        // Fallback AI in handler (replace with AI logic later)
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

    #region Player Shoot Hook

    private void HookPlayerShootEvent(bool hook)
    {
        if (playerController == null) return;

        // Avoid double subscription
        playerController.OnShoot -= OnPlayerShoot;
        if (hook) playerController.OnShoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(Projectile projectile)
    {
        // Only process if waiting for player action during unit turn
        if (!awaitingPlayerAction) return;

        // Prevent double actions if projectile shoots many times
        awaitingPlayerAction = false;

        // End turn after a brief delay (to let controller spawn the projectile)
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
        if (cam != null) cam.Priority = 20;
    }

    private void SetPlayerCameraFollow(bool enable)
    {
        if (enable)
        {
            // Set the camera to follow the player (the currently active unit in BlueTeam)
            cameraFollow = CurrentActor != null ? CurrentActor.transform : cameraFollow;
            mainCamera.transform.position = cameraFollow.position;
            mainCamera.transform.LookAt(cameraFollow);
        }
    }

    private void SetEnemyCameraFollow()
    {
        // Set the camera to follow the enemy team as a whole
        cameraFollow = RedTeam.Members.FirstOrDefault(m => m.IsAlive)?.transform;
        if (cameraFollow != null)
        {
            mainCamera.transform.position = cameraFollow.position;
            mainCamera.transform.LookAt(cameraFollow);
        }
    }

    #endregion

    #region OnTick Implementation

    protected override void OnTick(BattleState3D state)
    {
        // Handle game logic for each state
        switch (state)
        {
            case BattleState3D.BlueTeamTurn:
                // Handle BlueTeam's turn (could be a more specific logic here)
                break;
            case BattleState3D.RedTeamTurn:
                // Handle RedTeam's turn (could be a more specific logic here)
                break;
            case BattleState3D.Endbattle:
                // Handle end of battle logic (could be a more specific logic here)
                break;
                // Handle other states if needed
        }
    }

    #endregion
}
