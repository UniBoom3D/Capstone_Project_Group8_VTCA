using UnityEngine;
using TMPro;

public class LoginCanvasController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel; // Panel đăng nhập
    public GameObject registerPanel; // Panel đăng ký

    [Header("UI Buttons")]
    public GameObject registerButton;
    public GameObject cancelButton; 

    [Header("UI InputFields")]
    public TMP_InputField usernameInput;  
    public TMP_InputField passwordInput;  
    public TMP_InputField confirmPasswordInput; 

    void Start()
    {
        // Mặc định, chỉ hiển thị LoginPanel
        ShowLoginPanel();
    }

    // chuyển sang Register Panel khi người chơi nhấn nút đăng ký
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false); 
        registerPanel.SetActive(true);

        
        usernameInput.Select();  // Focus vào InputField username
        usernameInput.ActivateInputField();

    }

    // Hàm chuyển sang Login Panel khi người chơi nhấn nút hủy tại RegisterPanel
    public void ShowLoginPanel()
    {
        registerPanel.SetActive(false); 
        loginPanel.SetActive(true); 
    }

    // Hàm đăng ký (nút đăng ký)
    public void OnRegisterButtonClicked()
    {
        ShowRegisterPanel();
    }

    // Hàm hủy (nút hủy tại RegisterPanel)
    public void OnCancelButtonClicked()
    {
        ShowLoginPanel();
    }
}
