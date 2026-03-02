using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public TextMeshProUGUI percentText;

    private void Start()
    {
        UpdatePercentText(healthSlider.value);
    }

    // Gắn hàm này vào OnValueChanged trong Inspector
    public void UpdatePercentText(float value)
    {
        float percent = value / healthSlider.maxValue;
        percentText.text = Mathf.RoundToInt(percent * 100f) + "%";
    }
}
