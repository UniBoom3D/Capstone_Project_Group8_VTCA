using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayFabLoginManager : MonoBehaviour
{
    [Header("UI References")]
    public LoginCanvasController canvasController;
    public TMP_Text messageText;
    public GameObject playButton;   // 👉 Nút Play (ẩn/hiện sau đăng nhập)

    public static StaticPlayerData playerData = new StaticPlayerData();

    void Start()
    {
        if (playButton != null)
            playButton.SetActive(false); // Ẩn nút Play khi bắt đầu
    }

    public void OnLoginButtonClicked()
    {
        string username = canvasController.loginUsernameInput.text;
        string password = canvasController.loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        messageText.text = "🔄 Đang đăng nhập...";

        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        messageText.text = "✅ Đăng nhập thành công!";
        Debug.Log($"🎮 Login Success! Player ID: {result.PlayFabId}");

        playerData._playerID = result.PlayFabId;
        playerData._username = canvasController.loginUsernameInput.text;
        playerData._password = canvasController.loginPasswordInput.text;
        playerData._level = 1;
        playerData._characterName = "New Player";
        playerData._characterID = System.Guid.NewGuid().ToString();

        if (canvasController.login_Button != null)
            canvasController.login_Button.SetActive(false);
        if (canvasController.logout_Button != null)
            canvasController.logout_Button.SetActive(true);

        if (playButton != null)
            playButton.SetActive(true);  // ✅ Bật nút Play sau khi đăng nhập thành công

        Debug.Log($"🧩 Player logged in: {playerData._username}");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        messageText.text = "❌ Đăng nhập thất bại: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }

    public void OnPlayButtonClicked()
    {
        messageText.text = "🚀 Đang tải sảnh chờ...";
        StartCoroutine(LoadLobbyAfterDelay(0.8f));
    }

    private IEnumerator LoadLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnLogoutButtonClicked()
    {
        messageText.text = "👋 Bạn đã đăng xuất.";

        if (canvasController.login_Button != null)
            canvasController.login_Button.SetActive(true);
        if (canvasController.logout_Button != null)
            canvasController.logout_Button.SetActive(false);
        if (playButton != null)
            playButton.SetActive(false);

        Debug.Log("🚪 Logged out and returned to login screen.");
    }

    public void OnExitGameButtonClicked()
    {
        Debug.Log("👋 Exiting game...");
        Application.Quit();
    }
}
