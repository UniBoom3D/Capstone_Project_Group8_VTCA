using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Base class cho tất cả Player trong multiplayer battle
/// Hỗ trợ cả Local Player, Remote Player, và AI
/// </summary>
public class NetworkBattlePlayer : NetworkBehaviour
{
    [Header("Player Identity")]
    public NetworkVariable<PlayerType> playerType = new NetworkVariable<PlayerType>(PlayerType.LocalPlayer);
    public NetworkVariable<int> teamID = new NetworkVariable<int>(0); // 0=Blue, 1=Red

    [Header("Stats (Synced)")]
    public NetworkVariable<int> currentHP = new NetworkVariable<int>(100);
    public NetworkVariable<int> maxHP = new NetworkVariable<int>(100);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    [Header("Turn State")]
    public NetworkVariable<bool> hasActed = new NetworkVariable<bool>(false);
    public NetworkVariable<float> turnTimeRemaining = new NetworkVariable<float>(0);

    [Header("Components")]
    protected PlayerController humanController;
    protected AIPlayerController aiController;
    protected CharacterController characterController;

    [Header("VFX")]
    public GameObject damageEffect;
    public GameObject deathEffect;

    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDied;
    public System.Action<Projectile> OnProjectileFired;

    // ===========================
    // 🔧 INITIALIZATION
    // ===========================
    protected virtual void Awake()
    {
        humanController = GetComponent<PlayerController>();
        aiController = GetComponent<AIPlayerController>();
        characterController = GetComponent<CharacterController>();

        // Disable cả 2 controller ban đầu
        if (humanController) humanController.enabled = false;
        if (aiController) aiController.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to network variable changes
        currentHP.OnValueChanged += OnHPChanged;
        isAlive.OnValueChanged += OnAliveChanged;

        // Setup controller based on player type
        SetupController();

        Debug.Log($"🌐 NetworkBattlePlayer spawned: {playerType.Value} | Team: {teamID.Value} | IsOwner: {IsOwner}");
    }

    public override void OnNetworkDespawn()
    {
        currentHP.OnValueChanged -= OnHPChanged;
        isAlive.OnValueChanged -= OnAliveChanged;

        base.OnNetworkDespawn();
    }

    // ===========================
    // 🎮 CONTROLLER SETUP
    // ===========================
    private void SetupController()
    {
        switch (playerType.Value)
        {
            case PlayerType.LocalPlayer:
                if (IsOwner)
                {
                    SetupLocalPlayer();
                }
                else
                {
                    SetupRemotePlayer();
                }
                break;

            case PlayerType.RemotePlayer:
                SetupRemotePlayer();
                break;

            case PlayerType.AIPlayer:
                SetupAIPlayer();
                break;
        }
    }

    private void SetupLocalPlayer()
    {
        if (humanController)
        {
            humanController.enabled = true;
            humanController.EnableControl(false); // Đợi đến lượt
            humanController.OnShoot += OnLocalPlayerShoot;
        }

        if (aiController) aiController.enabled = false;

        Debug.Log("✅ Setup Local Player");
    }

    private void SetupRemotePlayer()
    {
        // Remote player chỉ nhận sync data, không điều khiển
        if (humanController) humanController.enabled = false;
        if (aiController) aiController.enabled = false;

        // Có thể disable physics để tránh conflict
        if (characterController) characterController.enabled = false;

        Debug.Log("📡 Setup Remote Player (sync only)");
    }

    private void SetupAIPlayer()
    {
        if (IsServer) // AI chỉ chạy trên Server
        {
            if (aiController)
            {
                aiController.enabled = true;
                aiController.EnableControl(false);
                aiController.OnShoot += OnAIPlayerShoot;
            }

            if (humanController) humanController.enabled = false;

            Debug.Log("🤖 Setup AI Player (Server)");
        }
        else
        {
            // Client chỉ nhận sync
            if (humanController) humanController.enabled = false;
            if (aiController) aiController.enabled = false;
            if (characterController) characterController.enabled = false;
        }
    }

    // ===========================
    // ⚔️ TURN MANAGEMENT
    // ===========================
    /// <summary>
    /// Server gọi khi đến lượt player này
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartTurnServerRpc()
    {
        hasActed.Value = false;
        turnTimeRemaining.Value = 20f;

        // Notify client để bật điều khiển
        StartTurnClientRpc();
    }

    [ClientRpc]
    private void StartTurnClientRpc()
    {
        if (playerType.Value == PlayerType.LocalPlayer && IsOwner)
        {
            humanController?.EnableControl(true);
            Debug.Log("🎮 Your turn!");
        }
        else if (playerType.Value == PlayerType.AIPlayer && IsServer)
        {
            aiController?.EnableControl(true);
            Debug.Log("🤖 AI turn start");
        }
    }

    /// <summary>
    /// Server gọi khi hết lượt
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        hasActed.Value = true;

        EndTurnClientRpc();
    }

    [ClientRpc]
    private void EndTurnClientRpc()
    {
        if (playerType.Value == PlayerType.LocalPlayer && IsOwner)
        {
            humanController?.EnableControl(false);
            Debug.Log("⏸️ Turn ended");
        }
        else if (playerType.Value == PlayerType.AIPlayer && IsServer)
        {
            aiController?.EnableControl(false);
        }
    }

    // ===========================
    // 💥 SHOOTING
    // ===========================
    private void OnLocalPlayerShoot(Projectile projectile)
    {
        if (!IsOwner) return;

        // Gửi request bắn đến server
        Vector3 spawnPos = projectile.transform.position;
        Quaternion spawnRot = projectile.transform.rotation;
        float power = 50f; // Lấy từ humanController

        RequestShootServerRpc(spawnPos, spawnRot, power);

        // Destroy local projectile (server sẽ spawn networked version)
        Destroy(projectile.gameObject);
    }

    private void OnAIPlayerShoot(Projectile projectile)
    {
        if (!IsServer) return;

        Vector3 spawnPos = projectile.transform.position;
        Quaternion spawnRot = projectile.transform.rotation;
        float power = 50f; // Lấy từ aiController

        SpawnNetworkProjectile(spawnPos, spawnRot, power);

        // Destroy local projectile
        Destroy(projectile.gameObject);
    }

    [ServerRpc]
    private void RequestShootServerRpc(Vector3 position, Quaternion rotation, float power)
    {
        // Validate shoot request
        if (!ValidateShoot(position, power))
        {
            Debug.LogWarning($"⚠️ Invalid shoot request from {OwnerClientId}");
            return;
        }

        SpawnNetworkProjectile(position, rotation, power);

        // Mark turn as completed
        hasActed.Value = true;
    }

    private void SpawnNetworkProjectile(Vector3 position, Quaternion rotation, float power)
    {
        // TODO: Spawn NetworkProjectile prefab
        // GameObject projObj = Instantiate(networkProjectilePrefab, position, rotation);
        // NetworkObject netObj = projObj.GetComponent<NetworkObject>();
        // netObj.Spawn();

        Debug.Log($"🚀 Network projectile spawned at {position} with power {power}");
    }

    private bool ValidateShoot(Vector3 position, float power)
    {
        // Validate:
        // 1. Is it this player's turn?
        if (hasActed.Value) return false;

        // 2. Is position reasonable? (not teleported)
        float distance = Vector3.Distance(transform.position, position);
        if (distance > 10f) return false;

        // 3. Is power valid?
        if (power < 0 || power > 100f) return false;

        return true;
    }

    // ===========================
    // 💔 DAMAGE & HEALTH
    // ===========================
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, ulong attackerClientId)
    {
        if (!isAlive.Value) return;

        int actualDamage = Mathf.Max(damage, 0);
        currentHP.Value -= actualDamage;

        if (currentHP.Value <= 0)
        {
            currentHP.Value = 0;
            isAlive.Value = false;
        }

        // Notify all clients to show damage effect
        ShowDamageEffectClientRpc(actualDamage);

        Debug.Log($"💔 {gameObject.name} took {actualDamage} damage. HP: {currentHP.Value}/{maxHP.Value}");
    }

    [ClientRpc]
    private void ShowDamageEffectClientRpc(int damage)
    {
        // Show damage VFX
        if (damageEffect)
        {
            Instantiate(damageEffect, transform.position + Vector3.up * 2f, Quaternion.identity);
        }

        // Show damage number (TODO: implement FloatingText)
        Debug.Log($"💥 -{damage}");
    }

    private void OnHPChanged(int oldValue, int newValue)
    {
        OnHealthChanged?.Invoke(newValue);

        if (newValue <= 0 && oldValue > 0)
        {
            HandleDeath();
        }
    }

    private void OnAliveChanged(bool oldValue, bool newValue)
    {
        if (!newValue && oldValue)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"💀 {gameObject.name} died!");

        // Show death effect
        if (deathEffect)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Disable controllers
        if (humanController) humanController.EnableControl(false);
        if (aiController) aiController.EnableControl(false);

        OnPlayerDied?.Invoke();

        // Optional: Fade out, ragdoll, etc.
    }

    // ===========================
    // 📊 UTILITY
    // ===========================
    public bool IsMyTurn()
    {
        return !hasActed.Value && turnTimeRemaining.Value > 0;
    }

    public float GetHealthPercent()
    {
        return (float)currentHP.Value / maxHP.Value;
    }

    public string GetDisplayName()
    {
        string typeStr = playerType.Value == PlayerType.AIPlayer ? " (AI)" : "";
        string teamStr = teamID.Value == 0 ? "Blue" : "Red";
        return $"{teamStr} Team{typeStr}";
    }
}

// ===========================
// 📋 ENUMS
// ===========================
public enum PlayerType
{
    LocalPlayer,    // Người chơi hiện tại trên máy này
    RemotePlayer,   // Người chơi từ xa
    AIPlayer        // AI bot
}