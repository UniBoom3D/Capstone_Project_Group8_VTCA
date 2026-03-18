using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCBattleTrigger : MonoBehaviour
{
    public GameObject dialogueCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Hiện UI trò chuyện
            if (dialogueCanvas != null) dialogueCanvas.SetActive(true);

            // 2. Gọi Controller để mở khóa chuột
            var controller = other.GetComponent<CharacterOverMapController>();
            if (controller != null) controller.ToggleControl(false);
        }
    }

    public void StartBattle()
    {
        // 3. Load scene chiến đấu
        // Lưu ý: Đảm bảo "BattleTurtleScene" đã add vào Build Settings
        SceneManager.LoadScene("BattleTurtleScene");
    }
}