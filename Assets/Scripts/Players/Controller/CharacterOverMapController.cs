using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterOverMapController : MonoBehaviour
{
    [Header("Cấu hình Di chuyển (Player/Move)")]
    public float moveSpeed = 5f;
    public InputActionReference moveAction;

    [Header("Cấu hình Xoay (Player/Look)")]
    public float lookSensitivity = 0.1f;
    public Transform playerCamera;
    public InputActionReference lookAction;

    [Header("Trạng thái Chuột")]
    public bool isCursorLocked = true;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private void Start()
    {
        UpdateCursorState(isCursorLocked);
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();

        moveAction.action.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.action.canceled += ctx => moveInput = Vector2.zero;

        lookAction.action.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        lookAction.action.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isCursorLocked = !isCursorLocked;
            UpdateCursorState(isCursorLocked);
        }

        HandleMovement();

        if (isCursorLocked)
        {
            HandleLook();
        }
    }

    private void UpdateCursorState(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleMovement()
    {
        // Sử dụng transform để di chuyển đúng hướng nhân vật đang nhìn
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // --- PHẦN SỬA ĐỔI CHÍNH ---
    private void HandleLook()
    {

        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;


        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    // Thêm hàm này vào trong class CharacterOverMapController
    public void ToggleControl(bool enable)
    {
        isCursorLocked = enable;
        UpdateCursorState(enable);

        // Nếu bị vô hiệu hóa (khi đang nói chuyện với NPC)
        // thì reset input về 0 để nhân vật không tự trượt đi
        if (!enable)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }
    }
}