using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleHandlerPvE : BattleManagerCore
{
    public static BattleHandlerPvE Instance { get; private set; }

    [Header("TEAMS (DYNAMIC)")]
    [Tooltip("Managed by BattleManager script.")]
    public List<BattleTeamData> battleTeams = new List<BattleTeamData>();
    private int currentTeamIndex = 0;

    [Header("UI ANNOUNCEMENT")]
    [SerializeField] private GameObject turnNotifyText;

    [Header("ACTIVE ACTOR")]
    private PlayerBattleController _activePlayerInTurn;

    [Header("RULES")]
    [SerializeField] private int limitedTurnForPlayer = 20;
    [SerializeField] private float phaseDelay = 1f;
    [SerializeField] private float aiTurnWaitTime = 3.0f;

    [Header("PLAYER TURN CONFIG")]
    [SerializeField] private float endTurnAfterShootDelay = 0.1f;
    

    [Header("CAMERA")]
    [SerializeField] private Camera mainCamera;

    [Header("AI CONFIG")]
    [SerializeField] private CameraLookEnemyAction cameraAction;
    [SerializeField] private Transform enemyContainer;


    public ITurnParticipant CurrentActor { get; private set; }
    public string currentTeamName;

    private bool awaitingPlayerAction;
    private int playerTurnCount;
    private bool playerWinResult;
    private Coroutine phaseRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        LevelSpawner spawner = FindFirstObjectByType<LevelSpawner>();
        IntroBattlePVE intro = FindFirstObjectByType<IntroBattlePVE>();

        if (spawner != null)
        {
            spawner.OnSpawningComplete += () => {
                intro.PlayIntro();
            };
        }
    }
    
    // =========================
    // Public API
    // =========================
    public void StartBattlePVE(BattleTeamData blue, BattleTeamData red)
    {
        // BƯỚC QUAN TRỌNG: Nạp dữ liệu vào List của lớp Con trước
        battleTeams.Clear();
        battleTeams.Add(blue);
        battleTeams.Add(red);

        // Sau đó mới gọi StartBattle của lớp Cha
        base.StartBattle(blue, red, BattleState3D.BlueTeamTurn);
    }
   
  

    protected override IEnumerator OnBattleStartIntro()
    {
        yield return null;
    }

    protected override void OnStateEnter(BattleState3D state)
    {
        if (state == BattleState3D.BlueTeamTurn)
        {
            // Khi Intro xong, State chuyển sang BlueTeamTurn, lúc này mới hiện đồng hồ
            var firstPlayer = BlueTeam.Members.FirstOrDefault() as PlayerBattleController;
            if (firstPlayer != null && firstPlayer.turnTimer != null)
            {
                firstPlayer.turnTimer.gameObject.SetActive(true);
            }

            if (phaseRoutine == null)
                phaseRoutine = StartCoroutine(MasterBattleLoop());
        }       
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
        yield return new WaitForSeconds(1f);
        if (battleTeams == null || battleTeams.Count == 0)
        {
            Debug.LogError("❌ BattleTeams chưa có dữ liệu!");
            yield break;
        }
        while (isBattleActive)
        {
            var currentTeam = battleTeams[currentTeamIndex];
            currentTeamName = currentTeam.TeamName;

            bool isPlayerTeam = currentTeam.Members.Any(m => m is PlayerBattleController);
            if (isPlayerTeam)
            {
                playerTurnCount++;
                if (playerTurnCount > limitedTurnForPlayer)
                {
                    playerWinResult = false;
                    EndBattle();
                    yield break;
                }
            }

            foreach (var actor in currentTeam.Members.Where(m => m != null && m.IsAlive))
            {
                if (!isBattleActive) yield break;

                CurrentActor = actor;

                // Đảm bảo Camera quay về Actor NGAY LẬP TỨC
                if (actor is MonoBehaviour monoActor)
                {
                    FocusCamera(monoActor.transform);
                }
                //if (actor is MonoBehaviour monoActor)
                //    FocusCamera(monoActor.transform);

                // Nghỉ một chút để Camera kịp lướt (Blend) về nhân vật trước khi cho phép hành động
                yield return new WaitForSeconds(1.0f);

                // Xác định PlayerTarget
                PlayerBattleController playerTarget = actor as PlayerBattleController;
                if (playerTarget == null && actor is MonoBehaviour m)
                    playerTarget = m.GetComponent<PlayerBattleController>();

                if (playerTarget != null)
                {
                    yield return StartCoroutine(HandlePlayerTurn(playerTarget));
                }
                else
                {
                    yield return StartCoroutine(HandleAITurn(actor));
                }

                if (CheckBattleEndCondition()) yield break;
            }

            if (phaseDelay > 0f) yield return new WaitForSeconds(phaseDelay);
            currentTeamIndex = (currentTeamIndex + 1) % battleTeams.Count;
        }
        yield return null;
    }

    // =========================
    // Turn Logic Handlers
    // =========================

    private IEnumerator PlayerTurnSequence(PlayerBattleController player)
    {
        // 1. Hiển thị UI đồng hồ nhưng kim đứng yên
        if (player.turnTimer != null)
        {
            player.turnTimer.PrepareNewTurn();
            // Bạn có thể chạy thêm Coroutine Scale UI đồng hồ ở đây cho đẹp
        }

        // 2. Hiện thông báo Text "Lượt của bạn!"
        if (turnNotifyText != null)
        {
            turnNotifyText.SetActive(true);
            // Đợi 1.5s để người chơi kịp nhìn
            yield return new WaitForSeconds(1.5f);
            turnNotifyText.SetActive(false);
        }

        // 3. KÍCH HOẠT: Lúc này đồng hồ mới bắt đầu chạy và người chơi mới được bắn
        if (player.turnTimer != null)
        {
            player.turnTimer.ResumeTimer();
        }
        player.EnableControl(true);
    }

    private IEnumerator HandlePlayerTurn(PlayerBattleController player)
    {
        isActionDone = false;
        awaitingPlayerAction = true;
        _activePlayerInTurn = player;

        // Thay vì bật timer trực tiếp, hãy chạy sequence thông báo
        yield return StartCoroutine(PlayerTurnSequence(player));

        HookPlayerShootEvent(true);

        while (!isActionDone && isBattleActive)
        {
            // Logic Cheat phím O
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("⏳ Cheat: Ending turn in 5s...");
                _activePlayerInTurn.EnableControl(false);
                HookPlayerShootEvent(false);
                awaitingPlayerAction = false;
                StartCoroutine(EndUnitTurnAfterDelay(5f));
                break;
            }
            yield return null;
        }

        // Dọn dẹp sau khi xong lượt
        if (_activePlayerInTurn != null)
        {
            if (_activePlayerInTurn.turnTimer != null)
            {
                _activePlayerInTurn.turnTimer.StopTimer();
                _activePlayerInTurn.turnTimer.gameObject.SetActive(false);
            }
            _activePlayerInTurn.EnableControl(false);
        }

        HookPlayerShootEvent(false);
        awaitingPlayerAction = false;
        _activePlayerInTurn = null;
    }

    public void ForceEndTurn()
    {
        if (awaitingPlayerAction && isBattleActive && _activePlayerInTurn != null)
        {
            Debug.Log($"⏰ [TIMEOUT] {_activePlayerInTurn.name} hết giờ!");
            _activePlayerInTurn.EnableControl(false);
            if (_activePlayerInTurn.turnTimer != null) _activePlayerInTurn.turnTimer.StopTimer();

            awaitingPlayerAction = false;
            isActionDone = true;
        }
    }

  
    private IEnumerator HandleAITurn(ITurnParticipant aiActor)
    {
        // Kiểm tra nếu đây là nhân vật sống sót đầu tiên của Team
        if (aiActor == CurrentTeamLead())
        {
            // Thực hiện lượt cho toàn bộ quái trong Container
            yield return StartCoroutine(ExecuteAllEnemyTurns(enemyContainer));
        }
        else
        {
            // Các con quái sau trong cùng team sẽ được Skip vì Lead đã điều khiển rồi
            yield return null;
        }
    }
    public IEnumerator ExecuteAllEnemyTurns(Transform container)
    {
        if (container == null) yield break;
        var allEnemies = container.GetComponentsInChildren<TurtleEnemyAction>();

        // 1. Ưu tiên rùa cận chiến (Warrior)
        foreach (var enemy in allEnemies.Where(e => !e.IsTurtleCanon && e.IsAlive))
        {
            if (cameraAction != null) cameraAction.SetEnemyTarget(enemy);
            yield return StartCoroutine(RunEnemyTurn(enemy));
        }

        // 2. Sau đó đến rùa pháo thủ (Canon)
        foreach (var enemy in allEnemies.Where(e => e.IsTurtleCanon && e.IsAlive))
        {
            if (cameraAction != null) cameraAction.SetEnemyTarget(enemy);
            yield return StartCoroutine(RunEnemyTurn(enemy));
        }
    }

    private ITurnParticipant CurrentTeamLead()
    {
        return battleTeams[currentTeamIndex].Members.FirstOrDefault(m => m.IsAlive);
    }
    private IEnumerator RunEnemyTurn(TurtleEnemyAction enemy)
    {
        // 1. Kích hoạt AI
        enemy.TakeTurn();

        // 2. Chờ hành động hoàn tất
        // Nếu là rùa bắn (Canon), ta cần đợi đến khi đạn nổ
        if (enemy.IsTurtleCanon)
        {
            // Ta cần đợi một chút để đạn được Instantiate
            yield return new WaitForSeconds(0.5f);

            // Tìm viên đạn vừa bắn ra (đạn của Enemy)
            GameObject lastProjectile = GameObject.FindObjectOfType<ProjectileEnemy>()?.gameObject;
            while (lastProjectile != null)
            {
                yield return null;
            }
        }
        else // Nếu là rùa cận chiến
        {
            yield return new WaitForSeconds(1.5f); // Đợi animation cắn xong
        }

        yield return new WaitForSeconds(0.5f); // Nghỉ ngắn giữa các con rùa
    }

    private bool CheckBattleEndCondition()
    {
        int activeTeamsCount = battleTeams.Count(t => !t.IsDefeated);
        BattleTeamData winnerTeam = battleTeams.FirstOrDefault(t => !t.IsDefeated);

        if (activeTeamsCount <= 1)
        {
            playerWinResult = winnerTeam != null && winnerTeam.Members.Any(m => m is PlayerBattleController);
            EndBattle();
            return true;
        }
        return false;
    }

    // =========================
    // Shoot hook
    // =========================
    private void HookPlayerShootEvent(bool hook)
    {
        if (_activePlayerInTurn == null) return;
        _activePlayerInTurn.OnShoot -= OnPlayerShoot;
        if (hook) _activePlayerInTurn.OnShoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(Projectile projectile)
    {
        if (!awaitingPlayerAction) return;

        if (_activePlayerInTurn != null && _activePlayerInTurn.turnTimer != null)
            _activePlayerInTurn.turnTimer.StopTimer();

        awaitingPlayerAction = false;

        CameraFollowPlayer camControl = Object.FindFirstObjectByType<CameraFollowPlayer>();
        if (camControl != null && projectile != null)
        {
            //camControl.SetProjectileTarget(projectile.transform);
        }
        StartCoroutine(WaitUntilProjectileDestroyed(projectile));
    }

    private IEnumerator EndUnitTurnAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        isActionDone = true;
    }
    private IEnumerator WaitUntilProjectileDestroyed(Projectile projectile)
    {
        // 1. Chờ đạn bay
        while (projectile != null)
        {
            yield return null;
        }
        // 2. Đạn nổ
        yield return new WaitForSeconds(1.5f);

        // 3. Kết thúc lượt
        isActionDone = true;
    }

    // =========================
    // Helpers
    // =========================
    private void FocusCamera(Transform follow)
    {
        CameraFollowPlayer camControl = Object.FindFirstObjectByType<CameraFollowPlayer>();
        if (camControl != null)
        {
            //camControl.SetTarget(follow);
            // Nếu dùng Cinemachine, hãy đảm bảo Priority của Camera Player > Camera Intro lúc này
        }
    }

    private void Cleanup()
    {
        if (_activePlayerInTurn != null) _activePlayerInTurn.EnableControl(false);
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