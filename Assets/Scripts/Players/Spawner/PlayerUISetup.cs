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
            // Gán Power Slider theo Tag "PowerBar"
            Slider powerBar = FindComponentInChildWithTag<Slider>(canvas, "PowerBar");
            if (powerBar != null) controller.powerSlider = powerBar;

            // Gán Countdown Timer và kết nối với BattleHandler
            Timer uiTimer = canvas.GetComponentInChildren<Timer>();
            if (uiTimer != null)
            {
                controller.turnTimer = uiTimer;

                // ĐỒNG BỘ HÓA: Khi Timer này hết giờ, gọi hàm ForceEndTurn của Handler
                if (pveHandler != null)
                {
                    uiTimer.onTimerEnd.RemoveAllListeners(); // Xóa sạch để tránh trùng lặp khi hồi sinh/spawn lại
                    uiTimer.onTimerEnd.AddListener(pveHandler.ForceEndTurn);

                    Debug.Log($"<color=green>🔔 Link Timer của {player.name} tới BattleHandler thành công!</color>");
                }
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