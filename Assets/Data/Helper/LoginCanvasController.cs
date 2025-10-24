using UnityEngine;

public class LoginCanvasController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel; // Panel đăng nhập
    public GameObject registerPanel; // Panel đăng ký

    [Header("UI Buttons")]
    public GameObject registerButton; // Nút đăng ký
    public GameObject cancelButton; // Nút hủy (quay lại đăng nhập)

    void Start()
    {
        
        ShowLoginPanel();
    }

    // Hàm chuyển sang Register Panel khi người chơi nhấn nút đăng ký
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);  
        registerPanel.SetActive(true); 
    }

    // Hàm chuyển sang Login Panel khi người chơi nhấn nút hủy tại RegisterPanel
    public void ShowLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);    
    }

    // Hàm đăng ký (nút đăng ký tại LoginPanel)
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
