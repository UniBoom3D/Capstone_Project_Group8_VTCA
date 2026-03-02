using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class LoginCanvasController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginMessageText;
    public GameObject loginButton;
    public GameObject playButton;
    public GameObject registerButton;

    [Header("Register Fields")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField confirmRegisterPasswordInput;

    void Start()
    {
        ShowLoginPanel();
        playButton.SetActive(false);
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        StartCoroutine(FocusNextFrame(registerUsernameInput));
    }

    public void ShowLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        StartCoroutine(FocusNextFrame(loginUsernameInput));
    }

    IEnumerator FocusNextFrame(TMP_InputField field)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(field.gameObject);
        field.ActivateInputField();
    }

    // ===========================================
    // UI gọi Login backend
    // ===========================================
    public void OnLoginButtonClicked()
    {
        PlayFabLoginManager.Instance.Login(
            loginUsernameInput.text,
            loginPasswordInput.text,
            loginMessageText,
            onSuccess: () =>
            {
                playButton.SetActive(true);
                loginButton.SetActive(false);
                registerButton.SetActive(false);
            }
        );
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            OnLoginButtonClicked();
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnLoginButtonClicked();
    }

    // ===========================================
    // ▶ Load Character Scene
    // ===========================================
    public void OnPlayButtonClicked()
    {
        PlayFabLoginManager.Instance.LoadCharacterScene();
    }

    public void OnRegisterButtonClicked() => ShowRegisterPanel();
    public void OnCancelButtonClicked() => ShowLoginPanel();
}
