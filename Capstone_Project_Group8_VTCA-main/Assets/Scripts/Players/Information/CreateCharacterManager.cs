using UnityEngine;
using TMPro;

public class CreateCharacterManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text warningText;
    public GameObject confirmButton;

    [Header("References")]
    public CreateCharacterDisplay display;
    public CharacterCreator creator;

    public void OnConfirmCreate()
    {
        string name = nameInput.text.Trim();
        string className = display.GetClassName();

        if (string.IsNullOrEmpty(name))
        {
            warningText.text = "⚠ Nhập tên nhân vật";
            return;
        }

        creator.CreateCharacter(name, className, () =>
        {
            warningText.text = "🎉 Tạo nhân vật thành công!";
        });
    }
}
