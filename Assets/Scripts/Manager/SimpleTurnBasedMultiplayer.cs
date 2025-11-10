using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn-based multiplayer đơn giản cho 2 player
/// Hỗ trợ: Hot-seat (local) và Online (với AI fallback)
/// </summary>
public class SimpleTurnBasedMultiplayer : MonoBehaviour
{
    [Header("Mode Selection")]
    [SerializeField] private MultiplayerMode mode = MultiplayerMode.HotSeat;

    [Header("Player References")]
    [SerializeField] private PlayerController bluePlayer;
    [SerializeField] private AIPlayerController redAIPlayer;

    [Header("Settings")]
    [SerializeField] private float aiThinkingTime = 1.5f;
    [SerializeField] private float turnTransitionDelay = 2f;

    [Header("UI")]
    [SerializeField] private GameObject turnIndicatorUI;
    [SerializeField] private UnityEngine.UI.Text turnText;

    // Runtime state
    private int currentTurn = 0; // 0 = Blue, 1 = Red
    private bool waitingForAction = false;
    private TurnAction pendingAction = null;
    private List<TurnAction> turnHistory = new List<TurnAction>();

    // Match data
    private MatchData currentMatch;
    private bool useAI = false;

    public enum MultiplayerMode
    {
        HotSeat,        // 2 người cùng máy
        OnlineAsync,    // Online với AI fallback
        OnlineRealtime  // Realtime (TODO)
    }

    // ===========================
    // 🎬 INITIALIZATION
    // ===========================
    void Start()
    {
        InitializeMatch();
    }

    private void InitializeMatch()
    {
        currentMatch = new MatchData
        {
            matchId = Guid.NewGuid().ToString(),
            bluePlayerHP = 100,
            redPlayerHP = 100,
            currentTurn = 0,
            status = "active"
        };

        // Subscribe to player actions
        if (bluePlayer)
        {
            bluePlayer.OnShoot += OnBluePlayerAction;
        }

        if (redAIPlayer)
        {
            redAIPlayer.OnShoot += OnRedPlayerAction;
        }

        // Start based on mode
        switch (mode)
        {
            case MultiplayerMode.HotSeat:
                StartHotSeatMatch();
                break;

            case MultiplayerMode.OnlineAsync:
                StartCoroutine(StartOnlineMatch());
                break;
        }
    }

    // ===========================
    // 🎮 HOT-SEAT MODE
    // ===========================
    private void StartHotSeatMatch()
    {
        Debug.Log("🎮 Hot-seat mode: Pass device between turns");
        useAI = false;
        StartCoroutine(HotSeatLoop());
    }

    private IEnumerator HotSeatLoop()
    {
        while (currentMatch.status == "active")
        {
            // Blue turn
            yield return StartCoroutine(ProcessTurn(0, false));

            if (CheckGameOver()) break;

            // Red turn (human or AI)
            yield return StartCoroutine(ProcessTurn(1, useAI));

            if (CheckGameOver()) break;
        }

        EndMatch();
    }

    // ===========================
    // 🌐 ONLINE ASYNC MODE
    // ===========================
    private IEnumerator StartOnlineMatch()
    {
        Debug.Log("🌐 Starting online match...");

        // Giả lập tìm opponent
        UpdateTurnUI("Searching for opponent...", Color.yellow);

        // Đợi 5s
        float timer = 5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Giả lập không tìm thấy → dùng AI
        Debug.Log("⏰ No opponent found, using AI");
        useAI = true;

        UpdateTurnUI("Playing vs AI", Color.white);
        yield return new WaitForSeconds(1f);

        // Start game loop
        StartCoroutine(HotSeatLoop()); // Dùng cùng loop
    }

    // ===========================
    // ⚔️ PROCESS TURN
    // ===========================
    private IEnumerator ProcessTurn(int teamId, bool isAI)
    {
        currentTurn = teamId;
        string teamName = teamId == 0 ? "Blue Team" : "Red Team";

        Debug.Log($"🎮 {teamName}'s Turn");
        UpdateTurnUI($"{teamName}'s Turn", teamId == 0 ? Color.cyan : Color.red);

        waitingForAction = true;
        pendingAction = null;

        // Enable appropriate controller
        if (teamId == 0)
        {
            // Blue turn (always human)
            bluePlayer.EnableControl(true);
            if (redAIPlayer) redAIPlayer.EnableControl(false);
        }
        else
        {
            // Red turn
            bluePlayer.EnableControl(false);

            if (isAI)
            {
                // AI turn
                yield return new WaitForSeconds(aiThinkingTime);
                redAIPlayer.EnableControl(true);
            }
            else
            {
                // Human Red player (hot-seat)
                // TODO: Enable red human controller
                Debug.Log("⏳ Pass device to Red player...");
                yield return new WaitForSeconds(2f); // Give time to pass device
            }
        }

        // Wait for action
        while (waitingForAction && pendingAction == null)
        {
            yield return null;
        }

        // Disable controllers
        bluePlayer.EnableControl(false);
        if (redAIPlayer) redAIPlayer.EnableControl(false);

        // Process action
        if (pendingAction != null)
        {
            yield return StartCoroutine(ExecuteAction(pendingAction));
            turnHistory.Add(pendingAction);
        }

        // Transition delay
        yield return new WaitForSeconds(turnTransitionDelay);
    }

    // ===========================
    // 💥 ACTION CALLBACKS
    // ===========================
    private void OnBluePlayerAction(Projectile projectile)
    {
        if (currentTurn != 0 || !waitingForAction) return;

        Debug.Log("🔫 Blue player shot!");

        pendingAction = new TurnAction
        {
            turnNumber = turnHistory.Count,
            playerTeam = 0,
            actionType = "shoot",
            position = projectile.transform.position,
            direction = projectile.transform.forward,
            power = 50f, // TODO: Get from player
            timestamp = DateTime.Now
        };

        waitingForAction = false;
    }

    private void OnRedPlayerAction(Projectile projectile)
    {
        if (currentTurn != 1 || !waitingForAction) return;

        Debug.Log("🤖 Red player shot!");

        pendingAction = new TurnAction
        {
            turnNumber = turnHistory.Count,
            playerTeam = 1,
            actionType = "shoot",
            position = projectile.transform.position,
            direction = projectile.transform.forward,
            power = 50f,
            timestamp = DateTime.Now
        };

        waitingForAction = false;
    }

    // ===========================
    // 🎬 EXECUTE ACTION
    // ===========================
    private IEnumerator ExecuteAction(TurnAction action)
    {
        Debug.Log($"🎬 Executing action: {action.actionType}");

        // Action đã được thực hiện bởi player/AI controller
        // Chỉ cần đợi projectile nổ

        // TODO: Track projectile and wait for explosion
        yield return new WaitForSeconds(2f);

        // Calculate damage (giả lập)
        int damage = UnityEngine.Random.Range(10, 30);

        if (action.playerTeam == 0)
        {
            // Blue attacked Red
            currentMatch.redPlayerHP -= damage;
            Debug.Log($"💥 Red took {damage} damage! HP: {currentMatch.redPlayerHP}");
        }
        else
        {
            // Red attacked Blue
            currentMatch.bluePlayerHP -= damage;
            Debug.Log($"💥 Blue took {damage} damage! HP: {currentMatch.bluePlayerHP}");
        }

        action.damageDealt = damage;
    }

    // ===========================
    // 🏁 GAME OVER
    // ===========================
    private bool CheckGameOver()
    {
        if (currentMatch.bluePlayerHP <= 0)
        {
            currentMatch.status = "finished";
            currentMatch.winner = "Red";
            return true;
        }

        if (currentMatch.redPlayerHP <= 0)
        {
            currentMatch.status = "finished";
            currentMatch.winner = "Blue";
            return true;
        }

        return false;
    }

    private void EndMatch()
    {
        Debug.Log($"🏁 Match ended! Winner: {currentMatch.winner}");

        string message = currentMatch.winner == "Blue" ? "BLUE TEAM WINS!" : "RED TEAM WINS!";
        Color color = currentMatch.winner == "Blue" ? Color.cyan : Color.red;

        UpdateTurnUI(message, color);

        // TODO: Show victory screen
    }

    // ===========================
    // 🎨 UI UPDATES
    // ===========================
    private void UpdateTurnUI(string message, Color color)
    {
        if (turnText)
        {
            turnText.text = message;
            turnText.color = color;
        }
    }

    // ===========================
    // 🔧 PUBLIC API
    // ===========================
    public void SetMode(MultiplayerMode newMode)
    {
        mode = newMode;
    }

    public void RestartMatch()
    {
        // Clear state
        turnHistory.Clear();
        currentTurn = 0;
        waitingForAction = false;
        pendingAction = null;

        // Reinitialize
        InitializeMatch();
    }

    // ===========================
    // 📊 SAVE/LOAD (Optional)
    // ===========================
    public string SaveMatchState()
    {
        var saveData = new MatchSaveData
        {
            match = currentMatch,
            turnHistory = turnHistory,
            currentTurn = currentTurn
        };

        return JsonUtility.ToJson(saveData);
    }

    public void LoadMatchState(string json)
    {
        var saveData = JsonUtility.FromJson<MatchSaveData>(json);

        currentMatch = saveData.match;
        turnHistory = saveData.turnHistory;
        currentTurn = saveData.currentTurn;

        Debug.Log($"📥 Match loaded: {turnHistory.Count} turns played");
    }
}

// ===========================
// 📋 DATA CLASSES
// ===========================
[Serializable]
public class TurnAction
{
    public int turnNumber;
    public int playerTeam;         // 0=Blue, 1=Red
    public string actionType;      // "shoot", "move", "skill"
    public Vector3 position;
    public Vector3 direction;
    public float power;
    public int damageDealt;
    public DateTime timestamp;
}

[Serializable]
public class MatchData
{
    public string matchId;
    public int bluePlayerHP;
    public int redPlayerHP;
    public int currentTurn;
    public string status;          // "active", "finished"
    public string winner;          // "Blue", "Red", or null
}

[Serializable]
public class MatchSaveData
{
    public MatchData match;
    public List<TurnAction> turnHistory;
    public int currentTurn;
}