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
    [SerializeField] private LayerMask hitMask = -1;

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
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
            enemy.ChangeBaseTemperature(damage);

        Vector2 hitPoint;
        Vector2 normal;

        Vector2 currentPos = transform.position;
        Vector2 toCurrent = currentPos - lastPosition;
        float distance = toCurrent.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(lastPosition, toCurrent.normalized, distance, hitMask);

        if (hit.collider != null && hit.collider.gameObject == other.gameObject)
        {
            hitPoint = hit.point;
            normal = hit.normal;
        }
        else
        {
            hitPoint = other.ClosestPoint(currentPos);
            normal = (currentPos - hitPoint).normalized;
            if (normal == Vector2.zero)
                normal = Vector2.up;
        }

        SpawnHitEffect(hitPoint, normal);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 hitPoint = contact.point;
        Vector2 normal = contact.normal;

        SpawnHitEffect(hitPoint, normal);

        // Apply damage if the collided object has an EnemyController
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
            enemy.ChangeBaseTemperature(damage);

        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector2 hitPoint, Vector2 normal)
    {
        if (hitEffectPrefab == null) return;

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