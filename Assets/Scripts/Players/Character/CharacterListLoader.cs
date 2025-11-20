using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class CharacterListLoader : MonoBehaviour
{
    public static CharacterListLoader Instance;

    [Header("Runtime List (Loaded From PlayFab)")]
    public List<CharacterProgressData> characters = new List<CharacterProgressData>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadCharacters();
    }

    // ============================================================
    // 🔵 LOAD CHARACTERS từ PlayFab UserData
    // ============================================================
    public void LoadCharacters()
    {
        Debug.Log("📥 [CharacterListLoader] Loading characters...");

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                characters.Clear();

                if (result.Data == null)
                {
                    Debug.Log("Không có UserData.");
                    return;
                }

                foreach (var entry in result.Data)
                {
                    if (!entry.Key.StartsWith("CHAR_"))
                        continue; // Không phải character → bỏ qua

                    CharacterProgressData data =
                        JsonUtility.FromJson<CharacterProgressData>(entry.Value.Value);

                    if (data != null)
                        characters.Add(data);
                }

                Debug.Log($"✅ Loaded {characters.Count} characters.");
            },
            error =>
            {
                Debug.LogError("LoadCharacters FAILED: " + error.GenerateErrorReport());
            });
    }

    // ============================================================
    // 🔄 Reload (sau khi tạo/xóa nhân vật)
    // ============================================================
    public void ReloadCharacters()
    {
        LoadCharacters();
    }

    // ============================================================
    // 🔍 Check name trùng trong danh sách hiện có
    // ============================================================
    public bool HasCharacterName(string name)
    {
        foreach (var c in characters)
        {
            if (c.characterName.ToLower() == name.ToLower())
                return true;
        }
        return false;
    }

    // ============================================================
    // 🔍 Lấy character theo ID (option)
    // ============================================================
    public CharacterProgressData GetCharacterByID(string id)
    {
        return characters.Find(c => c.characterID == id);
    }
}
