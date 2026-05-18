using UnityEngine;

public class ArmGunController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool aimEnabled = true;
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Flip")]
    [SerializeField] private float flipThreshold = 0.1f;

    [Header("Gun Details")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform directionIndicator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 5f; // Shots per second (higher = faster)

    [Header("Thermal Vision")]
    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private CameraController cam;
    private PlayerController player;
    private float nextFireTime = 0f;

    private void Start()
    {
        if (player == null)
            player = GetComponentInParent<PlayerController>();
        cam = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        if (!aimEnabled) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // FLIP LOGIC
        float dx = mousePos.x - player.transform.position.x;
        if (dx > flipThreshold && !player.isFacingRight)
            player.Flip();
        else if (dx < -flipThreshold && player.isFacingRight)
            player.Flip();

        // AIM LOGIC
        Vector2 dir = mousePos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (player.isFacingRight)
        {
            angle = Mathf.DeltaAngle(0f, angle);
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            angle = Mathf.DeltaAngle(0f, angle - 180f);
            angle = Mathf.Clamp(angle, -maxAngle, -minAngle);
            transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        // SHOOT with fire rate limit
        if (Input.GetMouseButtonDown(0) && bulletPrefab != null && firePoint != null && Time.time >= nextFireTime)
        {
            // Calculate next allowed shot time
            float fireDelay = fireRate > 0 ? 1f / fireRate : 0f;
            nextFireTime = Time.time + fireDelay;

            Vector2 direction = (firePoint.position - directionIndicator.position).normalized;

            // Instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            GunBullet bulletScript = bullet.GetComponent<GunBullet>();
            if (bulletScript != null)
            {
                // Camera shake
                cam.TriggerShake(0.5f, 0.3f, 1f);
                bulletScript.Initialize(direction);
            }

            // Increase temperature
            if (armThermalObject != null)
                armThermalObject.ChangeCurrentTemperature(20f);
            if (bodyThermalObject != null)
                bodyThermalObject.ChangeCurrentTemperature(10f);
        }
    }
}