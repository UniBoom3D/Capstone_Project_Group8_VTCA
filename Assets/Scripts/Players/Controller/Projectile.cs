using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 100; // Not used directly for force anymore, but good to keep
    public LayerMask collisionLayerMask;

    [Header("References")]
    public GameObject rocketExplosion;
    public MeshRenderer projectileMesh;
    public AudioSource inFlightAudioSource;
    public ParticleSystem disableOnHit;

    // Internal State
    private bool targetHit;
    private Rigidbody rb;
    private ITurnParticipant myShooter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity until fired
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
    }

    /// <summary>
    /// Fires the projectile with a specific velocity calculated by the controller
    /// </summary>
    public void Launch(float power, ITurnParticipant shooter)
    {
        myShooter = shooter;
        rb.useGravity = true; // Gravity ON

        // 🚀 PHYSICS: Convert Power to Velocity
        // We multiply by 0.5f to make the charge bar feel responsive without shooting into space
        Vector3 launchVelocity = transform.forward * power * 0.5f;

        rb.linearVelocity = launchVelocity;

        // Play Audio
        if (inFlightAudioSource != null && !inFlightAudioSource.isPlaying)
            inFlightAudioSource.Play();

        // Safety: Destroy after 8 seconds if it hits nothing
        Destroy(gameObject, 8f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit || !enabled) return;

        // 1. Damage Logic
        ITurnParticipant victim = collision.gameObject.GetComponent<ITurnParticipant>();
        if (victim != null)
        {
            // Don't hurt yourself (optional, but good for safety)
            if (victim == myShooter) return;

            victim.TakeDamage(25);
            Debug.Log($"🎯 Hit {victim.Name}!");
        }

        // 2. Visuals
        Explode();
        targetHit = true;

        if (projectileMesh != null) projectileMesh.enabled = false;
        if (inFlightAudioSource != null) inFlightAudioSource.Stop();
        if (disableOnHit != null) disableOnHit.Stop();

        foreach (Collider col in GetComponents<Collider>()) col.enabled = false;

        // 3. Destroy to end turn
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