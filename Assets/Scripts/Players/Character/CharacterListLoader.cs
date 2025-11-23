using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class CharacterListLoader : MonoBehaviour
{
    [Header("Runtime List (Loaded From PlayFab)")]
    public List<CharacterProgressData> characters = new List<CharacterProgressData>();

    [Header("UI Canvases")]
    public GameObject createCharacterCanvas;
    public GameObject createCharacterNameCanvas;

    public GameObject selectionCharacterCanvas;
    public GameObject selectionCharacterNameCanvas;

    private void Start()
    {
        LoadCharacters();
    }

    // =======================================================
    // 🔄 LOAD CHARACTERS TỪ PLAYFAB
    // =======================================================
    public void LoadCharacters()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        characters.Clear();

        foreach (var pair in result.Data)
        {
            // Chỉ lấy các key có dạng CHAR_xxx
            if (pair.Key.StartsWith("CHAR_"))
            {
                CharacterProgressData data =
                    JsonUtility.FromJson<CharacterProgressData>(pair.Value.Value);

                characters.Add(data);
            }
        }

        UpdateCanvasState();
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("❌ LoadCharacters failed: " + error.GenerateErrorReport());
    }


    // =======================================================
    // 🧭 ĐIỀU HƯỚNG UI CANVAS
    // =======================================================
    private void UpdateCanvasState()
    {
        if (characters.Count == 0)
        {
            ShowCreateCharacterCanvas();
        }
        else
        {
            ShowSelectionCharacterCanvas();
        }
    }

    public void ShowCreateCharacterCanvas()
    {
        createCharacterCanvas.SetActive(true);
        createCharacterNameCanvas.SetActive(true);
        selectionCharacterCanvas.SetActive(false);
        selectionCharacterNameCanvas.SetActive(false);
        Debug.Log("🎯 Switched to Create Character Canvas.");
    }

    public void ShowSelectionCharacterCanvas()
    {
        createCharacterCanvas.SetActive(false);
        createCharacterNameCanvas.SetActive(false);
        selectionCharacterCanvas.SetActive(true);
        selectionCharacterNameCanvas.SetActive(true);
        Debug.Log("🎯 Switched to Selection Character Canvas.");
    }


    // =======================================================
    // 🔎 CHECK NAME LOCAL
    // =======================================================
    public bool HasCharacterName(string name)
    {
        foreach (var c in characters)
        {
            if (c.characterName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
