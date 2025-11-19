using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class CharacterListLoader : MonoBehaviour
{
    public static CharacterListLoader Instance { get; private set; }

    [Header("UI References")]
    public GameObject createCharacterCanvas;      // UI tạo nhân vật
    public GameObject createCharacterNameCanvas;  // UI nhập tên nhân vật
    public GameObject selectionCharacterCanvas;   // UI chọn nhân vật

    [Header("Runtime Data")]
    public List<CharacterProgressData> characterList = new List<CharacterProgressData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadCharacterListFromPlayFab();
    }

    // ============================================================
    // ☁️ 1. LOAD CHARACTER LIST FROM PLAYFAB
    // ============================================================
    public void LoadCharacterListFromPlayFab()
    {
        PlayFabClientAPI.GetAllUsersCharacters(
            new ListUsersCharactersRequest(),
            OnCharacterListLoaded,
            error =>
            {
                Debug.LogError("❌ Lỗi khi tải Character List: " + error.GenerateErrorReport());
                ShowCreateCharacterUI(); // fallback
            });
    }

    private void OnCharacterListLoaded(ListUsersCharactersResult result)
    {
        characterList.Clear();

        if (result.Characters == null || result.Characters.Count == 0)
        {
            Debug.Log("🆕 Không có nhân vật nào → bật UI tạo nhân vật");
            ShowCreateCharacterUI();
            return;
        }

        // Chuyển đổi PlayFabCharacter → CharacterProgressData
        foreach (var c in result.Characters)
        {
            CharacterProgressData data = new CharacterProgressData
            {
                characterID = c.CharacterId,
                characterName = c.CharacterName,
                characterClass = c.CharacterType,

                // EXP/Level/Stats sẽ load sau bằng CharacterSaveLoadManager
                level = 1,
                exp = 0
            };

            characterList.Add(data);
        }

        Debug.Log($"☁️ Đã tải {characterList.Count} nhân vật từ PlayFab.");
        ShowSelectionCharacterUI();
    }

    // ============================================================
    // 🎨 2. UI CONTROL
    // ============================================================
    private void ShowCreateCharacterUI()
    {
        createCharacterCanvas.SetActive(true);
        createCharacterNameCanvas.SetActive(true);
        selectionCharacterCanvas.SetActive(false);
    }

    private void ShowSelectionCharacterUI()
    {
        createCharacterCanvas.SetActive(false);
        createCharacterNameCanvas.SetActive(false);
        selectionCharacterCanvas.SetActive(true);
    }
}
