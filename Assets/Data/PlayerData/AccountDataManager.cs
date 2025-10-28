using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class AccountDataManager : MonoBehaviour
{
    public static AccountDataManager Instance { get; private set; }

    [Header("🔑 Player Info")]
    [ReadOnly][SerializeField] private string TitlePlayerID;
    [ReadOnly][SerializeField] private string MasterPlayerID;
    [ReadOnly][SerializeField] private string TitleID;

    // Getter công khai nếu cần đọc ở script khác
    public string GetTitlePlayerID() => TitlePlayerID;
    public string GetMasterPlayerID() => MasterPlayerID;
    public string GetTitleID() => TitleID;


    [Header("🎮 UI References")]
    public Canvas PlayerNameCanvas;

    [Header("Create Character UI")]
    public GameObject CreateCharacterCanvas;
    public TMP_InputField inputCharacterName;
    public TMP_Text checkNameNotice;
    public GameObject CreateCharacterConfirmButton;

    [Header("Select Character UI")]
    public GameObject SelectionCharacterCanvas;
    public Transform characterSlotParent;
    public GameObject characterSlotPrefab;
    public GameObject CreateNewButton;

    [Header("📜 Static Character Classes")]
    public StaticDataCharacter StaticArcherCharacter;
    public StaticDataCharacter StaticGunnerCharacter;
    public StaticDataCharacter StaticMageCharacter;

    [Header("Runtime")]
    public List<PlayerProgressData> allCharacters = new List<PlayerProgressData>();
    public PlayerProgressData currentCharacter;

    private const int MaxCharacters = 3;

    // ==========================================================
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
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "A1B2C"; // 🔧 Thay bằng TitleID của bạn
        }

        TitleID = PlayFabSettings.staticSettings.TitleId;
        Debug.Log($"🏷️ Title ID: {TitleID}");

        LoadCharactersFromPlayFab();
    }

    // ==========================================================
    // ☁️ TẢI DANH SÁCH NHÂN VẬT
    // ==========================================================
    public void LoadCharactersFromPlayFab()
    {
        PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest(),
        result =>
        {
            allCharacters.Clear();
            if (result.Characters == null || result.Characters.Count == 0)
            {
                Debug.Log("🆕 Không có nhân vật nào. Bắt đầu tạo mới...");
                ShowCreateCharacterUI();
            }
            else
            {
                foreach (var c in result.Characters)
                {
                    var newData = new PlayerProgressData();
                    newData._characterID = c.CharacterId;
                    newData._characterName = c.CharacterName;
                    newData._playerID = TitlePlayerID;
                    allCharacters.Add(newData);
                }

                Debug.Log($"☁️ Đã tải {allCharacters.Count} nhân vật từ PlayFab.");
                ShowSelectionUI();
            }
        },
        error =>
        {
            Debug.LogError("❌ Lỗi khi tải danh sách nhân vật: " + error.GenerateErrorReport());
            ShowCreateCharacterUI();
        });
    }

    // ==========================================================
    // 🧱 UI HIỂN THỊ
    // ==========================================================
    private void ShowCreateCharacterUI()
    {
        PlayerNameCanvas.enabled = true;
        CreateCharacterCanvas.SetActive(true);
        SelectionCharacterCanvas.SetActive(false);
        inputCharacterName.text = "";
        checkNameNotice.text = "Nhập tên nhân vật mới...";
    }

    private void ShowSelectionUI()
    {
        PlayerNameCanvas.enabled = true;
        CreateCharacterCanvas.SetActive(false);
        SelectionCharacterCanvas.SetActive(true);
        RefreshCharacterSlots();
    }

    private void RefreshCharacterSlots()
    {
        foreach (Transform child in characterSlotParent)
            Destroy(child.gameObject);

        foreach (var character in allCharacters)
        {
            GameObject slot = Instantiate(characterSlotPrefab, characterSlotParent);
            var text = slot.GetComponentInChildren<TMP_Text>();
            text.text = $"{character._characterName}";
        }

        CreateNewButton.SetActive(allCharacters.Count < MaxCharacters);
    }

    // ==========================================================
    // 🧙 TẠO NHÂN VẬT MỚI
    // ==========================================================
    public void OnConfirmCreateCharacter(int classIndex)
    {
        string name = inputCharacterName.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            checkNameNotice.text = "⚠️ Tên nhân vật không được để trống.";
            return;
        }

        if (allCharacters.Any(c => c._characterName.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
        {
            checkNameNotice.text = "❌ Tên nhân vật đã tồn tại.";
            return;
        }

        if (allCharacters.Count >= MaxCharacters)
        {
            checkNameNotice.text = "⚠️ Đã đạt tối đa 3 nhân vật.";
            return;
        }

        StaticDataCharacter chosen = GetClassByIndex(classIndex);
        if (chosen == null)
        {
            checkNameNotice.text = "❌ Chưa chọn lớp nhân vật hợp lệ.";
            return;
        }

        checkNameNotice.text = "🔄 Đang tạo nhân vật...";

        PlayFabClientAPI.GrantCharacterToUser(new GrantCharacterToUserRequest
        {
            CharacterName = name
        },
        result =>
        {
            Debug.Log($"🎉 Nhân vật mới được tạo: {name} - ID: {result.CharacterId}");

            // Khởi tạo dữ liệu cho nhân vật mới
            var newChar = new PlayerProgressData();
            newChar._characterName = name;
            newChar._characterID = result.CharacterId;
            newChar._playerID = TitlePlayerID;
            newChar._level = 1;

            var stats = chosen.GetStatsAtLevel(1);
            newChar._health = stats.health;
            newChar._stamina = stats.stamina;
            newChar._attack = stats.attack;
            newChar._magic = stats.magic;
            newChar._armor = stats.armor;
            newChar._magicResist = stats.magicResist;

            // ✅ Lưu loại class và stats vào CharacterData (PlayFab)
            var saveRequest = new UpdateCharacterDataRequest
            {
                CharacterId = result.CharacterId,
                Data = new Dictionary<string, string>
                {
                    { "ClassType", chosen.characterName },
                    { "Level", newChar._level.ToString() },
                    { "Health", newChar._health.ToString() },
                    { "Attack", newChar._attack.ToString() },
                    { "Magic", newChar._magic.ToString() },
                    { "Armor", newChar._armor.ToString() },
                    { "MagicResist", newChar._magicResist.ToString() }
                }
            };

            PlayFabClientAPI.UpdateCharacterData(saveRequest,
                s => Debug.Log("✅ Dữ liệu nhân vật đã lưu vào PlayFab."),
                e => Debug.LogError("❌ Lỗi khi lưu CharacterData: " + e.GenerateErrorReport()));

            allCharacters.Add(newChar);
            newChar.SaveToPlayFab(); // lưu user data (nếu cần)
            checkNameNotice.text = "✅ Tạo nhân vật thành công!";

            ShowSelectionUI();
        },
        error =>
        {
            Debug.LogError("❌ Lỗi khi tạo nhân vật: " + error.GenerateErrorReport());
            checkNameNotice.text = "❌ Tạo nhân vật thất bại.";
        });
    }

    private StaticDataCharacter GetClassByIndex(int index)
    {
        switch (index)
        {
            case 0: return StaticArcherCharacter;
            case 1: return StaticGunnerCharacter;
            case 2: return StaticMageCharacter;
            default: return null;
        }
    }

    // ==========================================================
    // 🧍 CHỌN NHÂN VẬT
    // ==========================================================
    public void OnSelectCharacter(string characterId)
    {
        currentCharacter = allCharacters.FirstOrDefault(c => c._characterID == characterId);
        if (currentCharacter != null)
        {
            Debug.Log($"🧙 Đã chọn nhân vật: {currentCharacter._characterName}");
            // TODO: chuyển sang gameplay scene
        }
    }

    // ==========================================================
    // ➕ TẠO THÊM NHÂN VẬT TỪ UI SELECTION
    // ==========================================================
    public void OnCreateNewFromSelection()
    {
        if (allCharacters.Count < MaxCharacters)
            ShowCreateCharacterUI();
        else
            checkNameNotice.text = "⚠️ Đã đạt tối đa 3 nhân vật.";
    }
}
