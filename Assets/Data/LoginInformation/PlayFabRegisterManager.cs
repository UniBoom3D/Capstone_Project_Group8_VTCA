using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class PlayFabRegisterManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Text messageText;
    public TMP_Text checkUsernameText;  // Hiển thị trạng thái username
    public GameObject confirmButton;    // Nút Xác nhận

    // Static player data sau khi đăng ký thành công
    public static StaticPlayerData playerData = new StaticPlayerData();

    private string username;
    private string password;
    private string confirmPassword;
    private Coroutine checkUsernameCoroutine;

    void Start()
    {
       
        checkUsernameText.text = "";

    }

    // ✅ Gọi khi người dùng thay đổi username input
    public void OnUsernameChanged()
    {
        if (checkUsernameCoroutine != null)
            StopCoroutine(checkUsernameCoroutine);

        // Ẩn nút xác nhận trong lúc đang gõ
        confirmButton.SetActive(false);
        checkUsernameText.text = "Đang kiểm tra...";

        checkUsernameCoroutine = StartCoroutine(CheckUsernameWithDelay());
    }

    // Đợi 3s sau khi dừng nhập rồi mới check
    private IEnumerator CheckUsernameWithDelay()
    {
        yield return new WaitForSeconds(3);

        string name = usernameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            checkUsernameText.text = "";
            yield break;
        }

        // Thử tạo tài khoản giả chỉ để kiểm tra username
        var request = new RegisterPlayFabUserRequest
        {
            Username = name,
            Password = "TempPass123!",   // Tạo password tạm vì PlayFab yêu cầu
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnCheckAvailable, OnCheckUnavailable);
    }

    // ✅ Khi username chưa tồn tại
    private void OnCheckAvailable(RegisterPlayFabUserResult result)
    {
        checkUsernameText.text = "✅ Tên người dùng khả dụng!";
        confirmButton.SetActive(true); // Bật nút Xác nhận

        // Xóa tài khoản tạm nếu cần (ở bản thật có thể disable phần tạo)
        Debug.Log("Temporary test user created: " + result.PlayFabId);
    }

    // ❌ Khi username đã tồn tại hoặc lỗi khác
    private void OnCheckUnavailable(PlayFabError error)
    {
        if (error.ErrorMessage.Contains("Username already exists"))
        {
            checkUsernameText.text = "❌ Người dùng đã tồn tại!";
        }
        else
        {
            checkUsernameText.text = "⚠️ Lỗi khi kiểm tra: " + error.ErrorMessage;
        }

        confirmButton.SetActive(false);
    }

    // ✅ Khi nhấn nút Xác nhận
    public void OnConfirmButtonClicked()
    {
        username = usernameInput.text;
        password = passwordInput.text;
        confirmPassword = confirmPasswordInput.text;

        // Kiểm tra nhập hợp lệ
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
            return;
        }
        if (password != confirmPassword)
        {
            messageText.text = "⚠️ Mật khẩu xác nhận không trùng khớp.";
            return;
        }

        // Gửi đăng ký chính thức
        RegisterPlayer(username, password);
    }

    private void RegisterPlayer(string username, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "🎉 Đăng ký thành công!";
        Debug.Log($"✅ PlayerID: {result.PlayFabId}");

        // Lưu dữ liệu
        playerData._playerID = result.PlayFabId;
        playerData._username = username;
        playerData._password = password;
        playerData._level = 1;
        playerData._characterName = "New Player";
        playerData._characterID = System.Guid.NewGuid().ToString();
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        messageText.text = "❌ Lỗi đăng ký: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
