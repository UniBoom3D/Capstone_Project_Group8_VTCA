using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
    [Header("References")]
    public Image dialImage;

    [Header("Color Settings")]
    public Color colorFull = Color.green;
    public Color colorHalf = Color.yellow;
    public Color colorCritical = Color.red;

    [Header("Blinking Settings")]
    public float blinkSpeed = 10f; // Tốc độ nhấp nháy
    private bool isBlinking = false;

    void Update()
    {
        if (dialImage == null) return;

        float fillValue = dialImage.fillAmount;

        // 1. Logic đổi màu theo %
        if (fillValue > 0.5f)
        {
            dialImage.color = colorFull;
            isBlinking = false;
        }
        else if (fillValue > 0.25f)
        {
            dialImage.color = colorHalf;
            isBlinking = false;
        }
        else
        {
            // Dưới 25% - Chuyển màu đỏ và bật nhấp nháy
            dialImage.color = colorCritical;
            isBlinking = true;
        }

        // 2. Logic nhấp nháy
        if (isBlinking)
        {
            BlinkEffect();
        }
    }

    void BlinkEffect()
    {
        // Sử dụng hàm Sin để tạo giá trị Alpha chạy từ 0.2 đến 1.0
        float alpha = Mathf.Lerp(0.2f, 1.0f, (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) / 2.0f);
        Color c = dialImage.color;
        c.a = alpha;
        dialImage.color = c;
    }
}