using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBattleTurtleScene : MonoBehaviour
{
    public void LoadBattleScene()
    {
        Debug.Log("⚔️ Đang chuyển sang BattleTurtleScene...");
        // Đảm bảo tên Scene chính xác tuyệt đối với Build Settings
        SceneManager.LoadScene("BattleTurtleScene");
    }
}
