using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Control Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;
    public Transform firePoint;              // Nơi bắn đạn ra (gắn ở nòng súng)
    public GameObject projectilePrefab;      // Prefab đạn (có script Projectile)
    public float maxChargePower = 100f;      // Lực bắn tối đa
    public float chargeSpeed = 40f;          // Tốc độ nạp lực khi giữ Space

    [Header("Runtime State")]
    public bool isControllable = false;      // BattleHandler sẽ bật/tắt điều khiển
    private bool isCharging = false;
    private float currentChargePower = 0f;
    private Vector3 moveDir;
    private CharacterController controller;
    private Camera playerCamera;

    // Event callback khi bắn xong
    public event Action<Projectile> OnShoot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!isControllable) return;

        HandleMovement();
        HandleAiming();
        HandleShooting();
    }

    private void LateUpdate()
    {
        // Giữ camera theo sau player
        if (playerCamera != null)
        {
            Vector3 camOffset = new Vector3(1, 2, -5);
            playerCamera.transform.position = transform.position + camOffset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 2);
        }
    }

    // ==========================
    // 🕹️ Movement (local XZ)
    // ==========================
    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveDir = transform.right * h + transform.forward * v;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);
    }


    // ==========================
    // Aiming (local XY)
    // ==========================
    private void HandleAiming()
    {
        // TODO: Điều chỉnh góc local x - ngang, y - dọc
    }

    

    // ==========================
    // Shooting (hold + release Space)
    // ==========================
    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentChargePower = 0;
            Debug.Log("Start charging shot!");
        }

        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            currentChargePower += chargeSpeed * Time.deltaTime;
            currentChargePower = Mathf.Clamp(currentChargePower, 0, maxChargePower);
            // Bạn có thể cập nhật UI thanh lực tại đây
        }

        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            FireProjectile();
            isCharging = false;
            currentChargePower = 0;
        }
    }

    // ==========================
    // Fire Projectile
    // ==========================
    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(currentChargePower, this);
            OnShoot?.Invoke(projectile); // Gửi event cho BattleHandler
        }

        Debug.Log($"🚀 Fired projectile with power {currentChargePower}");
    }

    // ==========================
    // 🔒 Control toggling
    // ==========================
    public void EnableControl(bool enable)
    {
        isControllable = enable;
        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}
