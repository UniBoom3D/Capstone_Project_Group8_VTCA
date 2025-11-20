using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayFabLoginManager : MonoBehaviour
{
    [Header("UI References")]
    public LoginCanvasController canvasController;
    public TMP_Text messageText;
    public GameObject playButton;

    public static PlayerAccountData playerAccount = new PlayerAccountData();

    private void Start()
    {
        if (playButton != null)
            playButton.SetActive(false);

        // 🎯 Tự động focus input field thông qua LoginCanvasController
        canvasController.ShowLoginPanel();
    }

    // ===========================================
    // 🔵 GỌI LOGIN KHI BẤM NÚT HOẶC PHÍM ENTER
    // ===========================================
    public void OnLoginButtonClicked()
    {
        Login();
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Login();
        }
    }

    // ===========================================
    // 🔐 Thực hiện Login
    // ===========================================
    private void Login()
    {
        string username = canvasController.loginUsernameInput.text;
        string password = canvasController.loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        messageText.text = "🔄 Đang đăng nhập...";

        PlayFabClientAPI.LoginWithPlayFab(
            new LoginWithPlayFabRequest { Username = username, Password = password },
            OnLoginSuccess,
            OnLoginFailure
        );
    }

    // ===========================================
    // 🔵 LOGIN SUCCESS
    // ===========================================
    private void OnLoginSuccess(LoginResult result)
    {
        messageText.text = "✅ Đăng nhập thành công!";
        Debug.Log("Login Success: " + result.PlayFabId);

        // Lưu lại account
        playerAccount.playerID = result.PlayFabId;
        playerAccount.username = canvasController.loginUsernameInput.text;
        playerAccount.password = canvasController.loginPasswordInput.text;

        // Tạo playerName tự động
        playerAccount.playerName = "Player_" + result.PlayFabId.Substring(result.PlayFabId.Length - 6);

        playButton.SetActive(true);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        messageText.text = "❌ Đăng nhập thất bại: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }

    // ===========================================
    // ▶ Load Character Scene
    // ===========================================
    public void OnPlayButtonClicked()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("CharacterScene");
    }
}
