using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

/// <summary>
/// Quản lý matchmaking và kết nối multiplayer
/// </summary>
public class MatchmakingManager : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private ushort serverPort = 7777;

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject searchingPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Match Settings")]
    [SerializeField] private float matchmakingTimeout = 30f;

    private NetworkManager networkManager;
    private UnityTransport transport;
    private MatchMode currentMode;
    private bool isSearching = false;

    public enum MatchMode
    {
        PvP,            // Player vs Player (tìm người chơi thật)
        PvAI,           // Player vs AI (chơi với AI ngay lập tức)
        QuickMatch      // Tìm người, timeout → AI
    }

    // ===========================
    // 🔧 INITIALIZATION
    // ===========================
    private void Awake()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError("❌ NetworkManager not found!");
            return;
        }

        transport = networkManager.GetComponent<UnityTransport>();
    }

    private void Start()
    {
        // Setup UI
        if (searchingPanel) searchingPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);

        // Register network callbacks
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    // ===========================
    // 🎮 PUBLIC API
    // ===========================
    /// <summary>
    /// Tìm match PvP (người chơi thật)
    /// </summary>
    public void FindPvPMatch()
    {
        StartMatchmaking(MatchMode.PvP);
    }

    /// <summary>
    /// Chơi với AI ngay lập tức
    /// </summary>
    public void PlayVsAI()
    {
        StartMatchmaking(MatchMode.PvAI);
    }

    /// <summary>
    /// Quick Match (tìm người, timeout → AI)
    /// </summary>
    public void QuickMatch()
    {
        StartMatchmaking(MatchMode.QuickMatch);
    }

    // ===========================
    // 🔍 MATCHMAKING LOGIC
    // ===========================
    private void StartMatchmaking(MatchMode mode)
    {
        if (isSearching)
        {
            Debug.LogWarning("⚠️ Already searching for match!");
            return;
        }

        currentMode = mode;
        isSearching = true;

        // Hide main menu, show searching UI
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (searchingPanel) searchingPanel.SetActive(true);

        switch (mode)
        {
            case MatchMode.PvP:
                StartCoroutine(SearchPvPMatch());
                break;

            case MatchMode.PvAI:
                StartCoroutine(CreatePvAIMatch());
                break;

            case MatchMode.QuickMatch:
                StartCoroutine(QuickMatchSearch());
                break;
        }
    }

    // ===========================
    // 🎯 PvP MATCHMAKING
    // ===========================
    private IEnumerator SearchPvPMatch()
    {
        UpdateStatus("Searching for opponent...");

        // Option 1: Join existing server
        bool joinedExisting = TryJoinExistingServer();

        if (joinedExisting)
        {
            Debug.Log("🔗 Joined existing server as Red Team");
            yield break;
        }

        // Option 2: Create new server and wait
        Debug.Log("🌐 No server found, creating new one...");
        UpdateStatus("Creating match...");

        StartHost();

        // Đợi người chơi thứ 2
        float timer = matchmakingTimeout;
        while (timer > 0)
        {
            UpdateCountdown(timer);

            // Kiểm tra có người join chưa
            if (networkManager.ConnectedClients.Count >= 2)
            {
                Debug.Log("✅ Opponent found!");
                UpdateStatus("Opponent found! Starting match...");
                yield return new WaitForSeconds(2f);
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // Timeout → Chuyển sang AI
        Debug.Log("⏰ PvP timeout → Switching to AI");
        UpdateStatus("No opponent found. Fighting AI...");
        yield return new WaitForSeconds(2f);

        // AI đã được spawn tự động bởi NetworkBattleManager
    }

    // ===========================
    // 🤖 PvAI MATCHMAKING
    // ===========================
    private IEnumerator CreatePvAIMatch()
    {
        UpdateStatus("Creating match vs AI...");

        // Tạo host ngay lập tức
        StartHost();

        yield return new WaitForSeconds(1f);

        UpdateStatus("Match ready!");

        // AI sẽ được spawn bởi NetworkBattleManager
        // (do timeout trong WaitForPlayers phase)
    }

    // ===========================
    // ⚡ QUICK MATCH
    // ===========================
    private IEnumerator QuickMatchSearch()
    {
        UpdateStatus("Quick match searching...");

        // Try join existing
        bool joined = TryJoinExistingServer();

        if (joined)
        {
            Debug.Log("✅ Joined existing match!");
            yield break;
        }

        // Create new và đợi 10s (shorter timeout)
        StartHost();

        float quickTimeout = 10f;
        while (quickTimeout > 0)
        {
            UpdateCountdown(quickTimeout);

            if (networkManager.ConnectedClients.Count >= 2)
            {
                Debug.Log("✅ Quick match found!");
                yield break;
            }

            quickTimeout -= Time.deltaTime;
            yield return null;
        }

        // Quick timeout → AI
        Debug.Log("🤖 Quick match → AI opponent");
        UpdateStatus("Fighting AI...");
    }

    // ===========================
    // 🔌 CONNECTION HELPERS
    // ===========================
    private void StartHost()
    {
        if (transport != null)
        {
            transport.SetConnectionData(serverIP, serverPort);
        }

        bool success = networkManager.StartHost();

        if (success)
        {
            Debug.Log("✅ Started as Host (Blue Team)");
        }
        else
        {
            Debug.LogError("❌ Failed to start host!");
            OnMatchmakingFailed("Failed to create match");
        }
    }

    private bool TryJoinExistingServer()
    {
        if (transport != null)
        {
            transport.SetConnectionData(serverIP, serverPort);
        }

        bool success = networkManager.StartClient();

        if (success)
        {
            Debug.Log("🔗 Attempting to join as Client (Red Team)");
            return true;
        }

        return false;
    }

    public void CancelSearch()
    {
        if (!isSearching) return;

        Debug.Log("🚫 Matchmaking cancelled");

        // Shutdown network
        if (networkManager.IsHost || networkManager.IsClient)
        {
            networkManager.Shutdown();
        }

        // Reset UI
        isSearching = false;
        if (searchingPanel) searchingPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
    }

    // ===========================
    // 📡 NETWORK CALLBACKS
    // ===========================
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client {clientId} connected");

        if (networkManager.IsHost)
        {
            // Host nhận client mới
            if (clientId != networkManager.LocalClientId)
            {
                Debug.Log($"🎉 Opponent joined! ClientId: {clientId}");
                UpdateStatus("Opponent found! Starting...");
            }
        }
        else
        {
            // Client kết nối thành công
            Debug.Log("✅ Connected to host!");
            UpdateStatus("Connected! Waiting for battle...");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"🔌 Client {clientId} disconnected");

        if (clientId == networkManager.LocalClientId)
        {
            // Local client bị disconnect
            OnMatchmakingFailed("Disconnected from server");
        }
        else if (networkManager.IsHost)
        {
            // Opponent disconnected
            Debug.Log("⚠️ Opponent disconnected. AI will take over.");
        }
    }

    private void OnMatchmakingFailed(string reason)
    {
        Debug.LogError($"❌ Matchmaking failed: {reason}");

        isSearching = false;

        UpdateStatus($"Error: {reason}");

        // Show retry button
        StartCoroutine(ReturnToMenuAfterDelay(3f));
    }

    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (searchingPanel) searchingPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
    }

    // ===========================
    // 🎨 UI UPDATES
    // ===========================
    private void UpdateStatus(string message)
    {
        if (statusText)
        {
            statusText.text = message;
        }
        Debug.Log($"📢 Status: {message}");
    }

    private void UpdateCountdown(float timeRemaining)
    {
        if (countdownText)
        {
            countdownText.text = $"Timeout in: {Mathf.CeilToInt(timeRemaining)}s";
        }
    }

    // ===========================
    // 🧪 DEBUG HELPERS
    // ===========================
    [ContextMenu("Debug: Start as Host")]
    public void DebugStartHost()
    {
        StartHost();
    }

    [ContextMenu("Debug: Start as Client")]
    public void DebugStartClient()
    {
        TryJoinExistingServer();
    }

    [ContextMenu("Debug: Shutdown")]
    public void DebugShutdown()
    {
        networkManager.Shutdown();
    }
}

// ===========================
// 📱 UI HELPER COMPONENT
// ===========================
/// <summary>
/// Gắn vào buttons trong Main Menu
/// </summary>
public class MatchmakingButton : MonoBehaviour
{
    private MatchmakingManager matchmaking;

    void Start()
    {
        matchmaking = FindObjectOfType<MatchmakingManager>();
    }

    public void OnPvPClicked()
    {
        matchmaking?.FindPvPMatch();
    }

    public void OnPvAIClicked()
    {
        matchmaking?.PlayVsAI();
    }

    public void OnQuickMatchClicked()
    {
        matchmaking?.QuickMatch();
    }

    public void OnCancelClicked()
    {
        matchmaking?.CancelSearch();
    }
}