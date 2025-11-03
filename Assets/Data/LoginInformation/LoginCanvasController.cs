using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class LoginCanvasController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Setup Button Login Panel UI Buttons")]
    public GameObject login_Button;
    public GameObject logout_Button;
    public GameObject ExitGameButton;

    [Header("Setup Button Register Panel UI Buttons")]
    public GameObject registerButton;
    public GameObject cancelButton;

    [Header("Register UI InputFields")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField confirmRegisterPasswordInput;

    [Header("Login UI InputFields")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;

    void Start()
    {
        ShowLoginPanel();
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
        yield return null; // đợi 1 frame

        EventSystem.current.SetSelectedGameObject(field.gameObject);
        field.Select();
        field.ActivateInputField();
    }

    public void OnRegisterButtonClicked() => ShowRegisterPanel();
    public void OnCancelButtonClicked() => ShowLoginPanel();
}
