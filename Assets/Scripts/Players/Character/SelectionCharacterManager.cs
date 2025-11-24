using UnityEngine;
using TMPro;

public class SelectionCharacterManager : MonoBehaviour
{
    [Header("Loader Reference")]
    public CharacterListLoader characterListLoader;

    [Header("Slot Preview")]
    public Transform slotPosition;

    [Header("UI Text")]
    public TMP_Text nameText;
    public TMP_Text levelText;

    private int currentIndex = 0;
    private GameObject currentPreview;


    public void InitSelection()
    {
        var list = characterListLoader.characters;

        if (list.Count == 0)
        {
            Debug.LogWarning("No characters found -> Should show CreateCharacterCanvas");
            return;
        }

        currentIndex = 0;
        ShowCharacter(currentIndex);
    }

    private void ShowCharacter(int index)
    {
        var data = characterListLoader.characters[index];

        // Xóa preview cũ
        if (currentPreview != null)
            Destroy(currentPreview);

        // Spawn capsule đại diện
        currentPreview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        currentPreview.transform.SetParent(slotPosition, false);
        currentPreview.transform.localPosition = Vector3.zero;
        currentPreview.transform.localScale = Vector3.one * 1.2f;

        // Cập nhật UI
        nameText.text = data.characterName;
        levelText.text = "Level: " + data.level;
    }

    // ============================
    // NEXT / PREVIOUS
    // ============================
    public void NextCharacter()
    {
        if (characterListLoader.characters.Count == 0) return;

        currentIndex++;
        if (currentIndex >= characterListLoader.characters.Count)
            currentIndex = 0;

        ShowCharacter(currentIndex);
    }

    public void PreviousCharacter()
    {
        if (characterListLoader.characters.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = characterListLoader.characters.Count - 1;

        ShowCharacter(currentIndex);
    }

    // ============================
    // PLAY = CONFIRM
    // ============================
    public void OnPlay()
    {
        var data = characterListLoader.characters[currentIndex];
        CharacterSelector.Instance.SelectCharacter(data.characterId);
    }
}
