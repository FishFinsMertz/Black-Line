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
        // If Initialize wasn't called, use default direction
        if (direction == Vector2.zero)
            direction = Vector2.right;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (Time.time - spawnTime >= lifetime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy bullet when it hits anything
        Destroy(gameObject);
        
        // Damage logic here
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}