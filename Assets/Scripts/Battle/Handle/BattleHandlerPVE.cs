using System.Collections;
using System.Collections.Generic; // REQUIRED for Lists
using System.Linq;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    [Header("TEAMS (DYNAMIC)")]
    [Tooltip("Add as many teams here as you want. They will take turns in order.")]
    public List<BattleTeamData> battleTeams = new List<BattleTeamData>();
    private int currentTeamIndex = 0;

    [Header("PLAYER CONTROLLER")]
    [SerializeField] private PlayerBattleController playerBattlerController;

    [Header("RULES")]
    [SerializeField] private int limitedTurnForPlayer = 20;
    [SerializeField] private float phaseDelay = 1f;
    [SerializeField] private float aiTurnWaitTime = 3.0f; // Time allocated for AI animations

    [Header("PLAYER TURN")]
    [SerializeField] private float endTurnAfterShootDelay = 0.1f;

    [Header("UI REFERENCES")]
    [SerializeField] private Timer countdownTimer;

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
    /// <summary>
    /// Call this with ANY number of teams to start a battle!
    /// </summary>
    public void StartBattlePVE(List<BattleTeamData> teamsInBattle)
    {
        if (isBattleActive) return;

        battleTeams = teamsInBattle;
        playerTurnCount = 0;
        playerWinResult = false;
        currentTeamIndex = 0;

        if (playerBattlerController == null)
            playerBattlerController = FindFirstObjectByType<PlayerBattleController>();

        // Set base variables
        isBattleActive = true;

        // Start the master dynamic loop instead of the hardcoded Red/Blue states
        StartCoroutine(OnBattleStartIntro());
        phaseRoutine = StartCoroutine(MasterBattleLoop());
    }

    // =========================
    // Core hooks
    // =========================
    protected override IEnumerator OnBattleStartIntro()
    {
        currentTeamName = "Start";
        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        yield return new WaitForSeconds(0.2f);
    }

    protected override void OnStateEnter(BattleState3D state)
    {
        if (state == BattleState3D.Endbattle) Cleanup();
        // We ignore Red/Blue states here because MasterBattleLoop handles everything now.
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
            // 1. Get the current team
            var currentTeam = battleTeams[currentTeamIndex];
            currentTeamName = currentTeam.TeamName;

            // 2. Turn Limit Check (Only increment if this team contains a player)
            bool isPlayerTeam = currentTeam.Members.Any(m => m is PlayerBattleController);
            if (isPlayerTeam)
            {
                playerTurnCount++;
                Debug.Log($"=== Team: {currentTeamName} | Turn {playerTurnCount}/{limitedTurnForPlayer} ===");

                if (playerTurnCount > limitedTurnForPlayer)
                {
                    playerWinResult = false;
                    EndBattle();
                    yield break;
                }
            }
            else
            {
                Debug.Log($"=== Team: {currentTeamName} (AI) ===");
            }

            // 3. Loop through all ALIVE members of this team
            foreach (var actor in currentTeam.Members.Where(m => m != null && m.IsAlive))
            {
                if (!isBattleActive) yield break;

                CurrentActor = actor;
                Debug.Log($"👉 It is now {actor.Name}'s Turn!");

                if (actor is MonoBehaviour monoActor)
                    FocusCamera(monoActor.transform);

                // 4. Determine if Actor is Player or AI dynamically
                PlayerBattleController playerTarget = actor as PlayerBattleController;
                if (playerTarget == null && actor is MonoBehaviour m)
                    playerTarget = m.GetComponent<PlayerBattleController>();

                // Execute the correct turn logic
                if (playerTarget != null)
                {
                    yield return StartCoroutine(HandlePlayerTurn(playerTarget));
                }
                else
                {
                    yield return StartCoroutine(HandleAITurn(actor));
                }

                // 5. Check if the battle is over after EVERY move
                if (CheckBattleEndCondition()) yield break;
            }

            // 6. Delay before next team
            if (phaseDelay > 0f) yield return new WaitForSeconds(phaseDelay);

            // 7. Cycle to the next team in the list
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
        if (turnTimer != null)
        {
            countdownTimer.StartNewTurn();
        }

        bool cheatUsed = false;

        // Link our global variable to the current active player
        playerBattlerController = player;
        HookPlayerShootEvent(true);
        SetPlayerControl(true);

        // Wait for shoot or cheat code
        while (!isActionDone && isBattleActive)
        {
            if (!cheatUsed && Input.GetKeyDown(KeyCode.O))
            {
                cheatUsed = true;
                Debug.Log("⏳ Cheat Activated: Ending turn in 5 seconds...");
                SetPlayerControl(false);
                HookPlayerShootEvent(false);
                awaitingPlayerAction = false;
                StartCoroutine(EndUnitTurnAfterDelay(5f));
            }
            yield return null;
        }

        SetPlayerControl(false);
        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
        CurrentActor = null;
    }

    private IEnumerator HandleAITurn(ITurnParticipant aiActor)
    {
        // Tell the AI to do its logic
        aiActor.TakeTurn();

        // Give the AI time to walk and shoot
        yield return new WaitForSeconds(aiTurnWaitTime);

        CurrentActor = null;
    }

    // =========================
    // Win / Loss Condition
    // =========================
    private bool CheckBattleEndCondition()
    {
        int activeTeamsCount = 0;
        BattleTeamData lastActiveTeam = null;

        // Count how many teams still have living members
        foreach (var team in battleTeams)
        {
            if (!team.IsDefeated)
            {
                activeTeamsCount++;
                lastActiveTeam = team;
            }
        }

        // If 1 or 0 teams are left, battle is over
        if (activeTeamsCount <= 1)
        {
            // Did a player-controlled team win?
            if (lastActiveTeam != null && lastActiveTeam.Members.Any(m => m is PlayerBattleController))
            {
                playerWinResult = true;
            }
            else
            {
                playerWinResult = false; // Enemies won, or a draw
            }

            EndBattle();
            return true; // Yes, battle ended
        }

        return false; // Battle continues
    }

    // =========================
    // Shoot hook
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
        isActionDone = true; // Signals the MasterLoop to continue
    }

    // =========================
    // Helpers
    // =========================
    private void FocusCamera(Transform follow)
    {
        if (mainCamera == null || follow == null) return;
        mainCamera.transform.position = follow.position - (follow.forward * 5f) + (Vector3.up * 3f);
        mainCamera.transform.LookAt(follow);
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

    // Hàm này sẽ được gọi từ Sự kiện On Timer End của Timer
    public void OnPlayerTimerTimeout()
    {
        // Chỉ xử lý nếu đang thực sự trong lượt của Player và đang chờ hành động
        if (awaitingPlayerAction && isBattleActive)
        {
            Debug.Log("⏰ [TIMEOUT] Hết thời gian suy nghĩ!");

            // 1. Tắt quyền điều khiển của Player ngay lập tức
            SetPlayerControl(false);
            HookPlayerShootEvent(false);


            if (playerBattlerController != null)
            {
                playerBattlerController.EnableControl(false);
            }

            // 3. Đánh dấu hành động đã xong
            awaitingPlayerAction = false;
            isActionDone = true;
        }
    }
}