using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;
using System.Collections.Generic;

public class CreateCharacterManager : MonoBehaviour
{
    [Header("🧾 UI References")]
    public TMP_InputField inputCharacterName;
    public TMP_Text checkNameNotice;
    public Button confirmButton;

    [Header("🎮 Navigation Buttons")]
    public Button previousButton;
    public Button nextButton;

    [Header("🧍 Character Display")]
    public CreateCharacterDisplay characterDisplay; // tham chiếu tới CreateCharacterDisplay trong scene

    private int currentIndex = 0;
    private readonly string[] classNames = { "Archer", "Gunner", "Mage" };
    private bool isNameValid = false;

    // ==========================================================
    private void Start()
    {
        // Gán sự kiện cho các nút UI
        previousButton.onClick.AddListener(PreviousClass);
        nextButton.onClick.AddListener(NextClass);
        confirmButton.onClick.AddListener(OnConfirmCreate);
        inputCharacterName.onValueChanged.AddListener(OnNameChanged);

        confirmButton.interactable = false;

        // Hiển thị class đầu tiên
        UpdateClassDisplay();
    }

    // ==========================================================
    // 🔁 Chuyển class khi nhấn nút
    // ==========================================================
    private void NextClass()
    {
        currentIndex = (currentIndex + 1) % classNames.Length;
        UpdateClassDisplay();
    }

    private void PreviousClass()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = classNames.Length - 1;
        UpdateClassDisplay();
    }

    private void UpdateClassDisplay()
    {
        if (characterDisplay != null)
            characterDisplay.SetClassIndex(currentIndex);
    }

    // ==========================================================
    // 🔤 Kiểm tra tên nhân vật
    // ==========================================================
    private void OnNameChanged(string newName)
    {
        newName = newName.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            checkNameNotice.text = "⚠️ Tên không được để trống.";
            confirmButton.interactable = false;
            isNameValid = false;
            return;
        }

        // kiểm tra trùng tên trong danh sách nhân vật hiện có
        bool nameExists = AccountDataManager.Instance.allCharacters
            .Any(c => c._characterName.Equals(newName, System.StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            checkNameNotice.text = "❌ Tên đã tồn tại.";
            confirmButton.interactable = false;
            isNameValid = false;
        }
        else
        {
            checkNameNotice.text = "✅ Tên hợp lệ.";
            confirmButton.interactable = true;
            isNameValid = true;
        }
    }

    // ==========================================================
    // 🧙‍♂️ Xác nhận tạo nhân vật
    // ==========================================================
    private void OnConfirmCreate()
    {
        if (!isNameValid)
        {
            checkNameNotice.text = "⚠️ Tên chưa hợp lệ.";
            return;
        }

        string charName = inputCharacterName.text.Trim();
        string classType = classNames[currentIndex];

        confirmButton.interactable = false;
        checkNameNotice.text = "🔄 Đang tạo nhân vật...";

        // 🔹 1) Tạo nhân vật trên PlayFab (không có CharacterType)
        PlayFabClientAPI.GrantCharacterToUser(new GrantCharacterToUserRequest
        {
            CharacterName = charName
        },
        result =>
        {
            Debug.Log($"✅ Nhân vật mới: {charName} ({classType}), ID: {result.CharacterId}");

            // 🔹 2) Lưu thông tin nhân vật vào danh sách local
            var newChar = new PlayerProgressData
            {
                _characterID = result.CharacterId,
                _characterName = charName,
                _playerID = AccountDataManager.Instance.GetTitlePlayerID(),
                _level = 1
            };

            AccountDataManager.Instance.allCharacters.Add(newChar);

            // 🔹 3) Lưu ClassType lên CharacterData (PlayFab)
            var saveData = new Dictionary<string, string>
            {
                { "ClassType", classType },
                { "Level", "1" }
            };

            PlayFabClientAPI.UpdateCharacterData(new UpdateCharacterDataRequest
            {
                CharacterId = result.CharacterId,
                Data = saveData
            },
            s => Debug.Log("💾 Đã lưu ClassType vào CharacterData."),
            e => Debug.LogWarning("⚠️ Không thể lưu ClassType: " + e.GenerateErrorReport()));

            checkNameNotice.text = "✅ Tạo nhân vật thành công!";
            confirmButton.interactable = true;

            // 🔹 4) Quay lại màn hình chọn nhân vật
            SwitchToSelectionCanvas();
        },
        error =>
        {
            Debug.LogError("❌ Tạo nhân vật thất bại: " + error.GenerateErrorReport());
            checkNameNotice.text = "❌ Lỗi khi tạo nhân vật.";
            confirmButton.interactable = true;
        });
    }

    private void SwitchToSelectionCanvas()
    {
        AccountDataManager.Instance.CreateCharacterNameCanvas.enabled = false;
        AccountDataManager.Instance.CreateCharacterCanvas.SetActive(false);
        AccountDataManager.Instance.SelectionCharacterCanvas.SetActive(true);

        Debug.Log("🎮 Đã quay lại SelectionCharacterCanvas.");
    }
}
