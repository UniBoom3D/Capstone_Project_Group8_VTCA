using UnityEngine;
using TMPro;

public class CreateCharacterDisplay : MonoBehaviour
{
    [Header("🧍 Model References")]
    public GameObject ArcherModel;
    public GameObject GunnerModel;
    public GameObject MageModel;

    [Header("🧾 UI References")]
    public TMP_Text classNameText;

    private int currentIndex = 0;
    private readonly string[] classNames = { "Archer", "Gunner", "Mage" };
    private GameObject[] models;

    private void Start()
    {
        models = new GameObject[] { ArcherModel, GunnerModel, MageModel };
        UpdateDisplay();
    }

    private void Update()
    {
        // Xoay nhẹ model hiện tại cho hiệu ứng động
        if (models != null && models[currentIndex] != null)
            models[currentIndex].transform.Rotate(Vector3.up * 20f * Time.deltaTime);
    }

    /// <summary>
    /// Gọi từ CreateCharacterManager để thay đổi class được chọn.
    /// </summary>
    public void SetClassIndex(int index)
    {
        if (models == null) return;
        if (index < 0 || index >= models.Length) index = 0;
        currentIndex = index;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < models.Length; i++)
            models[i].SetActive(i == currentIndex);

        classNameText.text = classNames[currentIndex];
    }

    public string GetCurrentClassName()
    {
        return classNames[currentIndex];
    }
}
