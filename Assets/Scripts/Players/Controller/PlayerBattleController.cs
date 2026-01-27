using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour, ITurnParticipant
{
    [Header("Stats Link")]
    [Tooltip("Drag the CombatStats component here")]
    public CombatStats myStats;

    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION (Required for Battle)
    // =========================================================
    public string Name => gameObject.name;

    // Read HP directly from CombatStats script
    public int HP => myStats != null ? myStats.currentHealth : 0;

    // Check if alive based on that HP
    public bool IsAlive => HP > 0;

    // Explicit Transform Implementation (Satisfies the interface requirement)
    Transform ITurnParticipant.transform
    {
        get => this.transform;
        set { /* Unity Transform is read-only */ }
    }

    public void TakeDamage(int dmg)
    {
        // Pass the damage to the stats script
        if (myStats != null)
        {
            myStats.TakeDamage(dmg);
        }
    }

    public void TakeTurn()
    {
        // Player turn is handled by Input in Update(), so this stays empty.
    }
    // =========================================================

    [Header("UI Settings")]
    public Slider powerSlider;

    [Header("Movement Settings")]
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

    [Header("Visuals & Audio")]
    public GameObject ammoMeshToHide;
    public float reloadTime = 2f;
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;

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
        if (powerSlider != null) powerSlider.gameObject.SetActive(false);

        // Auto-find stats if forgotten
        if (myStats == null) myStats = GetComponent<CombatStats>();
    }

    private void Update()
    {
        // 1. GRAVITY (Always apply so you don't float)
        if (controller != null && controller.enabled)
        {
            controller.Move(Physics.gravity * Time.deltaTime);
        }

        // 2. INPUT (Only if it's my turn)
        if (!isControllable) return;

        HandleMovement();
        HandleAiming();
        HandleShooting();
    }

    private void HandleMovement()
    {
        if (controller == null || !controller.enabled) return;

        float h = Input.GetAxis("Horizontal"); // Rotate
        float v = Input.GetAxis("Vertical");   // Move Forward/Back

        // ROTATION (Always Allowed - Free)
        if (h != 0)
        {
            transform.Rotate(Vector3.up * h * rotationSpeed * Time.deltaTime);
        }

        // MOVEMENT (Costs Stamina)
        if (v != 0)
        {
            // Check if we have enough stamina
            if (myStats != null && myStats.HasStamina())
            {
                controller.Move(transform.forward * v * moveSpeed * Time.deltaTime);

                // Drain stamina over time
                myStats.UseStamina(myStats.moveStaminaCost * Time.deltaTime);
            }
            // If no stamina, we simply don't call controller.Move(), effectively locking the player.
        }
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

        // 2. Charging Loop (Ping Pong)
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

            Debug.Log($"🚀 Power Locked at: {LastFiredPower}");

            FireProjectile();

            isCharging = false;
            chargeTimer = 0;

            if (powerSlider != null)
            {
                powerSlider.value = 0;
                powerSlider.gameObject.SetActive(false);
            }

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
        else
        {
            Debug.LogError("❌ Projectile prefab is missing the 'Projectile' script!");
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

    public void EnableControl(bool enable)
    {
        isControllable = enable;

        // Reset Stats when turn begins
        if (enable && myStats != null)
        {
            myStats.ResetTurnStats();
        }

        if (!enable)
        {
            isCharging = false;
            if (powerSlider != null) { powerSlider.value = 0; powerSlider.gameObject.SetActive(false); }
        }

        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}