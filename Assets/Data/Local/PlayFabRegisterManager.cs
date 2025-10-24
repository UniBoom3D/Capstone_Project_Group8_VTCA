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
    public TMP_Text checkUsernameText; // Text to show username availability

    // Static player data after successful registration
    public static StaticPlayerData playerData = new StaticPlayerData();

    private string username;
    private string password;
    private string confirmPassword;

    private Coroutine checkUsernameCoroutine;

    // Handle Register Button click
    public void RegisterPlayer()
    {
        username = usernameInput.text;
        password = passwordInput.text;
        confirmPassword = confirmPasswordInput.text;

        // Validate the inputs
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "⚠️ Please enter all the information.";
            return;
        }
        if (password != confirmPassword)
        {
            messageText.text = "⚠️ Passwords do not match.";
            return;
        }

        // Check if the username is available by attempting to register
        CheckUsernameAvailability(username);
    }

    // Attempt to register a player and check if the username is valid
    private void CheckUsernameAvailability(string username)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        // Try to register the user. If username exists, it will trigger OnRegisterFailure
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    // Success handler for RegisterPlayFabUser
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "🎉 Đăng ký thành công!";
        Debug.Log($"✅ Title Player ID: {result.PlayFabId}");

        // Save player data
        playerData._playerID = result.PlayFabId; // Generated ID
        playerData._username = username;
        playerData._password = password;
        playerData._level = 1; // Default level
        playerData._characterName = "New Player";
        playerData._characterID = System.Guid.NewGuid().ToString(); // Generate character ID

        Debug.Log($"🧩 Player Created: {playerData._username} | CharID: {playerData._characterID}");
    }

    // Failure handler for RegisterPlayFabUser
    private void OnRegisterFailure(PlayFabError error)
    {
        if (error.ErrorMessage.Contains("Username already exists"))
        {
            checkUsernameText.text = "❌ Người dùng đã tồn tại!";
        }
        else
        {
            checkUsernameText.text = "❌ Lỗi đăng ký: " + error.ErrorMessage;
        }

        messageText.text = "❌ Đăng ký thất bại: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }

    // Start checking username availability with delay after user stops typing for 3 seconds
    public void OnUsernameChanged()
    {
        // Stop any previous ongoing checks
        if (checkUsernameCoroutine != null)
        {
            StopCoroutine(checkUsernameCoroutine);
        }

        // Start a new coroutine to check availability after 3 seconds
        checkUsernameCoroutine = StartCoroutine(CheckUsernameWithDelay());
    }

    // Coroutine to wait for 3 seconds before checking username
    private IEnumerator CheckUsernameWithDelay()
    {
        checkUsernameText.text = ""; // Clear previous text
        yield return new WaitForSeconds(3); // Wait for 3 seconds

        // After waiting, check the username availability
        CheckUsernameAvailability(usernameInput.text);
    }
}
