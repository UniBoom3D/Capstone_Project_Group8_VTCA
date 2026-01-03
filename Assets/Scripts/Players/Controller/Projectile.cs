using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 8f;          // thời gian tối đa bay
    public float explosionRadius = 3f;   // bán kính nổ (vùng ảnh hưởng)
    public GameObject explosionEffect;   // prefab hiệu ứng nổ (optional)
    public LayerMask hitLayers;

    [Header("Runtime State")]
    public bool isLaunched = false;
    private Rigidbody rb;
    private float launchPower;
    private PlayerBattleController owner;

    // Event callback khi nổ (BattleHandler sẽ lắng nghe)
    public event Action OnExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(float power, PlayerBattleController shooter)
    {
        isLaunched = true;
        owner = shooter;
        launchPower = power;

        rb.isKinematic = false;
        rb.AddForce(transform.forward * power, ForceMode.Impulse);

        Debug.Log($"💥 Projectile launched with {power} force from {owner.name}");
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLaunched) return;

        Debug.Log($"💢 Projectile hit: {collision.gameObject.name}");
        Explode();
    }

    private void Explode()
    {
        if (explosionEffect)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Tính sát thương vùng nổ
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);
        foreach (Collider col in hitObjects)
        {
            var participant = col.GetComponent<ITurnParticipant>();
            if (participant != null)
            {
                int damage = Mathf.RoundToInt(launchPower / 2f);
                participant.TakeDamage(damage);
                Debug.Log($"🔥 Hit {col.name} for {damage} dmg!");
            }
        }

        OnExploded?.Invoke();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
