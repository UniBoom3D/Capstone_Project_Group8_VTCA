using UnityEngine;
using TMPro;

public class SelectionCharacterManager : MonoBehaviour
{
    public CharacterListLoader characterListLoader;

    public Transform slotPosition;
    public TMP_Text nameText;
    public TMP_Text levelText;

    private int currentIndex = 0;
    private GameObject currentPreview;

    public void InitSelection()
    {
        var list = characterListLoader.characters;

        if (list.Count == 0)
            return;

        currentIndex = 0;
        ShowCharacter(currentIndex);
    }

    private void ShowCharacter(int index)
    {
        var data = characterListLoader.characters[index];

        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        currentPreview.transform.SetParent(slotPosition, false);
        currentPreview.transform.localPosition = Vector3.zero;

        nameText.text = data.characterName;
        levelText.text = "Level: " + data.level;
    }

    public void NextCharacter()
    {
        if (characterListLoader.characters.Count == 0) return;

        currentIndex = (currentIndex + 1) % characterListLoader.characters.Count;
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

    public void OnPlay()
    {
        var data = characterListLoader.characters[currentIndex];
        CharacterSelector.Instance.SelectCharacter(data.characterId);
    }
}
