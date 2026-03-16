using UnityEngine;
using UnityEngine.UI;
using LunarCatsStudio.Compass;

public class PlayerUISetup : MonoBehaviour
{
    public void SetupUI(GameObject player, GameObject canvas, int playerIndex)
    {
        PlayerBattleController controller = player.GetComponent<PlayerBattleController>();
        PlayerHeading heading = player.GetComponent<PlayerHeading>();

        if (canvas == null) return;

        // 1. Gán Power Slider theo Tag "PowerBar"
        if (controller != null)
        {
            Slider powerBar = FindComponentInChildWithTag<Slider>(canvas, "PowerBar");
            if (powerBar != null) controller.powerSlider = powerBar;

            // Gán Countdown Timer (Tìm theo Component Timer)
            Timer uiTimer = canvas.GetComponentInChildren<Timer>();
            if (uiTimer != null) controller.turnTimer = uiTimer;
        }

        // 2. Gán Compass (Thước xoay) cho PlayerHeading
        if (heading != null)
        {
            // Tìm Object có Tag CompassbarX
            GameObject compassObj = FindGameObjectInChildWithTag(canvas, "CompassbarX");
            if (compassObj != null)
            {
                // Lấy Component thực thi Interface ICompassBarPro
                heading._compass = compassObj.GetComponent<ICompassBarPro>();
                Debug.Log($"<color=yellow>🧭 Compass đã được gán cho {player.name}</color>");
            }
            else
            {
                Debug.LogWarning($"⚠️ Không tìm thấy Object có Tag 'CompassbarX' trong {canvas.name}");
            }
        }

        // 3. Gán đồng hồ cho BattleHandlerPvE (chỉ cho người chơi đầu tiên)
        if (playerIndex == 1)
        {
            BattleHandlerPvE pveHandler = Object.FindFirstObjectByType<BattleHandlerPvE>();
            Timer uiTimer = canvas.GetComponentInChildren<Timer>();
            // if (pveHandler != null && uiTimer != null) pveHandler.countdownTimer = uiTimer;
        }
    }

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