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
    private bool isLaunched = false;
    private CameraFollowProjectile _bulletCam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _bulletCam = GetComponentInChildren<CameraFollowProjectile>();
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

        if (_bulletCam != null)
        {
            _bulletCam.ActivateCamera(this.transform);
        }

        Destroy(gameObject, 20f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetHit || !enabled) return;

        targetHit = true;
        // THÔNG BÁO CAMERA DỪNG TRACKING
        if (_bulletCam != null)
        {
            _bulletCam.OnProjectileHit();
        }
        Explode();

        // Xử lý sát thương
        ITurnParticipant victim = collision.gameObject.GetComponent<ITurnParticipant>();
        if (victim != null && victim != myShooter)
        {
            victim.TakeDamage(25);
        }

        // Dọn dẹp visual
        if (projectileMesh != null) projectileMesh.enabled = false;
        if (inFlightAudioSource != null) inFlightAudioSource.Stop();
        if (disableOnHit != null) disableOnHit.Stop();
        foreach (Collider col in GetComponents<Collider>()) col.enabled = false;

        rb.isKinematic = true; // Dừng vật lý ngay khi nổ
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem layer có nằm trong mask không
        if (!targetHit && ((1 << other.gameObject.layer) & collisionLayerMask) != 0)
        {
            if (_bulletCam != null)
            {
                // Ngắt Follow để camera đứng yên chuẩn bị xem nổ
                _bulletCam.DetachFollow();
            }
        }
    }

    private void Explode()
    {
        if (rocketExplosion != null)
            Instantiate(rocketExplosion, transform.position, Quaternion.identity);
    }
}