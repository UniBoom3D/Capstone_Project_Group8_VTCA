using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabTest : MonoBehaviour
{
    void Start()
    {
        var request = new LoginWithCustomIDRequest { CustomId = "TestPlayer", CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful: " + result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
}
