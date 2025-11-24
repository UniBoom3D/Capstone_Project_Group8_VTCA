using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance;
    private void Awake()
    {
       if(Instance == null)
       {
            Instance = this;
            DontDestroyOnLoad(gameObject);
       }
       else if(Instance != this)
       {          
            Destroy(gameObject);
       }
    }

    public void SelectCharacter(string characterId)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "SelectedCharacter", characterId }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
        result =>
        {
            Debug.Log("✔ Selected character: " + characterId);
            SceneManager.LoadScene("Battle-test");
        },
        error =>
        {
            Debug.LogError("❌ Failed to select: " + error.ErrorMessage);
        });
    }
}
