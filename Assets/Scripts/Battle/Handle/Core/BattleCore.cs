using System.Collections;
using UnityEngine;

public enum BattleState3D
{
    Start,
    BlueTeamTurn,
    RedTeamTurn,
    AnimationPlay,
    Endbattle
}

/// <summary>
/// BattleCore (Abstract Parent)
/// --------------------------------------------------
/// RESPONSIBILITY
/// - Owns the core 5-state battle flow
/// - Holds shared runtime data (teams, timer, flags)
/// - Provides helper utilities for children
///
/// DOES NOT
/// - Spawn units
/// - Decide battle rules
/// - Control camera / UI
/// - Handle player input
///
/// CHILD CLASSES (PvE / PvP / Boss...)
/// - Setup teams
/// - Control phase logic
/// - Implement OnTick()
/// </summary>
public abstract class BattleCore : MonoBehaviour
{
    // =========================
    // STATE
    // =========================

    [Header("STATE")]
    [SerializeField] protected BattleState3D currentState = BattleState3D.Start;

    protected bool isBattleActive;
    protected bool isActionDone;

    protected float turnTimer;

    // =========================
    // TEAM DATA (Runtime)
    // =========================

    [Header("TEAM DATA (Runtime)")]

    public BattleTeamData BlueTeam { get; protected set; }
    public BattleTeamData RedTeam { get; protected set; }

    // =========================
    // SETTINGS
    // =========================

    [Header("SETTINGS")]
    [SerializeField] protected float timePerTurn = 20f;

    // =========================
    // EVENTS
    // =========================

    public System.Action OnBattleEnded;

    // =========================
    // LIFECYCLE
    // =========================

    protected virtual void Awake()
    {
        OnCoreAwake();
    }

    protected virtual void Update()
    {
        if (!isBattleActive)
            return;

        // countdown timer
        turnTimer -= Time.deltaTime;

        OnTick(currentState);
    }

    // =========================
    // CORE API (for children)
    // =========================

    /// <summary>
    /// Call this AFTER teams are setup
    /// </summary>
    protected void StartBattle(
        BattleTeamData blue,
        BattleTeamData red,
        BattleState3D firstState)
    {
        BlueTeam = blue;
        RedTeam = red;

        isBattleActive = true;

        ResetTurnTimer();

        SetState(BattleState3D.Start);

        StartCoroutine(BeginRoutine(firstState));
    }

    /// <summary>
    /// Called when unit / phase action is completed
    /// </summary>
    public void MarkActionDone()
    {
        isActionDone = true;
    }

    /// <summary>
    /// Change battle state
    /// </summary>
    protected void SetState(BattleState3D next)
    {
        currentState = next;

        OnStateEnter(next);
    }

    /// <summary>
    /// Reset turn timer + action flag
    /// </summary>
    protected void ResetTurnTimer()
    {
        turnTimer = timePerTurn;
        isActionDone = false;
    }

    /// <summary>
    /// Force end battle
    /// </summary>
    protected void EndBattle()
    {
        if (!isBattleActive)
            return;

        isBattleActive = false;

        SetState(BattleState3D.Endbattle);

        OnBattleFinished();

        OnBattleEnded?.Invoke();
    }

    // =========================
    // CORE FLOW
    // =========================

    private IEnumerator BeginRoutine(BattleState3D firstState)
    {
        yield return OnBattleStartIntro();

        SetState(firstState);

        ResetTurnTimer();
    }

    // =========================
    // CHILD HOOKS
    // =========================

    /// <summary>
    /// Called during Awake
    /// </summary>
    protected virtual void OnCoreAwake() { }

    /// <summary>
    /// Optional intro animation / delay
    /// </summary>
    protected virtual IEnumerator OnBattleStartIntro()
    {
        yield break;
    }

    /// <summary>
    /// Called when state changes
    /// </summary>
    protected virtual void OnStateEnter(BattleState3D state) { }

    /// <summary>
    /// Called every frame while battle active
    /// </summary>
    protected abstract void OnTick(BattleState3D state);

    /// <summary>
    /// Called when battle ends
    /// </summary>
    protected virtual void OnBattleFinished() { }

    // =========================
    // HELPERS
    // =========================

    protected bool IsAnyTeamDefeated()
    {
        if (BlueTeam == null || RedTeam == null)
            return false;

        return BlueTeam.IsDefeated || RedTeam.IsDefeated;
    }

    protected bool IsBlueTeamDefeated()
    {
        if (BlueTeam == null)
            return false;

        return BlueTeam.IsDefeated;
    }

    protected bool IsRedTeamDefeated()
    {
        if (RedTeam == null)
            return false;

        return RedTeam.IsDefeated;
    }

    protected bool IsTimerExpired()
    {
        return turnTimer <= 0f;
    }

    public BattleState3D GetCurrentState()
    {
        return currentState;
    }

    public bool IsBattleRunning()
    {
        return isBattleActive;
    }
}