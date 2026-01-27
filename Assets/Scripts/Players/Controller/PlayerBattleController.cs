using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerBattleController : MonoBehaviour, ITurnParticipant
{
    // =========================================================
    // 🟢 INTERFACE IMPLEMENTATION (Matches your ITurnParticipant)
    // =========================================================
    public string Name => gameObject.name;

    // 1. HP Property
    public int HP { get; private set; } = 100;

    // 2. IsAlive check
    public bool IsAlive => HP > 0;

    // 3. TakeDamage Function
    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        Debug.Log($"💥 {Name} took {dmg} damage! HP: {HP}");
    }

    // 4. Transform Fix: Explicitly implement the interface to handle the 'set' requirement
    // This tells C#: "When treating this as an ITurnParticipant, use this logic for transform"
    Transform ITurnParticipant.transform
    {
        get => this.transform;
        set { /* Unity Transforms cannot be swapped, so we leave this empty */ }
    }

    public void TakeTurn()
    {
        // Player turn is handled via Input in Update(), so this stays empty
    }
    // =========================================================

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
    }

    private void Update()
    {
        // Always apply gravity so you don't float when turn ends
        if (controller != null && controller.enabled)
        {
            controller.Move(Physics.gravity * Time.deltaTime);
        }

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
    }

    private void HandleAiming()
    {
        float aimInput = 0f;
        if (Input.GetKey(KeyCode.I)) aimInput = -1f;
        if (Input.GetKey(KeyCode.K)) aimInput = 1f;
        if (aimInput != 0 && firePoint != null) firePoint.Rotate(aimInput * 40f * Time.deltaTime, 0, 0);
    }

    private void HandleShooting()
    {
        if (isReloading) return;

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

        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            chargeTimer += Time.deltaTime;
            float range = maxChargePower - minChargePower;
            float currentPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);
            if (powerSlider != null) powerSlider.value = currentPower;
        }

        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            float range = maxChargePower - minChargePower;
            LastFiredPower = minChargePower + Mathf.PingPong(chargeTimer * chargeSpeed, range);
            Debug.Log($"🚀 Power Locked at: {LastFiredPower}");
            FireProjectile();
            isCharging = false;
            chargeTimer = 0;
            if (powerSlider != null) { powerSlider.value = 0; powerSlider.gameObject.SetActive(false); }
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
        isReloading = false;
    }

    public void EnableControl(bool enable)
    {
        isControllable = enable;
        if (!enable)
        {
            isCharging = false;
            if (powerSlider != null) { powerSlider.value = 0; powerSlider.gameObject.SetActive(false); }
        }
        Debug.Log($"Player control: {(enable ? "ENABLED" : "DISABLED")}");
    }
}