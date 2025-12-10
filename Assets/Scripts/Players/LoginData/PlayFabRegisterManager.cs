using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabRegisterManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Text messageText;

    public void OnConfirmButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string confirm = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        if (password != confirm)
        {
            messageText.text = "Mật khẩu xác nhận không trùng khớp.";
            return;
        }

        Register(username, password);
    }

    private void Register(string username, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            result =>
            {
                messageText.text = "Đăng ký thành công!";

                Debug.Log($"Registered PlayFabId: {result.PlayFabId}");

                messageText.text = "Vui Lòng đăng nhập lại";
            },
            error =>
            {
                messageText.text = "Đăng ký thất bại: " + error.ErrorMessage;
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
}
