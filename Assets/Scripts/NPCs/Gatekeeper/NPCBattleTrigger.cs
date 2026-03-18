using UnityEngine;

public class NPCBattleTrigger : MonoBehaviour
{
    [Header("UI Reference (Player's Canvas)")]
    [Tooltip("Kéo 'ConversationPanel' từ Canvas của Player vào đây")]
    public GameObject playerConversationPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Hiện UI trò chuyện trên Canvas của Player
            if (playerConversationPanel != null)
            {
                playerConversationPanel.SetActive(true);
            }

            // 2. Mở khóa chuột để tương tác với phím OK
            var controller = other.GetComponent<CharacterOverMapController>();
            if (controller != null)
            {
                controller.ToggleControl(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tắt bảng chat nếu player đi xa khỏi NPC
            if (playerConversationPanel != null)
            {
                playerConversationPanel.SetActive(false);
            }

            var controller = other.GetComponent<CharacterOverMapController>();
            if (controller != null)
            {
                controller.ToggleControl(true);
            }
        }
    }
}