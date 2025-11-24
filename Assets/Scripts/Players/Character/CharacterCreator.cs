using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class CharacterCreator : MonoBehaviour
{
    public CharacterListLoader characterListLoader;

    public void CreateCharacter(string name, string className, Action onSuccess = null)
    {
        // =======================================================
        // Tạo ID chuẩn
        // =======================================================
        string guid = Guid.NewGuid().ToString("N");
        string characterId = guid;
        string playFabKey = "CHAR_" + guid;

        // =======================================================
        //  Tạo dữ liệu nhân vật ban đầu để LƯU LÊN PLAYFAB
        //    (chỉ level + name + class)
        // =======================================================
        CharacterProgressData data = new CharacterProgressData
        {
            characterId = characterId,
            characterName = name,
            characterClass = className, 
            level = 1,           
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
