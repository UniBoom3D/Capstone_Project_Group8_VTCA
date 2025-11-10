using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class CharacterDataManager : MonoBehaviour
{
    public static CharacterDataManager Instance { get; private set; }

    [Header("🔗 Liên kết dữ liệu")]
    public PlayerProgressData playerProgressData = new PlayerProgressData();

    [Header("📦 Thông tin nhân vật")]
    public string CharacterID;
    public string CharacterName;
    public string ClassType;
    public int Level;
    public int Exp;
    public int Health;
    public int Stamina;
    public int Attack;
    public int Magic;
    public int Armor;
    public int MagicResist;

    [Header("🧩 Runtime References")]
    public StaticDataCharacter classTemplate; // Archer / Mage / Gunner ...

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

    #region ======== 🔽 LOAD DATA FROM PLAYFAB ========
    public void LoadCharacterFromPlayFab(string characterId, Action onLoaded = null)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogError("❌ CharacterID không hợp lệ khi tải dữ liệu.");
            return;
        }

        PlayFabClientAPI.GetCharacterData(new GetCharacterDataRequest
        {
            CharacterId = characterId
        },
        result =>
        {
            if (result.Data == null || result.Data.Count == 0)
            {
                Debug.Log("⚠️ Chưa có CharacterData, có thể là nhân vật mới tạo.");
                onLoaded?.Invoke();
                return;
            }

            CharacterID = characterId;
            CharacterName = result.Data.ContainsKey("CharacterName") ? result.Data["CharacterName"].Value : CharacterName;
            ClassType = result.Data.ContainsKey("ClassType") ? result.Data["ClassType"].Value : "Unknown";
            Level = GetInt(result.Data, "Level", 1);
            Exp = GetInt(result.Data, "Exp", 0);
            Health = GetInt(result.Data, "Health", 0);
            Stamina = GetInt(result.Data, "Stamina", 0);
            Attack = GetInt(result.Data, "Attack", 0);
            Magic = GetInt(result.Data, "Magic", 0);
            Armor = GetInt(result.Data, "Armor", 0);
            MagicResist = GetInt(result.Data, "MagicResist", 0);

            Debug.Log($"✅ Đã tải CharacterData: {CharacterName} (Class: {ClassType}, Lv.{Level})");
            onLoaded?.Invoke();
        },
        error =>
        {
            Debug.LogError("❌ Lỗi khi tải CharacterData: " + error.GenerateErrorReport());
        });
    }

    private int GetInt(Dictionary<string, UserDataRecord> data, string key, int defaultValue)
    {
        return data.ContainsKey(key) && int.TryParse(data[key].Value, out int value)
            ? value : defaultValue;
    }
    #endregion

    #region ======== 💾 SAVE DATA TO PLAYFAB ========
    public void SaveCharacterToPlayFab()
    {
        if (string.IsNullOrEmpty(CharacterID))
        {
            Debug.LogError("❌ Không thể lưu vì CharacterID trống.");
            return;
        }

        var data = new Dictionary<string, string>
        {
            { "CharacterName", CharacterName },
            { "ClassType", ClassType },
            { "Level", Level.ToString() },
            { "Exp", Exp.ToString() },
            { "Health", Health.ToString() },
            { "Stamina", Stamina.ToString() },
            { "Attack", Attack.ToString() },
            { "Magic", Magic.ToString() },
            { "Armor", Armor.ToString() },
            { "MagicResist", MagicResist.ToString() }
        };

        var request = new UpdateCharacterDataRequest
        {
            CharacterId = CharacterID,
            Data = data
        };

        PlayFabClientAPI.UpdateCharacterData(request,
            result => Debug.Log($"☁️ Đã lưu dữ liệu nhân vật: {CharacterName} (Lv.{Level})"),
            error => Debug.LogError("❌ Lỗi khi lưu CharacterData: " + error.GenerateErrorReport()));
    }
    #endregion

    #region ======== 🧮 CẬP NHẬT LEVEL & CHỈ SỐ ========
    public void AddExperience(int amount)
    {
        Exp += amount;
        int nextLevelExp = 100 + (Level * 25);

        if (Exp >= nextLevelExp)
        {
            Exp -= nextLevelExp;
            Level++;
            RecalculateStats();
            Debug.Log($"✨ {CharacterName} lên cấp {Level}!");
        }
        SaveCharacterToPlayFab();
    }

    public void RecalculateStats()
    {
        if (classTemplate == null)
        {
            Debug.LogWarning("⚠️ Chưa gán classTemplate cho CharacterDataManager.");
            return;
        }

        var stats = classTemplate.GetStatsAtLevel(Level);
        Health = stats.health;
        Stamina = stats.stamina;
        Attack = stats.attack;
        Magic = stats.magic;
        Armor = stats.armor;
        MagicResist = stats.magicResist;
    }
    #endregion
}
