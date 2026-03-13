using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải có để thao tác với UI (Slider, Image)

[RequireComponent(typeof(Slider))]
public class PowerBarDisplay : MonoBehaviour
{
    private Slider powerSlider;

    [Tooltip("Kéo thả GameObject 'Fill' nằm bên trong Slider vào đây")]
    public Image fillImage;

    [Tooltip("Thiết lập dải màu. Bên trái (0) là Xanh lá, bên phải (1) là Đỏ")]
    public Gradient colorGradient;

    void Start()
    {
        // Lấy component Slider mà script này đang gắn vào
        powerSlider = GetComponent<Slider>();

        // Cập nhật màu sắc ngay lúc bắt đầu game
        UpdateColor(powerSlider.value);

        // Lắng nghe sự kiện: Mỗi khi value của Slider thay đổi, gọi hàm UpdateColor
        powerSlider.onValueChanged.AddListener(UpdateColor);
    }

    // Hàm cập nhật màu sắc dựa trên giá trị của Slider
    void UpdateColor(float value)
    {
        if (fillImage != null)
        {
            // normalizedValue tự động đưa giá trị Slider về khoảng 0 - 1 (phù hợp với Gradient)
            fillImage.color = colorGradient.Evaluate(powerSlider.normalizedValue);
        }
    }
}