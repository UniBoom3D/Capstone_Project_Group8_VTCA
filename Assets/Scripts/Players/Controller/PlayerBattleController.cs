using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour
{
    [Header("Player Control Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;

    // Kept these so you don't lose Inspector references
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float maxChargePower = 100f;
    public float chargeSpeed = 40f;

    [Header("Runtime State")]
    public bool isControllable = false;      // BattleHandler toggles this
    private bool isCharging = false;
    private float currentChargePower = 0f;
    private Vector3 moveDir;
    private CharacterController controller;

    // REMOVED internal camera reference to use external script
    // private Camera playerCamera; 

    // Event callback 
    public event Action<Projectile> OnShoot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        // playerCamera = Camera.main; // Disabled internal camera
    }

    private void Update()
    {
        if (!isControllable) return;

        HandleMovement();
        HandleAiming();

        // --- SHOOTING DISABLED FOR MOVEMENT TESTING ---
        // HandleShooting();
    }

    // --- REMOVED INTERNAL CAMERA LOGIC ---
    // (Use the separate CameraFollow script below instead)
    /* private void LateUpdate()
    {
        if (playerCamera != null)
        {
            Vector3 camOffset = new Vector3(1, 2, -5);
            playerCamera.transform.position = transform.position + camOffset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 2);
        }
    } 
    */

    // ==========================
    // 🕹️ Movement (local XZ)
    // ==========================
    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Rotate Character
        if (h != 0)
        {
            transform.Rotate(Vector3.up * h * rotationSpeed * Time.deltaTime);
        }

        // Move Forward/Back
        if (v != 0)
        {
            Vector3 forwardMove = transform.forward * v;
            controller.Move(forwardMove * moveSpeed * Time.deltaTime);
        }

        // Optional: Apply gravity if using CharacterController
        controller.Move(Physics.gravity * Time.deltaTime);
    }


    // ==========================
    // Aiming (local XY)
    // ==========================
    private void HandleAiming()
    {
        // TODO: Local angle adjustments
    }

    /* // ==========================
    // Shooting (DISABLED)
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
        }

        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            FireProjectile();
            isCharging = false;
            currentChargePower = 0;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(currentChargePower, this);
            OnShoot?.Invoke(projectile); 
        }

        Debug.Log($"🚀 Fired projectile with power {currentChargePower}");
    }
    */

    // ==========================
    // 🔒 Control toggling
    // ==========================
    public void EnableControl(bool enable)
    {
        isControllable = enable;
        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}   