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
/// BattleManagerCore (Abstract Parent)
/// - Owns: 5-state game flow + shared runtime fields + timer helper
/// - Does NOT: setup data, spawn units, decide mode rules, handle audio or cameras
/// - Child (PVE/1v1/2v2/Boss...) implements OnTick + optional hooks
/// </summary>
public abstract class BattleManagerCore : MonoBehaviour
{
    [Header("STATE")]
    [SerializeField] protected BattleState3D currentState = BattleState3D.Start;
    protected bool isBattleActive;
    protected bool isActionDone;
    protected float turnTimer;

    [Header("TEAM DATA (Runtime)")]
    public BattleTeamData BlueTeam { get; protected set; }
    public BattleTeamData RedTeam { get; protected set; }

    [Header("SETTINGS")]
    [SerializeField] protected float timePerTurn = 20f;

    // ===========================
    // LIFECYCLE
    // ===========================
    protected virtual void Awake() => OnCoreAwake();

    protected virtual void Update()
    {
        if (!isBattleActive) return;

        // timer is optional: child may ignore it
        turnTimer -= Time.deltaTime;

        OnTick(currentState);
    }

    // ===========================
    // CORE API (for children)
    // ===========================

    /// <summary>
    /// Child calls this after it sets up teams/spawns/ui.
    /// You can choose who starts via firstState (e.g. RedTeamTurn).
    /// </summary>
    protected void StartBattle(BattleTeamData blue, BattleTeamData red, BattleState3D firstState)
    {
        BlueTeam = blue;
        RedTeam = red;

        isBattleActive = true;
        ResetTurnTimer();

        SetState(BattleState3D.Start);
        StartCoroutine(BeginRoutine(firstState));
    }

    /// <summary>
    /// Child/controllers call this when an action (unit/team phase) is completed.
    /// </summary>
    public void MarkActionDone() => isActionDone = true;

    protected void SetState(BattleState3D next)
    {
        currentState = next;
        OnStateEnter(next);
    }

    protected void ResetTurnTimer()
    {
        turnTimer = timePerTurn;
        isActionDone = false;
    }

    protected void EndBattle()
    {
        isBattleActive = false;
        SetState(BattleState3D.Endbattle);
        OnBattleFinished();
    }

    // ===========================
    // CORE FLOW (Minimal)
    // ===========================

    private IEnumerator BeginRoutine(BattleState3D firstState)
    {
        yield return OnBattleStartIntro(); // child can yield break
        SetState(firstState);
        ResetTurnTimer();
    }

    // ===========================
    // CHILD HOOKS
    // ===========================

    protected virtual void OnCoreAwake() { }
    protected virtual IEnumerator OnBattleStartIntro() { yield break; }
    protected virtual void OnStateEnter(BattleState3D state) { }
    protected abstract void OnTick(BattleState3D state);
    protected virtual void OnBattleFinished() { }

    // ===========================
    // HELPER: Check if any team is defeated
    // ===========================
    protected bool IsAnyTeamDefeated()
    {
        if (BlueTeam == null || RedTeam == null) return false;
        return BlueTeam.IsDefeated || RedTeam.IsDefeated;
    }
}
