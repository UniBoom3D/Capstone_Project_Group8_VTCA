using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

/// <summary>
/// Quản lý trận đấu multiplayer với hỗ trợ AI fallback
/// </summary>
public class NetworkBattleManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPlayerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform blueTeamSpawn;
    [SerializeField] private Transform redTeamSpawn;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera startCamera;
    [SerializeField] private CinemachineCamera blueTeamCamera;
    [SerializeField] private CinemachineCamera redTeamCamera;
    [SerializeField] private CinemachineCamera animationCamera;

    [Header("Settings")]
    [SerializeField] private float matchmakingTimeout = 30f;
    [SerializeField] private float turnDuration = 20f;
    [SerializeField] private float turnTransitionDelay = 2f;

    [Header("Runtime State (Synced)")]
    public NetworkVariable<BattleState> currentState = new NetworkVariable<BattleState>(BattleState.WaitingForPlayers);
    public NetworkVariable<int> currentTurn = new NetworkVariable<int>(0); // 0=Blue, 1=Red
    public NetworkVariable<float> turnTimer = new NetworkVariable<float>(0);

    // Teams
    public NetworkList<ulong> blueTeam;
    public NetworkList<ulong> redTeam;

    private Dictionary<ulong, NetworkBattlePlayer> allPlayers = new Dictionary<ulong, NetworkBattlePlayer>();
    private float matchmakingTimer;

    // ===========================
    // 🎬 INITIALIZATION
    // ===========================
    private void Awake()
    {
        blueTeam = new NetworkList<ulong>();
        redTeam = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            StartCoroutine(ServerMatchSequence());
        }

        // Subscribe to state changes
        currentState.OnValueChanged += OnStateChanged;
        currentTurn.OnValueChanged += OnTurnChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentState.OnValueChanged -= OnStateChanged;
        currentTurn.OnValueChanged -= OnTurnChanged;

        base.OnNetworkDespawn();
    }

    // ===========================
    // 🎮 SERVER MATCH SEQUENCE
    // ===========================
    private IEnumerator ServerMatchSequence()
    {
        Debug.Log("🎮 Server: Starting match sequence...");

        // Phase 1: Waiting for players
        currentState.Value = BattleState.WaitingForPlayers;
        yield return StartCoroutine(WaitForPlayersPhase());

        // Phase 2: Match found / AI spawned
        currentState.Value = BattleState.Preparing;
        yield return StartCoroutine(PreparationPhase());

        // Phase 3: Battle intro
        currentState.Value = BattleState.Intro;
        yield return new WaitForSeconds(3f);

        // Phase 4: Battle loop
        currentState.Value = BattleState.InProgress;
        yield return StartCoroutine(BattleLoopPhase());

        // Phase 5: Battle end
        currentState.Value = BattleState.Finished;
        yield return StartCoroutine(BattleEndPhase());
    }

    // ===========================
    // 👥 PHASE 1: WAITING FOR PLAYERS
    // ===========================
    private IEnumerator WaitForPlayersPhase()
    {
        matchmakingTimer = matchmakingTimeout;

        Debug.Log($"⏳ Waiting for Red Team player... ({matchmakingTimeout}s timeout)");

        while (matchmakingTimer > 0)
        {
            // Kiểm tra có player thật join Red Team chưa
            if (redTeam.Count > 0)
            {
                var redPlayer = GetPlayerByClientId(redTeam[0]);
                if (redPlayer != null && redPlayer.playerType.Value == PlayerType.RemotePlayer)
                {
                    Debug.Log("✅ Human opponent found!");
                    yield break;
                }
            }

            matchmakingTimer -= Time.deltaTime;
            turnTimer.Value = matchmakingTimer; // Hiển thị countdown

            yield return null;
        }

        // Timeout → Spawn AI
        Debug.Log("⏰ Matchmaking timeout → Spawning AI opponent");
        SpawnAIOpponent();
    }

    // ===========================
    // 🤖 SPAWN AI OPPONENT
    // ===========================
    [Server]
    private void SpawnAIOpponent()
    {
        if (aiPlayerPrefab == null)
        {
            Debug.LogError("❌ AI Player Prefab not assigned!");
            return;
        }

        // Spawn AI GameObject
        GameObject aiObj = Instantiate(aiPlayerPrefab, redTeamSpawn.position, redTeamSpawn.rotation);
        NetworkObject netObj = aiObj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("❌ AI Prefab missing NetworkObject component!");
            Destroy(aiObj);
            return;
        }

        netObj.Spawn();

        // Setup AI player
        NetworkBattlePlayer aiPlayer = aiObj.GetComponent<NetworkBattlePlayer>();
        if (aiPlayer != null)
        {
            aiPlayer.playerType.Value = PlayerType.AIPlayer;
            aiPlayer.teamID.Value = 1; // Red Team
            aiPlayer.maxHP.Value = 100;
            aiPlayer.currentHP.Value = 100;

            // Add to red team
            redTeam.Add(netObj.OwnerClientId);
            allPlayers[netObj.OwnerClientId] = aiPlayer;

            Debug.Log($"🤖 AI opponent spawned! ClientId: {netObj.OwnerClientId}");
        }
    }

    // ===========================
    // 🛠️ PHASE 2: PREPARATION
    // ===========================
    private IEnumerator PreparationPhase()
    {
        Debug.Log("🛠️ Preparing battle...");

        // Ensure all players are ready
        foreach (var playerId in blueTeam)
        {
            var player = GetPlayerByClientId(playerId);
            if (player != null)
            {
                player.currentHP.Value = player.maxHP.Value;
            }
        }

        foreach (var playerId in redTeam)
        {
            var player = GetPlayerByClientId(playerId);
            if (player != null)
            {
                player.currentHP.Value = player.maxHP.Value;
            }
        }

        yield return new WaitForSeconds(1f);
    }

    // ===========================
    // ⚔️ PHASE 3: BATTLE LOOP
    // ===========================
    private IEnumerator BattleLoopPhase()
    {
        currentTurn.Value = 0; // Blue team starts

        while (!IsBattleFinished())
        {
            // Blue Team Turn
            yield return StartCoroutine(ProcessTeamTurn(blueTeam, 0));

            if (IsBattleFinished()) break;

            // Red Team Turn
            yield return StartCoroutine(ProcessTeamTurn(redTeam, 1));
        }

        Debug.Log("🏁 Battle finished!");
    }

    private IEnumerator ProcessTeamTurn(NetworkList<ulong> team, int teamIndex)
    {
        currentTurn.Value = teamIndex;

        foreach (var playerId in team)
        {
            var player = GetPlayerByClientId(playerId);
            if (player == null || !player.isAlive.Value) continue;

            // Start player turn
            turnTimer.Value = turnDuration;
            player.StartTurnServerRpc();

            Debug.Log($"🎮 Turn start: {player.GetDisplayName()}");

            // Wait for action or timeout
            bool actionCompleted = false;
            while (turnTimer.Value > 0 && !actionCompleted)
            {
                turnTimer.Value -= Time.deltaTime;

                if (player.hasActed.Value)
                {
                    actionCompleted = true;
                    Debug.Log($"✅ {player.GetDisplayName()} completed action");
                }

                yield return null;
            }

            // End turn
            player.EndTurnServerRpc();

            if (!actionCompleted)
            {
                Debug.Log($"⏰ {player.GetDisplayName()} timeout - turn skipped");
            }

            // Transition delay
            yield return new WaitForSeconds(turnTransitionDelay);

            // Check if battle finished after this action
            if (IsBattleFinished()) yield break;
        }
    }

    // ===========================
    // 🏆 PHASE 4: BATTLE END
    // ===========================
    private IEnumerator BattleEndPhase()
    {
        bool blueWins = IsTeamAlive(blueTeam) && !IsTeamAlive(redTeam);
        bool redWins = IsTeamAlive(redTeam) && !IsTeamAlive(blueTeam);

        if (blueWins)
        {
            Debug.Log("🎉 BLUE TEAM WINS!");
            ShowVictoryClientRpc(0);
        }
        else if (redWins)
        {
            Debug.Log("🎉 RED TEAM WINS!");
            ShowVictoryClientRpc(1);
        }
        else
        {
            Debug.Log("⚖️ DRAW!");
            ShowDrawClientRpc();
        }

        yield return new WaitForSeconds(5f);

        // Return to lobby or restart
    }

    [ClientRpc]
    private void ShowVictoryClientRpc(int winningTeam)
    {
        string teamName = winningTeam == 0 ? "BLUE TEAM" : "RED TEAM";
        Debug.Log($"🏆 {teamName} VICTORY!");

        // TODO: Show victory UI
    }

    [ClientRpc]
    private void ShowDrawClientRpc()
    {
        Debug.Log("⚖️ MATCH DRAW!");
        // TODO: Show draw UI
    }

    // ===========================
    // 🔍 UTILITY METHODS
    // ===========================
    private bool IsBattleFinished()
    {
        return !IsTeamAlive(blueTeam) || !IsTeamAlive(redTeam);
    }

    private bool IsTeamAlive(NetworkList<ulong> team)
    {
        foreach (var playerId in team)
        {
            var player = GetPlayerByClientId(playerId);
            if (player != null && player.isAlive.Value)
            {
                return true;
            }
        }
        return false;
    }

    private NetworkBattlePlayer GetPlayerByClientId(ulong clientId)
    {
        if (allPlayers.ContainsKey(clientId))
        {
            return allPlayers[clientId];
        }

        // Try to find and cache
        var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .Values.FirstOrDefault(obj => obj.OwnerClientId == clientId);

        if (netObj != null)
        {
            var player = netObj.GetComponent<NetworkBattlePlayer>();
            if (player != null)
            {
                allPlayers[clientId] = player;
                return player;
            }
        }

        return null;
    }

    // ===========================
    // 🎥 CAMERA MANAGEMENT
    // ===========================
    private void OnStateChanged(BattleState oldState, BattleState newState)
    {
        Debug.Log($"🎬 State changed: {oldState} → {newState}");

        switch (newState)
        {
            case BattleState.WaitingForPlayers:
                ActivateCamera(startCamera);
                break;

            case BattleState.Intro:
                ActivateCamera(startCamera);
                break;

            case BattleState.InProgress:
                // Will be handled by turn changes
                break;
        }
    }

    private void OnTurnChanged(int oldTurn, int newTurn)
    {
        Debug.Log($"🔄 Turn changed: Team {oldTurn} → Team {newTurn}");

        if (newTurn == 0)
        {
            ActivateCamera(blueTeamCamera);
        }
        else
        {
            ActivateCamera(redTeamCamera);
        }
    }

    private void ActivateCamera(CinemachineCamera cam)
    {
        if (cam == null) return;

        startCamera.Priority = 1;
        blueTeamCamera.Priority = 1;
        redTeamCamera.Priority = 1;
        animationCamera.Priority = 1;

        cam.Priority = 20;
    }

    // ===========================
    // 📡 PLAYER CONNECTION
    // ===========================
    public void OnPlayerConnected(NetworkBattlePlayer player)
    {
        if (!IsServer) return;

        ulong clientId = player.OwnerClientId;
        allPlayers[clientId] = player;

        // Assign to team
        if (blueTeam.Count == 0)
        {
            // First player → Blue team
            player.playerType.Value = PlayerType.LocalPlayer;
            player.teamID.Value = 0;
            blueTeam.Add(clientId);
            Debug.Log($"👤 Player {clientId} joined BLUE TEAM");
        }
        else if (redTeam.Count == 0)
        {
            // Second player → Red team
            player.playerType.Value = PlayerType.RemotePlayer;
            player.teamID.Value = 1;
            redTeam.Add(clientId);
            Debug.Log($"👤 Player {clientId} joined RED TEAM");
        }
        else
        {
            Debug.LogWarning($"⚠️ Player {clientId} tried to join but teams are full!");
        }
    }

    public void OnPlayerDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"🔌 Player {clientId} disconnected");

        // Check if this was a team member
        if (redTeam.Contains(clientId))
        {
            var player = GetPlayerByClientId(clientId);
            if (player != null && player.playerType.Value == PlayerType.RemotePlayer)
            {
                // Replace with AI
                Debug.Log("🤖 Replacing disconnected player with AI...");

                // Remove old player
                redTeam.Remove(clientId);
                allPlayers.Remove(clientId);

                // Spawn AI replacement
                SpawnAIOpponent();
            }
        }
    }
}

// ===========================
// 📋 ENUMS
// ===========================
public enum BattleState
{
    WaitingForPlayers,
    Preparing,
    Intro,
    InProgress,
    Finished
}