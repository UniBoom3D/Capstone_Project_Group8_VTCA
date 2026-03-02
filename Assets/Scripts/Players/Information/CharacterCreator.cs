using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    public CharacterListLoader characterListLoader;

    // Thêm tham chiếu tới các StaticDataCharacter cho các lớp nhân vật
    public StaticArcherCharacter staticArcherData;
    public StaticMageCharacter staticMageData;
    public StaticGunnerCharacter staticGunnerData;

    public void CreateCharacter(string name, string className, Action onSuccess = null)
    {
        // =======================================================
        // Tạo ID chuẩn
        // =======================================================
        string guid = Guid.NewGuid().ToString("N");
        string characterId = guid;
        string playFabKey = "CHAR_" + guid;

        // =======================================================
        // Lấy StaticDataCharacter tương ứng theo className
        // =======================================================
        StaticDataCharacter selectedClassData = null;
        switch (className.ToLower())
        {
            case "archer":
                selectedClassData = staticArcherData;
                break;
            case "mage":
                selectedClassData = staticMageData;
                break;
            case "gunner":
                selectedClassData = staticGunnerData;
                break;
            default:
                Debug.LogError("Unknown class name: " + className);
                return;
        }

        // =======================================================
        // Tạo dữ liệu nhân vật ban đầu để LƯU LÊN PLAYFAB
        // =======================================================
        CharacterProgressData data = new CharacterProgressData(selectedClassData)  // Truyền vào StaticDataCharacter
        {
            characterId = characterId,
            characterName = name,
            characterClass = className,
            level = 1,  // Mới tạo nhân vật, level là 1
        };

        // =======================================================
        // Serialize JSON (đẩy lên PlayFab)
        // =======================================================
        string json = JsonUtility.ToJson(data);

        // =======================================================
        // Push lên PlayFab
        // Đồng thời set SelectedCharacter = ID được tạo
        // =======================================================
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { playFabKey, json },
                { "SelectedCharacter", characterId }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
        result =>
        {
            Debug.Log("✔ Created new character: " + json);

            // 🔄 Callback UI
            onSuccess?.Invoke();

            // 🔄 Load lại danh sách nhân vật mới
            characterListLoader.LoadCharacters();
        },
        error =>
        {
            Debug.LogError("❌ CreateCharacter FAILED: " + error.ErrorMessage);
        });
    }
}
