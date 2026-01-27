using UnityEngine;
using System.Collections;

// 1. Rename the class to match what PlayerBattleController expects: "Projectile"
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    // --- Config ---
    [Header("Settings")]
    public float speed = 100; // Used as force multiplier now
    public LayerMask collisionLayerMask;

    // --- Explosion VFX ---
    public GameObject rocketExplosion;

    // --- Projectile Mesh ---
    public MeshRenderer projectileMesh;

    // --- Audio ---
    public AudioSource inFlightAudioSource;

    // --- VFX ---
    public ParticleSystem disableOnHit;

    // --- Internal State ---
    private bool targetHit;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity until we launch
    }

    /// <summary>
    /// Called by PlayerBattleController to fire the bullet
    /// </summary>
    public void Launch(float power, ITurnParticipant shooter)
    {
        // Enable physics now that we are firing
        rb.useGravity = true;

        // Calculate Launch Force: Forward + Upward Arc
        // We use 'power' from your charge bar combined with 'speed'
        Vector3 force = transform.forward * power * 0.5f + transform.up * (power * 0.2f);

        rb.AddForce(force, ForceMode.Impulse);

        // Play Audio
        if (inFlightAudioSource != null && !inFlightAudioSource.isPlaying)
            inFlightAudioSource.Play();

        // Safety: Destroy after 8 seconds if it hits nothing so the turn ends
        Destroy(gameObject, 8f);
    }

    // We removed Update() because we are using Physics (Rigidbody) for movement now!
    // This allows the bullet to arc like in Gunny/Worms instead of flying straight.

    /// <summary>
    /// Explodes on contact.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Prevent double hits
        if (targetHit || !enabled) return;

        // 1. Logic: Apply Damage
        ITurnParticipant victim = collision.gameObject.GetComponent<ITurnParticipant>();
        if (victim != null)
        {
            victim.TakeDamage(25); // Apply 25 damage (you can change this later)
            Debug.Log($"🎯 Hit {victim.Name}!");
        }

        // 2. Visuals: Explode
        Explode();
        targetHit = true;

        // 3. Cleanup: Disable mesh/colliders so it looks "gone" immediately
        if (projectileMesh != null) projectileMesh.enabled = false;
        if (inFlightAudioSource != null) inFlightAudioSource.Stop();
        if (disableOnHit != null) disableOnHit.Stop();

        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        // 4. IMPORTANT: Destroy the object. 
        // The BattleHandler is waiting for this object to become 'null' to end the turn.
        // We wait a tiny bit (2s) for trail particles to fade, then destroy.
        Destroy(gameObject, 2f);
    }

    private void Explode()
    {
        if (rocketExplosion != null)
        {
            Instantiate(rocketExplosion, transform.position, Quaternion.identity);
        }
    }
}