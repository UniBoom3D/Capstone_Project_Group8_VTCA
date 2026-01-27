using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour, ITurnParticipant
{
    [Header("Stats Link")]
    public CombatStats myStats;

    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION
    // =========================================================
    public string Name => gameObject.name;
    public int HP => myStats != null ? myStats.currentHealth : 0;
    public bool IsAlive => HP > 0;
    Transform ITurnParticipant.transform
    {
        get => this.transform;
        set { }
    }

    public void TakeDamage(int dmg)
    {
        if (myStats != null) myStats.TakeDamage(dmg);
    }

    public void TakeTurn() { } // Handled by Update
    // =========================================================

    [Header("UI Settings")]
    public Slider powerSlider;

    [Header("Visuals & Audio")]
    public TrajectoryPredictor trajectory; // 👈 NEW SLOT FOR THE LINE
    public GameObject ammoMeshToHide;
    public float reloadTime = 2f;
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;

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
        if (myStats == null) myStats = GetComponent<CombatStats>();
    }

    private void Update()
    {
        // Gravity
        if (controller != null && controller.enabled)
            controller.Move(Physics.gravity * Time.deltaTime);

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

        if (v != 0 && myStats != null && myStats.HasStamina())
        {
            controller.Move(transform.forward * v * moveSpeed * Time.deltaTime);
            myStats.UseStamina(myStats.moveStaminaCost * Time.deltaTime);
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

        // 2. Charging (Draw Line)
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            chargeTimer += Time.deltaTime;
            float range = maxChargePower - minChargePower;
            float currentPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);

            if (powerSlider != null) powerSlider.value = currentPower;

            // 🟢 PREDICTION LOGIC
            // Must match the math in Projectile.Launch exactly (forward * power * 0.5)
            if (trajectory != null && firePoint != null)
            {
                Vector3 predictedVel = firePoint.forward * currentPower * 0.5f;
                trajectory.ShowTrajectory(firePoint.position, predictedVel);
            }
        }

        // 3. Fire
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            float range = maxChargePower - minChargePower;
            LastFiredPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);

            // Hide the line immediately
            if (trajectory != null) trajectory.Hide();

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

        if (enable && myStats != null) myStats.ResetTurnStats();

        if (!enable)
        {
            isCharging = false;
            if (trajectory != null) trajectory.Hide(); // Ensure line is hidden if turn ends abruptly
            if (powerSlider != null) { powerSlider.value = 0; powerSlider.gameObject.SetActive(false); }
        }

        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}