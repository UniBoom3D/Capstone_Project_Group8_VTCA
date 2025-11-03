using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class TabNavigation : MonoBehaviour
{
    [Header("Thứ tự ô nhập khi nhấn Tab")]
    public TMP_InputField[] inputs;

    void Update()
    {
        if (Keyboard.current == null) return;

        // Nhấn Tab
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            bool reverse = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            Jump(reverse);
        }
    }

    void Jump(bool reverse)
    {
        var currentGO = EventSystem.current.currentSelectedGameObject;
        TMP_InputField current = currentGO ? currentGO.GetComponent<TMP_InputField>() : null;

        // Nếu chưa có ô nào được chọn, chọn ô đầu tiên
        if (current == null)
        {
            if (inputs != null && inputs.Length > 0)
                StartCoroutine(SelectNextFrame(inputs[0]));
            return;
        }

        int index = System.Array.IndexOf(inputs, current);
        if (index < 0) return;

        int next = reverse
            ? (index - 1 + inputs.Length) % inputs.Length
            : (index + 1) % inputs.Length;

        StartCoroutine(SelectNextFrame(inputs[next]));
    }

    IEnumerator SelectNextFrame(TMP_InputField target)
    {
        // Clear selection để tránh kẹt focus
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // đợi 1 frame để UI cập nhật (đặc biệt khi panel vừa bật)

        EventSystem.current.SetSelectedGameObject(target.gameObject);
        target.Select();
        target.ActivateInputField();
    }
}
