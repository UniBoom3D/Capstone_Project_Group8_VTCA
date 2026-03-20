using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float powerMultiplier = 0.5f;
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
<<<<<<< HEAD
    private float _damage = 25f;
=======
    private bool isLaunched = false;
>>>>>>> main

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
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

        // 🎥 LOGIC CAMERA CHỈ CHO PLAYER:
        if (shooter is MonoBehaviour monoShooter && monoShooter.CompareTag("Player"))
        {
            CameraFollowPlayer cam = Object.FindFirstObjectByType<CameraFollowPlayer>();
            if (cam != null)
            {
                cam.SetProjectileTarget(this.transform);
            }
        }

        Destroy(gameObject, 20f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit || !enabled) return;

        targetHit = true;
        Explode();

<<<<<<< HEAD
            victim.TakeDamage(_damage);
            Debug.Log($"🎯 Hit {victim.Name}!");
=======
        // Xử lý sát thương
        ITurnParticipant victim = collision.gameObject.GetComponent<ITurnParticipant>();
        if (victim != null && victim != myShooter)
        {
            victim.TakeDamage(25);
>>>>>>> main
        }

        // Dọn dẹp visual
        if (projectileMesh != null) projectileMesh.enabled = false;
        if (inFlightAudioSource != null) inFlightAudioSource.Stop();
        if (disableOnHit != null) disableOnHit.Stop();
        foreach (Collider col in GetComponents<Collider>()) col.enabled = false;

        rb.isKinematic = true; // Dừng vật lý ngay khi nổ
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!targetHit && ((1 << other.gameObject.layer) & collisionLayerMask) != 0)
        {
            // Gọi hàm ngắt Follow trong Camera script
            CameraFollowPlayer cam = Object.FindFirstObjectByType<CameraFollowPlayer>();
            if (cam != null) cam.DetachFollow();
        }
    }

    private void Explode()
    {
        if (rocketExplosion != null)
            Instantiate(rocketExplosion, transform.position, Quaternion.identity);
    }
    public void SetDamage(float newDamage)
    {
        _damage = newDamage;
    }
}