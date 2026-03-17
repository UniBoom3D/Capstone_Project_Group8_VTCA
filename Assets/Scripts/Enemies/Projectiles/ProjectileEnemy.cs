using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileEnemy : MonoBehaviour
{
    [Header("Settings")]
    public float powerMultiplier = 0.5f;
    // Layer của môi trường và Player để đạn biết khi nào cần nổ
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
    private bool isLaunched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Đợi cho đến khi được bắn
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {

        if (isLaunched && !targetHit && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    public void Launch(float power, ITurnParticipant shooter)
    {
        myShooter = shooter;
        rb.useGravity = true;
        isLaunched = true;

        
        Vector3 launchVelocity = transform.forward * power * powerMultiplier;
        rb.linearVelocity = launchVelocity;


        if (inFlightAudioSource != null && !inFlightAudioSource.isPlaying)
            inFlightAudioSource.Play();

        
        Destroy(gameObject, 20f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit || !enabled) return;

        targetHit = true;
        Explode();

        // 🎯 CHỈ GÂY SÁT THƯƠNG CHO PLAYER
        if (collision.gameObject.CompareTag("Player"))
        {
            ITurnParticipant victim = collision.gameObject.GetComponent<ITurnParticipant>();
            if (victim != null)
            {
                victim.TakeDamage(25);
                Debug.Log("<color=red>🎯 AI hit Player!</color>");
            }
        }

        if (projectileMesh != null) projectileMesh.enabled = false;
        if (inFlightAudioSource != null) inFlightAudioSource.Stop();
        if (disableOnHit != null) disableOnHit.Stop();
        foreach (Collider col in GetComponents<Collider>()) col.enabled = false;

        rb.isKinematic = true;
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