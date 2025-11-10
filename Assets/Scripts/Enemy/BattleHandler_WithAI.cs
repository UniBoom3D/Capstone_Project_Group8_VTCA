using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

// Đoạn code sẽ này thay thế cho scripts BattleHandler_TurnBaseCore.cs

/// <summary>
/// Mở rộng từ BattleHandler_TurnBaseCore
/// Thêm logic điều khiển AI và Player thật
/// </summary>
public class BattleHandler_WithAI : BattleHandler_TurnBaseCore
{
    [Header("Player References")]
    [SerializeField] private PlayerController humanPlayer;      // Player người chơi
    [SerializeField] private AIPlayerController aiPlayer;       // AI địch

    [Header("UI References")]
    [SerializeField] private GameObject turnIndicatorUI;        // UI hiển thị lượt
    [SerializeField] private UnityEngine.UI.Text turnText;      // Text "Your Turn" / "Enemy Turn"
    [SerializeField] private UnityEngine.UI.Slider powerBarUI;  // Thanh lực bắn

    private Projectile currentProjectile;
    private bool isWaitingForProjectile = false;

    // ===========================
    // 🔧 SETUP OVERRIDE
    // ===========================
    protected override void Awake()
    {
        base.Awake();

        // Tắt điều khiển ban đầu
        if (humanPlayer) humanPlayer.EnableControl(false);
        if (aiPlayer) aiPlayer.EnableControl(false);

        // Đăng ký sự kiện bắn đạn
        if (humanPlayer) humanPlayer.OnShoot += OnPlayerShoot;
        if (aiPlayer) aiPlayer.OnShoot += OnAIShoot;
    }

    protected override void SetupDummyData()
    {
        // Không dùng dummy data nữa, sử dụng Player thật
        BlueTeam = new BattleTeamData("Blue Team (Human)");
        RedTeam = new BattleTeamData("Red Team (AI)");

        if (humanPlayer != null)
        {
            var humanData = new PlayerInBattle(new PlayerProgressData()
            {
                _characterName = "Player",
                _health = 120,
                _attack = 25,
                _armor = 5
            });
            BlueTeam.AddMember(humanData);
        }

        if (aiPlayer != null)
        {
            var aiData = new PlayerInBattle(new PlayerProgressData()
            {
                _characterName = "AI Enemy",
                _health = 100,
                _attack = 20,
                _armor = 4
            });
            RedTeam.AddMember(aiData);
        }

        Debug.Log("🎮 Real Player & AI loaded");
    }

    // ===========================
    // ⚔️ TURN PHASE OVERRIDE
    // ===========================
    protected override void HandleTurn(BattleTeamData team, BattleState3D nextState)
    {
        if (team == null) return;
        if (team.IsDefeated)
        {
            EndBattle();
            return;
        }

        // Khởi tạo lượt mới
        if (currentTeamName != team.TeamName)
        {
            currentTeamName = team.TeamName;
            ActivateCamera(team.TeamName.Contains("Blue") ? _blueTeamCamera : _redTeamCamera);
            _turnSound?.Play();

            // Bật điều khiển cho đúng player
            bool isHumanTurn = team.TeamName.Contains("Blue");
            if (humanPlayer) humanPlayer.EnableControl(isHumanTurn);
            if (aiPlayer) aiPlayer.EnableControl(!isHumanTurn);

            // Cập nhật UI
            UpdateTurnUI(isHumanTurn);

            Debug.Log($"🎮 {team.TeamName}'s Turn Start!");
            turnTimer = _timePerTurn;
            isPlayerActionDone = false;
            isWaitingForProjectile = false;
        }

        // Cập nhật thanh lực cho người chơi
        if (team.TeamName.Contains("Blue") && humanPlayer != null && powerBarUI != null)
        {
            // Có thể lấy giá trị từ humanPlayer nếu expose biến currentChargePower
            // powerBarUI.value = humanPlayer.currentChargePower;
        }

        turnTimer -= Time.deltaTime;

        // Timeout - bỏ lượt
        if (turnTimer <= 0 && !isPlayerActionDone && !isWaitingForProjectile)
        {
            Debug.Log($"⏰ {team.TeamName} Timeout — Skip turn");
            _skipTurnSound?.Play();
            isPlayerActionDone = true;
            StartCoroutine(HandleAnimationPhase(nextState));
        }
    }

    // ===========================
    // 💥 SHOOT CALLBACKS
    // ===========================
    private void OnPlayerShoot(Projectile projectile)
    {
        Debug.Log("🔫 Player shot fired!");
        HandleShoot(projectile, BattleState3D.RedTeamTurn);
    }

    private void OnAIShoot(Projectile projectile)
    {
        Debug.Log("🤖 AI shot fired!");
        HandleShoot(projectile, BattleState3D.BlueTeamTurn);
    }

    private void HandleShoot(Projectile projectile, BattleState3D nextState)
    {
        if (isPlayerActionDone || isWaitingForProjectile) return;

        currentProjectile = projectile;
        isWaitingForProjectile = true;
        isPlayerActionDone = true;

        // Tắt điều khiển ngay sau khi bắn
        if (humanPlayer) humanPlayer.EnableControl(false);
        if (aiPlayer) aiPlayer.EnableControl(false);

        // Đăng ký event khi đạn nổ
        if (projectile != null)
        {
            projectile.OnExploded += () => OnProjectileExploded(nextState);
        }

        // Chuyển camera theo đạn
        StartCoroutine(FollowProjectile(projectile));
    }

    private void OnProjectileExploded(BattleState3D nextState)
    {
        Debug.Log("💥 Projectile exploded!");
        isWaitingForProjectile = false;
        StartCoroutine(HandleAnimationPhase(nextState));
    }

    // ===========================
    // 📷 FOLLOW PROJECTILE
    // ===========================
    private IEnumerator FollowProjectile(Projectile projectile)
    {
        if (_animationCamera == null || projectile == null) yield break;

        ActivateCamera(_animationCamera);

        // Theo dõi đạn cho đến khi nổ
        while (projectile != null && projectile.isLaunched)
        {
            _animationCamera.transform.position = projectile.transform.position + new Vector3(0, 5, -8);
            _animationCamera.transform.LookAt(projectile.transform.position);
            yield return null;
        }
    }

    // ===========================
    // 🎬 ANIMATION PHASE OVERRIDE
    // ===========================
    protected override IEnumerator HandleAnimationPhase(BattleState3D nextState)
    {
        currentState = BattleState3D.AnimationPlay;

        Debug.Log("🎬 Showing impact effects...");
        yield return new WaitForSeconds(_transitionDelay);

        // Kiểm tra điều kiện thắng thua
        if (BlueTeam.IsDefeated || RedTeam.IsDefeated)
        {
            EndBattle();
            yield break;
        }

        // Chuyển lượt
        currentState = nextState;
        currentTeamName = "";
        turnTimer = _timePerTurn;

        Debug.Log($"🔁 Switching turn → {currentState}");
    }

    // ===========================
    // 🎨 UI UPDATES
    // ===========================
    private void UpdateTurnUI(bool isHumanTurn)
    {
        if (turnText == null) return;

        if (isHumanTurn)
        {
            turnText.text = "YOUR TURN";
            turnText.color = Color.cyan;
        }
        else
        {
            turnText.text = "ENEMY TURN";
            turnText.color = Color.red;
        }

        if (turnIndicatorUI)
        {
            turnIndicatorUI.SetActive(true);
        }
    }

    // ===========================
    // 🏁 FINISH PHASE OVERRIDE
    // ===========================
    protected override void EndBattle()
    {
        base.EndBattle();

        // Tắt toàn bộ điều khiển
        if (humanPlayer) humanPlayer.EnableControl(false);
        if (aiPlayer) aiPlayer.EnableControl(false);

        // Hiển thị màn hình kết quả
        StartCoroutine(ShowResults());
    }

    private IEnumerator ShowResults()
    {
        yield return new WaitForSeconds(2f);

        bool humanWins = !BlueTeam.IsDefeated && RedTeam.IsDefeated;

        if (humanWins)
        {
            Debug.Log("🎉 VICTORY! You defeated the AI!");
            // TODO: Show victory screen
        }
        else
        {
            Debug.Log("💀 DEFEAT! The AI won!");
            // TODO: Show defeat screen
        }
    }

    // ===========================
    // 🧹 CLEANUP
    // ===========================
    private void OnDestroy()
    {
        if (humanPlayer) humanPlayer.OnShoot -= OnPlayerShoot;
        if (aiPlayer) aiPlayer.OnShoot -= OnAIShoot;
    }
}