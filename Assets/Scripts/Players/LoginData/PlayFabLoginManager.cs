using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayFabLoginManager : MonoBehaviour
{
    public static PlayFabLoginManager Instance;
    public static PlayerAccountData playerAccount = new PlayerAccountData();

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

    // ===========================================
    // 🔵 BACKEND LOGIN – UI sẽ truyền username/password/messageText
    // ===========================================
    public void Login(string username, string password, TMP_Text messageText, System.Action onSuccess = null)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Vui lòng nhập đầy đủ thông tin !";
            return;
        }

        messageText.text = " Đang đăng nhập...";

        PlayFabClientAPI.LoginWithPlayFab(
            new LoginWithPlayFabRequest { Username = username, Password = password },
            result =>
            {
                messageText.text = "Đăng nhập thành công!";
                Debug.Log("Login Success: " + result.PlayFabId);

                // Lưu account
                playerAccount.playerID = result.PlayFabId;
                playerAccount.username = username;
                playerAccount.password = password;
                playerAccount.playerName = "Player_" + result.PlayFabId.Substring(result.PlayFabId.Length - 6);

                onSuccess?.Invoke();
            },
            error =>
            {
                messageText.text = "Đăng nhập thất bại: " + error.ErrorMessage;
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    // ===========================================
    // ▶ Load Character Scene
    // ===========================================
    public void LoadCharacterScene()
    {
        SceneManager.LoadScene("CharacterScene");
    }
}
