using UnityEngine;
using UnityEngine.UI;
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

    // Lưu thông tin người chơi tĩnh (sau khi đăng ký)
    public static StaticPlayerData playerData = new StaticPlayerData();

    // Hàm gọi khi nhấn nút "Đăng ký"
    public void RegisterPlayer()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string confirm = confirmPasswordInput.text;

        // Kiểm tra hợp lệ
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
            return;
        }
        if (password != confirm)
        {
            messageText.text = "⚠️ Mật khẩu xác nhận không trùng khớp.";
            return;
        }

        // Gửi yêu cầu đăng ký đến PlayFab
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    // Thành công
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "🎉 Đăng ký thành công!";
        Debug.Log($"✅ Title Player ID: {result.PlayFabId}");

        // Lưu dữ liệu vào class StaticPlayerData
        playerData._playerID = result.PlayFabId;  // ID do PlayFab tạo tự động
        playerData._username = usernameInput.text;
        playerData._password = passwordInput.text;
        playerData._level = 1; // Khởi tạo mặc định
        playerData._characterName = "New Player";
        playerData._characterID = System.Guid.NewGuid().ToString(); // tự tạo ID nhân vật

        Debug.Log($"🧩 Player Created: {playerData._username} | CharID: {playerData._characterID}");
    }

    // Thất bại
    private void OnRegisterFailure(PlayFabError error)
    {
        messageText.text = "❌ Lỗi đăng ký: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
