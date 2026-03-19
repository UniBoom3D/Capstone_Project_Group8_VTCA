using UnityEngine;
using UnityEngine.UI;
using LunarCatsStudio.Compass;

public class PlayerUISetup : MonoBehaviour
{
    public void SetupUI(GameObject player, GameObject canvas, int playerIndex)
    {
        PlayerBattleController controller = player.GetComponent<PlayerBattleController>();
        PlayerHeading heading = player.GetComponent<PlayerHeading>();
        BattleHandlerPvE pveHandler = Object.FindFirstObjectByType<BattleHandlerPvE>();

        if (canvas == null) return;

        // 1. Thiết lập Controller & Timer
        if (controller != null)
        {
            // 1. Gán YourTurnText (Tìm theo Tag "NotifyTurn")
            GameObject notifyObj = FindGameObjectInChildWithTag(canvas, "NotifyTurn");
            if (notifyObj != null)
            {
                controller.SetNotifyUI(notifyObj); // Gọi hàm gán bên Controller
                notifyObj.SetActive(false); // Đảm bảo ẩn lúc mới spawn
            }

            // 2. Gán Timer và ẩn đi
            Timer uiTimer = canvas.GetComponentInChildren<Timer>();
            if (uiTimer != null)
            {
                controller.turnTimer = uiTimer;
                uiTimer.gameObject.SetActive(false); // Luôn ẩn khi mới spawn

                // Kết nối Event kết thúc lượt
                if (pveHandler != null)
                {
                    uiTimer.onTimerEnd.RemoveAllListeners();
                    uiTimer.onTimerEnd.AddListener(pveHandler.ForceEndTurn);
                }
            }

            // 3. Gán Power Slider và ẩn đi
            Slider powerBar = FindComponentInChildWithTag<Slider>(canvas, "PowerBar");
            if (powerBar != null)
            {
                controller.powerSlider = powerBar;
                powerBar.gameObject.SetActive(false);
            }
        }

        // 2. Thiết lập Compass (Giữ nguyên logic của bạn)
        if (heading != null)
        {
            GameObject compassObj = FindGameObjectInChildWithTag(canvas, "CompassbarX");
            if (compassObj != null)
            {
                heading._compass = compassObj.GetComponent<ICompassBarPro>();
                Debug.Log($"<color=yellow>🧭 Compass đã được gán cho {player.name}</color>");
            }
        }
    }

    // --- Helper Methods ---

    private T FindComponentInChildWithTag<T>(GameObject parent, string tag) where T : Component
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag(tag)) return t.GetComponent<T>();
        }
        return null;
    }

    private GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag(tag)) return t.gameObject;
        }
        return null;
    }
}