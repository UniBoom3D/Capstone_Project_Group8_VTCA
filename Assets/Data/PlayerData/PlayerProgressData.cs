using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerProgressData : StaticPlayerData
{
    [Header("Progress Data")]
    public int _currentLevel = 1;
    public int _currentExp = 0;
    public int _maxExp = 100;

    [Header("Base Stats")]
    public int _health;
    public int _stamina;
    public int _attack;
    public int _magic;
    public int _armor;
    public int _magicResist;

    // ===========================
    // 🧩 METHODS
    // ===========================

    #region Character & Stats
    public void SetCharacterSelection(string characterId, string characterName)
    {
        _characterID = characterId;
        _characterName = characterName;
    }

    public BasicStats GetBaseStats()
    {
        return new BasicStats
        {
            health = _health,
            stamina = _stamina,
            attack = _attack,
            magic = _magic,
            armor = _armor,
            magicResist = _magicResist
        };
    }

    [System.Serializable]
    public struct BasicStats
    {
        public int health;
        public int stamina;
        public int attack;
        public int magic;
        public int armor;
        public int magicResist;
    }
    #endregion


    // ===========================
    // 💾 SAVE / LOAD TO PLAYFAB
    // ===========================

    #region SAVE TO PLAYFAB
    public void SaveToPlayFab()
    {
        var data = new Dictionary<string, string>
        {
            { "CharacterName", _characterName },
            { "CharacterID", _characterID },
            { "Level", _level.ToString() },
            { "CurrentLevel", _currentLevel.ToString() },
            { "CurrentExp", _currentExp.ToString() },
            { "MaxExp", _maxExp.ToString() },
            { "Health", _health.ToString() },
            { "Stamina", _stamina.ToString() },
            { "Attack", _attack.ToString() },
            { "Magic", _magic.ToString() },
            { "Armor", _armor.ToString() },
            { "MagicResist", _magicResist.ToString() }
        };

        var request = new UpdateUserDataRequest
        {
            Data = data
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("✅ Dữ liệu người chơi đã được lưu lên PlayFab."),
            error => Debug.LogError("❌ Lỗi khi lưu dữ liệu lên PlayFab: " + error.GenerateErrorReport())
        );
    }
    #endregion


    #region LOAD FROM PLAYFAB
    public void LoadFromPlayFab(Action onLoaded = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            var data = result.Data;
            if (data == null || data.Count == 0)
            {
                Debug.Log("⚠️ Không có dữ liệu người chơi trên cloud, tạo mới...");
                onLoaded?.Invoke();
                return;
            }

            // Gán dữ liệu
            _characterName = data.ContainsKey("CharacterName") ? data["CharacterName"].Value : _characterName;
            _characterID = data.ContainsKey("CharacterID") ? data["CharacterID"].Value : _characterID;
            _level = GetInt(data, "Level", 1);
            _currentLevel = GetInt(data, "CurrentLevel", 1);
            _currentExp = GetInt(data, "CurrentExp", 0);
            _maxExp = GetInt(data, "MaxExp", 100);
            _health = GetInt(data, "Health", 0);
            _stamina = GetInt(data, "Stamina", 0);
            _attack = GetInt(data, "Attack", 0);
            _magic = GetInt(data, "Magic", 0);
            _armor = GetInt(data, "Armor", 0);
            _magicResist = GetInt(data, "MagicResist", 0);

            Debug.Log($"☁️ Đã tải dữ liệu người chơi từ PlayFab: {_characterName}, Level {_level}");
            onLoaded?.Invoke();

        }, error =>
        {
            Debug.LogError("❌ Lỗi khi tải dữ liệu người chơi: " + error.GenerateErrorReport());
        });
    }

    private int GetInt(Dictionary<string, UserDataRecord> data, string key, int defaultValue)
    {
        return data.ContainsKey(key) && int.TryParse(data[key].Value, out int value)
            ? value : defaultValue;
    }
    #endregion
}
