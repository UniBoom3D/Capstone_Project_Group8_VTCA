using UnityEngine;
using UnityEngine.UI;

public class PowerBarDisplay : MonoBehaviour
{
    private Slider powerSlider;

    [Header("Settings")]
    public Image maskFillImage;

    void Start()
    {
        powerSlider = GetComponent<Slider>();

        
    }

    private void LateUpdate()
    {
        UpdateMask(powerSlider.value);

        powerSlider.onValueChanged.AddListener(UpdateMask);
    }
    void UpdateMask(float value)
    {
        if (maskFillImage != null)
        {
            // Đảo ngược logic: 
            // Slider 0 (Hết máu) -> FillAmount 1 (Che hết)
            // Slider 1 (Đầy máu) -> FillAmount 0 (Lộ hết)
            maskFillImage.fillAmount = 1f - powerSlider.normalizedValue;
        }
    }
}