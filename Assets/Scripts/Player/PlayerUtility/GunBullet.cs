using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float damage = 30f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float surfaceOffset = 0.05f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitMask = -1; // Default: Everything (you should assign in Inspector)

    private Vector2 direction;
    private float spawnTime;
    private Rigidbody2D rb;
    private Vector2 lastPosition;

    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;
        spawnTime = Time.time;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * speed;
        lastPosition = transform.position;
    }

    private void Start()
    {
        if (direction == Vector2.zero) direction = Vector2.right;
    }

    private void FixedUpdate()
    {
        // Store the previous position for accurate raycasting
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Raycast from last position to current to find exact hit point and normal
        Vector2 currentPos = transform.position;
        Vector2 toCurrent = currentPos - lastPosition;
        float distance = toCurrent.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(lastPosition, toCurrent.normalized, distance, hitMask);

        if (hit.collider != null)
        {
            SpawnHitEffect(hit.point, hit.normal);

            // Apply damage if the hit object is an enemy
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.ChangeBaseTemperature(damage);
        }
        else
        {
            SpawnHitEffect(currentPos, Vector2.zero);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Use the first contact point for accurate effect placement
        ContactPoint2D contact = collision.contacts[0];
        Vector2 hitPoint = contact.point;
        Vector2 normal = contact.normal;

        SpawnHitEffect(hitPoint, normal);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null) enemy.ChangeBaseTemperature(damage);
        }

        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector2 hitPoint, Vector2 normal)
    {
        if (hitEffectPrefab == null) return;

        // Offset the effect slightly outward from the surface to avoid clipping
        Vector3 spawnPos = (Vector3)hitPoint + (Vector3)normal * surfaceOffset;
        GameObject effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration;
            Destroy(effect, duration);
        }
        else
        {
            Destroy(effect, 1f);
        }
    }
}