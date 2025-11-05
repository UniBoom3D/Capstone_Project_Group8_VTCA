using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class AccountDataManager : MonoBehaviour
{
    public static AccountDataManager Instance { get; private set; }

    [Header("🔑 Player Info (Read Only)")]
    [ReadOnly][SerializeField] private string TitlePlayerID;
    [ReadOnly][SerializeField] private string MasterPlayerID;
    [ReadOnly][SerializeField] private string TitleID;

    public string GetTitlePlayerID() => TitlePlayerID;
    public string GetMasterPlayerID() => MasterPlayerID;
    public string GetTitleID() => TitleID;

    [Header("🎮 UI References")]
    public Canvas CreateCharacterNameCanvas;
    public GameObject CreateCharacterCanvas;
    public GameObject SelectionCharacterCanvas;
    

    [Header("Runtime Data")]
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
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 🔹 Lấy dữ liệu ID từ PlayFabLoginManager
        TitlePlayerID = PlayFabLoginManager.playerData._playerID;
        TitleID = PlayFabSettings.staticSettings.TitleId;

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
        result =>
        {
        // Không còn MasterAccountId trong SDK mới
        MasterPlayerID = "NotAvailable";
        Debug.Log($"🎯 TitlePlayerID: {TitlePlayerID} | MasterPlayerID: {MasterPlayerID}");
        LoadCharactersFromPlayFab();
         },
         error =>
        {
        MasterPlayerID = "Unavailable";
        Debug.LogWarning("⚠️ Không thể lấy MasterPlayerID: " + error.GenerateErrorReport());
        LoadCharactersFromPlayFab();
        });

    }

    // ==========================================================
    // ☁️ LOAD CHARACTER LIST
    // ==========================================================
    private void LoadCharactersFromPlayFab()
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
                    allCharacters.Add(new PlayerProgressData
                    {
                        _characterID = c.CharacterId,
                        _characterName = c.CharacterName,
                        _playerID = TitlePlayerID
                    });
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
    // 🧩 UI Hiển thị
    // ==========================================================
    private void ShowCreateCharacterUI()
    {
        CreateCharacterNameCanvas.enabled = true;
        CreateCharacterCanvas.SetActive(true);
        SelectionCharacterCanvas.SetActive(false);
       
    }

    private void ShowSelectionUI()
    {
        CreateCharacterNameCanvas.enabled = false;
        CreateCharacterCanvas.SetActive(false);
        SelectionCharacterCanvas.SetActive(true);
        Debug.Log("🎮 Đã vào giao diện chọn nhân vật.");
    }
}
