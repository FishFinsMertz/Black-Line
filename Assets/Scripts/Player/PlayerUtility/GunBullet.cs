using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab; // assign your particle system prefab here

    private Vector2 direction;
    private float spawnTime;
    private Rigidbody2D rb;

    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;
        spawnTime = Time.time;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * speed;
    }

    private void Start()
    {
        if (direction == Vector2.zero) direction = Vector2.right;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnHit(collision.gameObject);
    }

    private void OnHit(GameObject hitObject)
    {
        // Instantiate hit effect at bullet position, NOT as child
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float duration = ps.main.duration;
                Destroy(effect, duration);
            }
            else
            {
                Destroy(effect, 1f); // fallback
            }
        }

        if (hitObject.CompareTag("Enemy"))
        {
            EnemyController enemy = hitObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ChangeBaseTemperature(10f); // example damage value
            }
        }
        
        Destroy(gameObject);
    }
}