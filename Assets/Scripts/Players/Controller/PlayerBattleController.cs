using System;
using UnityEngine;
using UnityEngine.UI; // <--- 1. Added this for the UI Slider

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider powerSlider; // <--- 2. Drag your Red Slider here in Inspector!

    [Header("Player Control Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;

    [Header("Weapon Settings")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float maxChargePower = 100f;
    public float chargeSpeed = 40f;

    [Header("Runtime State")]
    public bool isControllable = false;
    private bool isCharging = false;
    private float currentChargePower = 0f;
    private Vector3 moveDir;
    private CharacterController controller;

    // Event callback 
    public event Action<Projectile> OnShoot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isControllable) return;

        HandleMovement();
        HandleAiming();
        HandleShooting(); // <--- 3. Re-enabled Shooting
    }

    // ==========================
    // 🕹️ Movement (local XZ)
    // ==========================
    private void HandleMovement()
    {
        // Safety check to prevent crashes if Component is disabled
        if (controller == null || !controller.enabled) return;

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

        // Apply Gravity
        controller.Move(Physics.gravity * Time.deltaTime);
    }


    // ==========================
    // Aiming (local XY)
    // ==========================
    private void HandleAiming()
    {
        // TODO: Local angle adjustments if needed
    }

    // ==========================
    // 💥 Shooting (With UI Slider)
    // ==========================
    private void HandleShooting()
    {
        // 1. Start Charging (Press Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentChargePower = 0;
            Debug.Log("Start charging shot!");

            // Initialize Slider
            if (powerSlider != null)
            {
                powerSlider.gameObject.SetActive(true); // Show it
                powerSlider.maxValue = maxChargePower;
                powerSlider.value = 0;
            }
        }

        // 2. Charging Loop (Hold Space)
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            currentChargePower += chargeSpeed * Time.deltaTime;
            currentChargePower = Mathf.Clamp(currentChargePower, 0, maxChargePower);

            // Update visual slider
            if (powerSlider != null)
            {
                powerSlider.value = currentChargePower;
            }
        }

        // 3. Fire (Release Space)
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            FireProjectile();
            isCharging = false;
            currentChargePower = 0;

            // Reset Slider
            if (powerSlider != null)
            {
                powerSlider.value = 0;
                // Optional: powerSlider.gameObject.SetActive(false); // Hide after shooting?
            }
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(currentChargePower, this); // Use "this" because we are inside PlayerBattleController
            OnShoot?.Invoke(projectile);
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