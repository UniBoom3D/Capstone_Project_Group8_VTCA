using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleHandlerPvE : BattleCore
{
    public static BattleHandlerPvE Instance { get; private set; }

    [Header("PLAYER CONTROLLER")]
    [SerializeField] protected PlayerBattleController playerBattlerController;

    [Header("RULES")]
    [SerializeField] protected int limitedTurnForPlayer = 20;
    [SerializeField] protected float phaseDelay = 1f;

    [Header("PLAYER TURN")]
    [SerializeField] protected float endTurnAfterShootDelay = 0.1f;

    public ITurnParticipant CurrentActor { get; private set; }
    public string currentTeamName;

    protected bool awaitingPlayerAction;
    protected int playerTurnCount;
    protected bool playerWinResult;
    protected Coroutine phaseRoutine;

    protected virtual void Awake()
    {
        Instance = this;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void StartBattlePVE(BattleTeamData blueTeam, BattleTeamData redTeam)
    {
        if (isBattleActive) return;

        playerTurnCount = 0;
        playerWinResult = false;

        if (playerBattlerController == null)
            playerBattlerController = FindFirstObjectByType<PlayerBattleController>();

        StartBattle(blueTeam, redTeam, BattleState3D.RedTeamTurn);
    }

    // =========================
    // CORE HOOKS
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
                Cleanup();
                break;
        }
    }

    protected override void OnTick(BattleState3D state) { }

    protected override void OnBattleFinished()
    {
        Debug.Log(playerWinResult ? "[BATTLE] Victory" : "[BATTLE] Defeat");
        Cleanup();
    }

    // =========================
    // ENEMY PHASE
    // =========================

    protected virtual IEnumerator EnemyPhase()
    {
        if (!isBattleActive) yield break;

        currentTeamName = RedTeam.TeamName;
        Debug.Log("=== Enemy Turn ===");

        foreach (var enemy in RedTeam.Members)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            if (enemy is MonoBehaviour monoEnemy)
            {
                CameraPvEController.Instance.FocusEnemy(monoEnemy.transform);
            }

            enemy.TakeTurn();

            yield return new WaitForSeconds(3f);
        }

        if (RedTeam.IsDefeated) { playerWinResult = true; EndBattle(); yield break; }
        if (BlueTeam.IsDefeated) { playerWinResult = false; EndBattle(); yield break; }

        yield return new WaitForSeconds(phaseDelay);

        SetState(BattleState3D.BlueTeamTurn);
    }

    // =========================
    // PLAYER PHASE
    // =========================

    protected virtual IEnumerator PlayerPhase()
    {
        if (!isBattleActive) yield break;

        currentTeamName = BlueTeam.TeamName;

        playerTurnCount++;

        Debug.Log($"=== Player Turn {playerTurnCount}/{limitedTurnForPlayer} ===");

        if (playerTurnCount > limitedTurnForPlayer && !RedTeam.IsDefeated)
        {
            playerWinResult = false;
            EndBattle();
            yield break;
        }

        foreach (var actor in BlueTeam.Members.Where(m => m != null && m.IsAlive))
        {
            CurrentActor = actor;

            if (actor is MonoBehaviour monoActor)
                CameraPvEController.Instance.FocusPlayer(monoActor.transform);

            isActionDone = false;
            awaitingPlayerAction = true;

            HookPlayerShootEvent(true);
            SetPlayerControl(true);

            while (!isActionDone && isBattleActive)
                yield return null;

            SetPlayerControl(false);
            HookPlayerShootEvent(false);

            awaitingPlayerAction = false;
            CurrentActor = null;

            if (RedTeam.IsDefeated) { playerWinResult = true; EndBattle(); yield break; }
            if (BlueTeam.IsDefeated) { playerWinResult = false; EndBattle(); yield break; }
        }

        yield return new WaitForSeconds(phaseDelay);

        SetState(BattleState3D.RedTeamTurn);
    }

    // =========================
    // PLAYER SHOOT
    // =========================

    protected void HookPlayerShootEvent(bool hook)
    {
        if (playerBattlerController == null) return;

        playerBattlerController.OnShoot -= OnPlayerShoot;

        if (hook)
            playerBattlerController.OnShoot += OnPlayerShoot;
    }

    protected void OnPlayerShoot(Projectile projectile)
    {
        if (!awaitingPlayerAction) return;

        awaitingPlayerAction = false;

        StartCoroutine(EndUnitTurnAfterDelay(endTurnAfterShootDelay));
    }

    protected IEnumerator EndUnitTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        MarkActionDone();
    }

    // =========================
    // HELPERS
    // =========================

    protected void SetPlayerControl(bool enable)
    {
        if (playerBattlerController == null) return;

        playerBattlerController.EnableControl(enable);
    }

    protected void Cleanup()
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