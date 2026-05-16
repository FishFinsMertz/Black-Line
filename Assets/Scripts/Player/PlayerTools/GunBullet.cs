using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;

    private Vector2 direction;
    private float spawnTime;

    // Called by ArmGunController after instantiation
    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;
        spawnTime = Time.time;
    }

    private void Start()
    {
        // If Initialize wasn't called (e.g., bullet placed manually), set a default direction
        if (direction == Vector2.zero)
            direction = Vector2.right;
    }

    private void Update()
    {
        // Move the bullet
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime
        if (Time.time - spawnTime >= lifetime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy bullet when it hits anything
        Destroy(gameObject);
        
        // Optional: add damage logic here
        // e.g., collision.gameObject.GetComponent<Enemy>()?.TakeDamage(1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}