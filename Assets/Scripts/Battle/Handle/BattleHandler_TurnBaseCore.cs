using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public enum BattleState3D
{
    Start,
    BlueTeamTurn,
    RedTeamTurn,
    AnimationPlay,
    Finish
}

public class BattleHandler_TurnBaseCore : MonoBehaviour
{
    [Header("STATE MACHINE")]
    [SerializeField] protected BattleState3D currentState = BattleState3D.Start;
    protected bool isBattleActive = false;
    protected bool isPlayerActionDone = false;
    protected float turnTimer;
    protected string currentTeamName;

    [Header("TEAM DATA (Runtime)")]
    public BattleTeamData BlueTeam { get; protected set; }
    public BattleTeamData RedTeam { get; protected set; }

    [Header("CAMERAS")]
    [SerializeField] protected CinemachineCamera _startCamera;
    [SerializeField] protected CinemachineCamera _blueTeamCamera;
    [SerializeField] protected CinemachineCamera _redTeamCamera;
    [SerializeField] protected CinemachineCamera _animationCamera;

    [Header("AUDIO")]
    //[SerializeField] protected AudioSource _battleMusic;
    [SerializeField] protected AudioSource _turnSound;
    [SerializeField] protected AudioSource _impactSound;
    [SerializeField] protected AudioSource _victoryMusic;
    [SerializeField] protected AudioSource _skipTurnSound;

    [Header("SETTINGS")]
    [SerializeField] protected float _timePerTurn = 20f;
    [SerializeField] protected float _transitionDelay = 2f;

    // ===========================
    // 🧩 CORE LIFE CYCLE
    // ===========================
    protected virtual void Awake()
    {
        SetupDummyData();

       // if (_battleMusic)
       // {
        //    _battleMusic.loop = true;
       //     _battleMusic.Play();
        //}

        Debug.Log("🧩 BattleHandler_Base Initialized");
    }

    protected virtual void Start()
    {
        StartCoroutine(HandleBattleStart());
    }

    protected virtual void Update()
    {
        if (!isBattleActive) return;

        switch (currentState)
        {
            case BattleState3D.BlueTeamTurn:
                HandleTurn(BlueTeam, BattleState3D.RedTeamTurn);
                break;

            case BattleState3D.RedTeamTurn:
                HandleTurn(RedTeam, BattleState3D.BlueTeamTurn);
                break;
        }
    }

    // ===========================
    // 🧱 DUMMY / REAL DATA
    // ===========================
    protected virtual void SetupDummyData()
    {
        BlueTeam = new BattleTeamData("Blue Team");
        RedTeam = new BattleTeamData("Red Team");
      

        Debug.Log("🧱 Dummy data loaded.");
    }

    public virtual void LoadRealData(BattleTeamData blueData, BattleTeamData redData)
    {
        BlueTeam = blueData;
        RedTeam = redData;
        Debug.Log("📦 Real data injected into BattleHandler");
    }

    // ===========================
    // 🎬 START PHASE
    // ===========================
    protected virtual IEnumerator HandleBattleStart()
    {
        Debug.Log("🎥 Start Phase — Intro Camera");
        ActivateCamera(_startCamera);
        yield return new WaitForSeconds(2f);

        currentState = BattleState3D.BlueTeamTurn;
        isBattleActive = true;
        turnTimer = _timePerTurn;
    }

    // ===========================
    // ⚔️ TURN PHASE
    // ===========================
    protected virtual void HandleTurn(BattleTeamData team, BattleState3D nextState)
    {
        if (team == null) return;
        if (team.IsDefeated)
        {
            EndBattle();
            return;
        }

        if (currentTeamName != team.TeamName)
        {
            currentTeamName = team.TeamName;
            ActivateCamera(team.TeamName.Contains("Blue") ? _blueTeamCamera : _redTeamCamera);
            _turnSound?.Play();
            Debug.Log($"🎮 {team.TeamName}'s Turn Start!");
            turnTimer = _timePerTurn;
            isPlayerActionDone = false;
        }

        turnTimer -= Time.deltaTime;

        // (Tạm thời) SPACE để giả lập hành động
        //if (Input.GetKeyDown(KeyCode.Space) && !isPlayerActionDone)
        //{
        //    Debug.Log($"💥 {team.TeamName} Action simulated!");
        //    isPlayerActionDone = true;
        //    StartCoroutine(HandleAnimationPhase(nextState));
        //}

        if (turnTimer <= 0 && !isPlayerActionDone)
        {
            Debug.Log($"⏰ {team.TeamName} Timeout — Skip turn");
            _skipTurnSound?.Play();
            isPlayerActionDone = true;
            StartCoroutine(HandleAnimationPhase(nextState));
        }
    }

    // ===========================
    // 🎬 ANIMATION PHASE
    // ===========================
    protected virtual IEnumerator HandleAnimationPhase(BattleState3D nextState)
    {
        currentState = BattleState3D.AnimationPlay;
        ActivateCamera(_animationCamera);

        Debug.Log("🎬 Animation playing...");
        yield return new WaitForSeconds(_transitionDelay);

        if (BlueTeam.IsDefeated || RedTeam.IsDefeated)
        {
            EndBattle();
            yield break;
        }

        currentState = nextState;
        currentTeamName = "";
        turnTimer = _timePerTurn;

        Debug.Log($"🔁 Switching turn → {currentState}");
    }

    // ===========================
    // 🏁 FINISH PHASE
    // ===========================
    protected virtual void EndBattle()
    {
        isBattleActive = false;
        currentState = BattleState3D.Finish;

        bool blueWin = !BlueTeam.IsDefeated && RedTeam.IsDefeated;
        bool redWin = !RedTeam.IsDefeated && BlueTeam.IsDefeated;

        //_battleMusic?.Stop();
        Debug.Log("🏁 Battle Finished");

        if (blueWin)
        {
            Debug.Log("✅ BLUE TEAM WINS!");
            _victoryMusic?.Play();
        }
        else if (redWin)
        {
            Debug.Log("❌ RED TEAM WINS!");
            _victoryMusic?.Play();
        }
        else
        {
            Debug.Log("⚖️ DRAW!");
        }
    }

    // ===========================
    // 🎥 CAMERA HELPER
    // ===========================
    protected virtual void ActivateCamera(CinemachineCamera cam)
    {
        if (!cam) return;

        _startCamera.Priority = 1;
        _blueTeamCamera.Priority = 1;
        _redTeamCamera.Priority = 1;
        _animationCamera.Priority = 1;

        cam.Priority = 20;
        Debug.Log($"📷 Active Camera: {cam.name}");
    }
}
