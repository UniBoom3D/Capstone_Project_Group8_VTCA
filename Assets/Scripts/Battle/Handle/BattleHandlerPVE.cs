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

    //private IEnumerator InitialBattleSequence()
    //{
    //    // --- BƯỚC 1: SPAWN PLAYER & UI SCALE ---
    //    currentTeamName = "Summoning Players...";

    //    // Lấy Camera Player để chuẩn bị lướt
    //    CameraFollowPlayer playerCam = Object.FindFirstObjectByType<CameraFollowPlayer>();
    //    //if (cameraAction != null && cameraAction.Vcam != null)
    //    //{
    //    //    cameraAction.Vcam.Priority = 30; // Đẩy cam Intro lên cao nhất
    //    //}

    //    foreach (var team in battleTeams)
    //    {
    //        foreach (var member in team.Members.OfType<PlayerBattleController>())
    //        {
    //            // Hiệu ứng scale nhân vật
    //            StartCoroutine(PerformSpawnEffect(member.gameObject));
    //            // Hiệu ứng UI scale 2 -> 1
    //            if (member.turnTimer != null)
    //            {
    //                member.turnTimer.gameObject.SetActive(true);
    //                StartCoroutine(ScaleCanvasRoutine(member.turnTimer.transform));
    //            }
    //        }
    //    }
    //    yield return new WaitForSeconds(1.0f);

    //    // --- BƯỚC 2: CHUYỂN CAMERA SANG ENEMY (MƯỢT MÀ 3 GIÂY) ---
    //    // Cinemachine Brain sẽ tự động blend 3s nếu bạn đã chỉnh trong Inspector
    //    //if (playerCam != null) playerCam.SetCameraPriority(5);

    //    // Kích hoạt Intro (Focus vào Enemy tiêu biểu)
    //    yield return StartCoroutine(OnBattleStartIntro());

    //    // --- BƯỚC 3: SPAWN ENEMY (HIỆN RA SAU KHI CAM ĐÃ NHÌN TỚI) ---
    //    var enemies = battleTeams.SelectMany(t => t.Members).OfType<TurtleEnemyAction>();
    //    foreach (var enemy in enemies)
    //    {
    //        StartCoroutine(PerformSpawnEffect(enemy.gameObject));
    //    }
    //    yield return new WaitForSeconds(1.0f);

    //    // --- BƯỚC 4: VÀO TRẬN ĐÁNH ---
    //    phaseRoutine = StartCoroutine(MasterBattleLoop());
    //}

    private IEnumerator ScaleCanvasRoutine(Transform uiTf)
    {
        uiTf.localScale = Vector3.one * 2f;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            uiTf.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, t / 0.5f);
            yield return null;
        }
        uiTf.localScale = Vector3.one;
    }
    private IEnumerator PerformSpawnEffect(GameObject playerObj)
    {
        // Giả sử vòng tròn ánh sáng là một con của Player hoặc bạn Instantiate nó ra
        // Ở đây ta làm hiệu ứng Scale đơn giản cho Player:
        playerObj.transform.localScale = Vector3.zero;

        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            playerObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / duration);
            yield return null;
        }
        playerObj.transform.localScale = Vector3.one;
    }

    protected override IEnumerator OnBattleStartIntro()
    {
        Debug.Log("🎬 State: START - Performing Cinematic Intro...");
        awaitingPlayerAction = false;
        isBattleActive = false; // Khóa trận đấu trong lúc diễn

        // 1. MƯỢN CAMERA: Lấy FreeLook Camera để diễn
        var introVcam = cameraAction.GetComponentInChildren<Unity.Cinemachine.CinemachineCamera>();
        if (introVcam != null) introVcam.Priority = 30;

        // 2. DIỄN CHO PLAYER: Xuất hiện rùa và scale UI
        var firstPlayer = BlueTeam.Members.FirstOrDefault() as MonoBehaviour;
        if (firstPlayer != null && cameraAction != null)
        {
            cameraAction.SetEnemyTarget(firstPlayer.GetComponent<ITurnParticipant>() as TurtleEnemyAction);

            foreach (var p in BlueTeam.Members.OfType<PlayerBattleController>())
            {
                p.gameObject.SetActive(true);
                p.EnableControl(false); // Quan trọng: Khóa bắn trong lúc intro
                if (p.turnTimer != null)
                {
                    p.turnTimer.gameObject.SetActive(true);
                    StartCoroutine(ScaleCanvasRoutine(p.turnTimer.transform));
                    p.turnTimer.StopTimer(); // Đảm bảo đồng hồ chưa chạy
                }
                StartCoroutine(PerformSpawnEffect(p.gameObject));
            }
        }
        yield return new WaitForSeconds(1.5f);

        // 3. LƯỚT MÁY SANG ENEMY (3 GIÂY)
        var firstEnemy = RedTeam.Members.FirstOrDefault() as TurtleEnemyAction;
        if (firstEnemy != null) cameraAction.SetEnemyTarget(firstEnemy);
        yield return new WaitForSeconds(3.0f);

        // 4. DIỄN CHO ENEMY: Spawn rùa địch
        foreach (var e in RedTeam.Members)
        {
            if (e is MonoBehaviour m)
            {
                m.gameObject.SetActive(true);
                StartCoroutine(PerformSpawnEffect(m.gameObject));
            }
        }
        yield return new WaitForSeconds(1.0f);

        // 5. KẾT THÚC INTRO: Trả máy về cho Player Camera
        if (introVcam != null) introVcam.Priority = 5;
        isBattleActive = true; // Mở khóa trận đấu
        Debug.Log("🎬 Intro xong. Master Loop sẽ tự chạy qua OnStateEnter.");
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
        //if (state == BattleState3D.Endbattle) Cleanup();
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
                if (actor is MonoBehaviour monoActor)
                    FocusCamera(monoActor.transform);

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
    }

    // =========================
    // Turn Logic Handlers
    // =========================
    private IEnumerator HandlePlayerTurn(PlayerBattleController player)
    {
        isActionDone = false;
        awaitingPlayerAction = true;
        _activePlayerInTurn = player;

        // Kích hoạt Timer riêng của nhân vật
        if (_activePlayerInTurn.turnTimer != null)
        {
            _activePlayerInTurn.turnTimer.gameObject.SetActive(true);
            _activePlayerInTurn.turnTimer.StartNewTurn();
        }

        HookPlayerShootEvent(true);
        _activePlayerInTurn.EnableControl(true);

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
        aiActor.TakeTurn();
        yield return new WaitForSeconds(aiTurnWaitTime);
        CurrentActor = null;
        if (aiActor == CurrentTeamLead()) // Hàm phụ để kiểm tra xem đây có phải con đầu tiên của team ko
        {
            yield return StartCoroutine(ExecuteAllEnemyTurns(enemyContainer));
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
            camControl.SetProjectileTarget(projectile.transform);
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
            camControl.SetTarget(follow);
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