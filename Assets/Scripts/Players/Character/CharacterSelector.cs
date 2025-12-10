using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance;

    // Lưu lại nhân vật đã chọn cho runtime
    public string selectedCharacterId;
    public CharacterProgressData selectedCharacterData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi từ SelectionCharacterManager – truyền luôn full data.
    /// </summary>
    public void SelectCharacter(CharacterProgressData data)
    {
        if (data == null)
        {
            Debug.LogError("SelectCharacter: data is null");
            return;
        }

        selectedCharacterId = data.characterId;
        selectedCharacterData = data;

        // Cập nhật lên PlayFab (chỉ cần gửi ID)
        UpdateSelectedCharacterOnServer(data.characterId);
    }

    private void UpdateSelectedCharacterOnServer(string characterId)
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
