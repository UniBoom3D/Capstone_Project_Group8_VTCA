using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CreateCharacterDisplay : MonoBehaviour
{
    [Header("Model References")]
    public List<GameObject> classModels;   // Archer, Gunner, Mage,...

    [Header("UI References")]
    public TMP_Text classNameText;

    private int currentIndex = 0;

    private readonly string[] classNames = { "Archer", "Gunner", "Mage" };

    private void Start()
    {
        if (classModels == null || classModels.Count == 0)
        {
            Debug.LogError("❌ Chưa gán model vào CreateCharacterDisplay!");
            return;
        }

        UpdateDisplay();
    }

    private void Update()
    {
        // Chỉ quay model khi chưa có Animator
        var model = classModels[currentIndex];
        Animator anim = model.GetComponent<Animator>();

        if (anim == null)
        {
            // Model placeholder → xoay nhẹ
            model.transform.Rotate(Vector3.up * 20f * Time.deltaTime);
        }
    }

    // ============================
    // 🔄 Chuyển class khi nhấn nút
    // ============================
    public void SetClassIndex(int index)
    {
        if (classModels.Count == 0) return;

        currentIndex = Mathf.Clamp(index, 0, classModels.Count - 1);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < classModels.Count; i++)
        {
            classModels[i].SetActive(i == currentIndex);

            // Reset rotation để không bị xoay lệch khi đổi model
            if (i == currentIndex)
                classModels[i].transform.rotation = Quaternion.identity;
        }

        classNameText.text = classNames[currentIndex];
    }

    public string GetCurrentClassName()
    {
        return classNames[currentIndex];
    }
}
