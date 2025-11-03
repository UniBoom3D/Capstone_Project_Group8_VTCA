using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class AccountDataManager : MonoBehaviour
{
    public static AccountDataManager Instance { get; private set; }

    [Header("Dữ liệu người chơi hiện tại")]
    public PlayerProgressData playerProgressData = new PlayerProgressData();

    [Header("Asset nhân vật")]
    public StaticDataCharacter StaticArcherCharacter;
    public StaticDataCharacter StaticGunnerCharacter;
    public StaticDataCharacter StaticMageCharacter; // có thể thêm nữa sau

    [Header("Canvas & Panel UI")]
    [Space(5)]
    public Canvas PlayerNameCanvas;                   // Canvas overlay (UI)
    public GameObject CreateCharacterNamePanel;       // Nhập tên khi tạo mới
    public GameObject ShowCharacterNamePanel;         // Hiển thị tên nhân vật khi có sẵn
    public GameObject CreateCharacterCanvas;          // Chọn class khi tạo mới (World space)
    public GameObject SelectionCharacterCanvas;       // Chọn lại nhân vật có sẵn (World space)

    [Header("UI Input & Text")]
    public TMP_InputField inputCharacterName;         // Ô nhập tên nhân vật
    public TMP_Text checkNameNotice;                  // Thông báo hợp lệ
    public TMP_Text showCharacterNameText;            // Hiển thị tên nhân vật đang có

    [Header("Runtime")]
    [HideInInspector] public StaticDataCharacter CurrentCharacterDataRuntime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeLobbyState();
    }

    /// <summary>
    /// Gọi sau khi đăng nhập thành công từ PlayFabLoginManager
    /// </summary>
    public void InitializePlayerFromLogin(StaticPlayerData staticData)
    {
        playerProgressData._playerID = staticData._playerID;
        playerProgressData._username = staticData._username;
        playerProgressData._password = staticData._password;
        playerProgressData._level = staticData._level;
        playerProgressData._characterID = staticData._characterID;
        playerProgressData._characterName = staticData._characterName;

        //TODO: Sau này lấy thêm thông tin MasterID từ PlayFab Entity API
    }

    /// <summary>
    /// Kiểm tra xem người chơi đã có nhân vật hay chưa và hiển thị giao diện tương ứng
    /// </summary>
    private void InitializeLobbyState()
    {
        bool hasCharacter = !string.IsNullOrEmpty(playerProgressData._characterID);

        if (hasCharacter)
        {
            // Đã có nhân vật -> hiển thị lựa chọn nhân vật
            CreateCharacterCanvas.SetActive(false);
            SelectionCharacterCanvas.SetActive(true);

            PlayerNameCanvas.enabled = true;
            CreateCharacterNamePanel.SetActive(false);
            ShowCharacterNamePanel.SetActive(true);

            showCharacterNameText.text = playerProgressData._characterName;
            Debug.Log($"🧍 Nhân vật hiện tại: {playerProgressData._characterName}");
        }
        else
        {
            // Chưa có nhân vật -> bật giao diện tạo mới
            CreateCharacterCanvas.SetActive(true);
            SelectionCharacterCanvas.SetActive(false);

            PlayerNameCanvas.enabled = true;
            CreateCharacterNamePanel.SetActive(true);
            ShowCharacterNamePanel.SetActive(false);

            inputCharacterName.text = "";
            checkNameNotice.text = "Nhập tên nhân vật mới";
            Debug.Log("🎨 Bắt đầu tạo nhân vật mới...");
        }
    }

    /// <summary>
    /// Gọi khi nhấn nút xác nhận tạo nhân vật mới (ví dụ: 0 = Archer, 1 = Gunner, 2 = Mage)
    /// </summary>
    public void ConfirmCreateCharacter(int classIndex)
    {
        string name = inputCharacterName.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            checkNameNotice.text = "⚠️ Tên không được để trống!";
            return;
        }

        StaticDataCharacter chosenClass = GetClassByIndex(classIndex);
        if (chosenClass == null)
        {
            checkNameNotice.text = "❌ Lớp nhân vật không hợp lệ!";
            return;
        }

        // Tạo nhân vật
        SelectCharacterAtLobby(chosenClass, name);

        // Cập nhật UI
        CreateCharacterCanvas.SetActive(false);
        SelectionCharacterCanvas.SetActive(true);
        CreateCharacterNamePanel.SetActive(false);
        ShowCharacterNamePanel.SetActive(true);
        showCharacterNameText.text = name;

        Debug.Log($"🎉 Tạo nhân vật mới: {name} ({chosenClass.characterName})");

        //TODO: Sau này lưu dữ liệu này vào PlayFab CharacterData
    }

    /// <summary>
    /// Trả về class nhân vật tương ứng với index
    /// </summary>
    private StaticDataCharacter GetClassByIndex(int classIndex)
    {
        switch (classIndex)
        {
            case 0: return StaticArcherCharacter;
            case 1: return StaticGunnerCharacter;
            case 2: return StaticMageCharacter;
            default: return null;
        }
    }

    /// <summary>
    /// Gán dữ liệu nhân vật được chọn cho người chơi hiện tại
    /// </summary>
    public void SelectCharacterAtLobby(StaticDataCharacter chosenClass, string customName)
    {
        if (chosenClass == null)
        {
            Debug.LogError("❌ Không có asset nhân vật được chọn!");
            return;
        }

        // Clone scriptable object
        CurrentCharacterDataRuntime = ScriptableObject.Instantiate(chosenClass);

        playerProgressData.SetCharacterSelection(Guid.NewGuid().ToString(),
            string.IsNullOrWhiteSpace(customName) ? chosenClass.characterName : customName);

        var stats = chosenClass.GetStatsAtLevel(playerProgressData._currentLevel);

        playerProgressData._health = stats.health;
        playerProgressData._stamina = stats.stamina;
        playerProgressData._attack = stats.attack;
        playerProgressData._magic = stats.magic;
        playerProgressData._armor = stats.armor;
        playerProgressData._magicResist = stats.magicResist;

        Debug.Log($"✅ Đã chọn lớp {chosenClass.characterName}, Level {playerProgressData._currentLevel} -> HP {stats.health}, ATK {stats.attack}");
    }

    /// <summary>
    /// Gọi lại khi lên cấp hoặc reset chỉ số
    /// </summary>
    public void RecalculateStatsByClass()
    {
        if (CurrentCharacterDataRuntime == null)
        {
            Debug.LogWarning("⚠️ Chưa chọn lớp để tính lại chỉ số.");
            return;
        }

        var stats = CurrentCharacterDataRuntime.GetStatsAtLevel(playerProgressData._currentLevel);
        playerProgressData._health = stats.health;
        playerProgressData._stamina = stats.stamina;
        playerProgressData._attack = stats.attack;
        playerProgressData._magic = stats.magic;
        playerProgressData._armor = stats.armor;
        playerProgressData._magicResist = stats.magicResist;

        //TODO: Sau này đồng bộ lại vào server hoặc PlayFab Character Data
    }
}
