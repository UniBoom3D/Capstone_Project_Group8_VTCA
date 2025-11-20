using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CreateCharacterDisplay : MonoBehaviour
{
    [Header("Model References")]
    public List<GameObject> classModels; 

    [Header("UI References")]
    public TMP_Text classNameText;

    // Tên class tương ứng với index của model
    [SerializeField] private string[] classNames = { "Archer", "Gunner", "Mage" };

    private int currentIndex = 0;

    private void Start()
    {
        if (classModels == null || classModels.Count == 0)
        {
            Debug.LogError("❌ Chưa gán model vào CreateCharacterDisplay!");
            return;
        }

        if (classNames.Length != classModels.Count)
        {
            Debug.LogWarning("Số classNames không khớp số model, sẽ lấy theo min.");
        }

        UpdateDisplay();
    }

    private void Update()
    {
        if (classModels == null || classModels.Count == 0) return;

        var model = classModels[currentIndex];
        if (model == null) return;

        Animator anim = model.GetComponent<Animator>();
        if (anim == null)
        {
            model.transform.Rotate(Vector3.up * 20f * Time.deltaTime);
        }
    }

    // =========================
    // 🔄 Điều khiển bằng button
    // =========================
    public void OnClickNextClass()
    {
        if (classModels.Count == 0) return;

        currentIndex = (currentIndex + 1) % classModels.Count;
        UpdateDisplay();
    }

    public void OnClickPreviousClass()
    {
        if (classModels.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = classModels.Count - 1;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < classModels.Count; i++)
        {
            if (classModels[i] != null)
            {
                classModels[i].SetActive(i == currentIndex);
                if (i == currentIndex)
                    classModels[i].transform.rotation = Quaternion.identity;
            }
        }


        if (classNameText != null && classNames.Length > 0)
        {
            int safeIndex = Mathf.Clamp(currentIndex, 0, classNames.Length - 1);
            classNameText.text = classNames[safeIndex];
        }
    }

    public string GetClassName()
    {
        if (classNames == null || classNames.Length == 0)
            return "Unknown";

        int safeIndex = Mathf.Clamp(currentIndex, 0, classNames.Length - 1);
        return classNames[safeIndex];
    }
}
