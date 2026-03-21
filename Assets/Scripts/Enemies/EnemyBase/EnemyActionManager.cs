using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyActionManager : MonoBehaviour
{
    public static EnemyActionManager Instance { get; private set; }

    [Header("Tracking")]
    public List<TurtleEnemyAction> activeEnemies = new List<TurtleEnemyAction>();
    public int totalSpawnedCount = 0; // Số lượng đã từng xuất hiện
    public int maxEnemyPool = 20;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Tự động quét Scene để tìm quái nếu chưa có danh sách từ Spawner
    public void RefreshEnemyList()
    {
        // Tìm tất cả Object có Tag "Enemy" và lấy component TurtleEnemyAction
        var foundEnemies = GameObject.FindGameObjectsWithTag("Enemy")
            .Select(go => go.GetComponent<TurtleEnemyAction>())
            .Where(t => t != null && t.IsAlive && !t.IsEnemyDead)
            .ToList();

        activeEnemies = foundEnemies;

        // Cập nhật totalSpawnedCount dựa trên số lượng quái hiện có trong Scene lúc bắt đầu
        if (totalSpawnedCount == 0) totalSpawnedCount = activeEnemies.Count;
    }

    public void OnEnemyRemoved(TurtleEnemyAction enemy)
    {
        if (activeEnemies.Contains(enemy)) activeEnemies.Remove(enemy);

        // Nếu hết quái và vẫn còn hạn mức spawn
        if (activeEnemies.Count == 0 && totalSpawnedCount < maxEnemyPool)
        {
            Debug.Log("💀 All enemies dead. Ready for next wave.");
            // PvE sẽ gọi Spawn quái mới tại đây
        }
    }

    public IEnumerator ExecuteAllAITurns()
    {
        // Quét lại danh sách trước khi bắt đầu lượt để đảm bảo không sót con nào mới spawn
        RefreshEnemyList();

        if (activeEnemies.Count == 0)
        {
            Debug.Log("No enemies found to act.");
            yield break;
        }

        foreach (var e in activeEnemies) e.isEnemyActionDone = false;

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            // 🎥 Setup Camera tập trung vào con chuẩn bị hành động
            // Lưu ý: CameraLookEnemyAction của bạn sẽ chiếm quyền ưu tiên tại đây
            var camAction = Object.FindFirstObjectByType<CameraLookEnemyAction>();
            if (camAction != null)
            {
                camAction.SetEnemyTarget(enemy);
            }

            // Đợi camera blend một chút cho mượt
            yield return new WaitForSeconds(0.5f);

            enemy.TakeTurn();

            // Đợi rùa báo cáo hoàn thành hành động (di chuyển + bắn + đạn nổ)
            while (!enemy.isEnemyActionDone) yield return null;

            // Sau khi con này xong, Reset Camera trước khi sang con tiếp theo
            if (camAction != null) camAction.MarkActionDone();

            yield return new WaitForSeconds(3f);
        }

        Debug.Log("🛡️ All Enemy Actions Completed.");
    }
}