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
        string id = System.Guid.NewGuid().ToString();

        CharacterProgressData data = new CharacterProgressData
        {
            characterID = id,
            characterName = name,
            characterClass = className,
            level = 1,
            exp = 0
        };

        string json = JsonUtility.ToJson(data);

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { "CHAR_" + id, json }
                }
            },
            result =>
            {
                Debug.Log("Created new character: " + json);
                //characterListLoader.ReloadCharacters();
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("CreateCharacter FAILED: " + error.ErrorMessage);
            }
        );
    }
}
