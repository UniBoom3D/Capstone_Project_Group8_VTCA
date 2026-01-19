using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider powerSlider;

    [Header("Player Control Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;

    [Header("Weapon Settings")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Tooltip("The highest power value")]
    public float maxChargePower = 100f;

    [Tooltip("The lowest power value")]
    public float minChargePower = 0f;

    [Tooltip("How fast the bar moves back and forth")]
    public float chargeSpeed = 50f;

    // ---------------------------------------------------------
    // Visuals & Audio
    // ---------------------------------------------------------
    [Header("Visuals & Audio")]
    [Tooltip("Drag the 'Rocket_Launcher_Missile' object here so it hides when you shoot")]
    public GameObject ammoMeshToHide;

    [Tooltip("Time it takes for the rocket to reappear")]
    public float reloadTime = 2f;

    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;
    // ---------------------------------------------------------

    public float LastFiredPower { get; private set; }

    [Header("Runtime State")]
    public bool isControllable = false;
    private bool isCharging = false;
    private bool isReloading = false;
    private float chargeTimer = 0f;
    private CharacterController controller;

    public event Action<Projectile> OnShoot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isControllable) return;

        HandleMovement();
        HandleAiming();
        HandleShooting();
    }

    private void HandleMovement()
    {
        if (controller == null || !controller.enabled) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0) transform.Rotate(Vector3.up * h * rotationSpeed * Time.deltaTime);
        if (v != 0) controller.Move(transform.forward * v * moveSpeed * Time.deltaTime);
        controller.Move(Physics.gravity * Time.deltaTime);
    }

    private void HandleAiming()
    {
        float aimInput = 0f;
        if (Input.GetKey(KeyCode.I)) aimInput = -1f;
        if (Input.GetKey(KeyCode.K)) aimInput = 1f;

        if (aimInput != 0 && firePoint != null)
        {
            firePoint.Rotate(aimInput * 40f * Time.deltaTime, 0, 0);
        }
    }

    private void HandleShooting()
    {
        if (isReloading) return;

        // 1. Start Charging
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeTimer = 0f;
            if (powerSlider != null)
            {
                powerSlider.gameObject.SetActive(true);
                powerSlider.minValue = minChargePower;
                powerSlider.maxValue = maxChargePower;
            }
        }

        // 2. Charging (Ping-Pong Effect)
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            chargeTimer += Time.deltaTime;
            float range = maxChargePower - minChargePower;
            float currentPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);
            if (powerSlider != null) powerSlider.value = currentPower;
        }

        // 3. Fire
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            float range = maxChargePower - minChargePower;
            LastFiredPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);

            FireProjectile();

            isCharging = false;
            chargeTimer = 0;
            if (powerSlider != null) powerSlider.value = 0;

            StartCoroutine(ReloadRoutine());
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        if (ammoMeshToHide != null) ammoMeshToHide.SetActive(false);
        if (audioSource != null && fireClip != null) audioSource.PlayOneShot(fireClip);

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(LastFiredPower, this);
            OnShoot?.Invoke(projectile);
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        if (ammoMeshToHide != null) ammoMeshToHide.SetActive(true);
        if (audioSource != null && reloadClip != null) audioSource.PlayOneShot(reloadClip);

        isReloading = false;
    }

    // ==========================
    // 🔒 Control toggling (Fixed!)
    // ==========================
    public void EnableControl(bool enable)
    {
        isControllable = enable;

        // Reset state when losing control
        if (!enable)
        {
            isCharging = false;
            if (powerSlider != null) powerSlider.value = 0;
        }

        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}