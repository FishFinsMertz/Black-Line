using UnityEngine;

public class ArmGunController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool aimEnabled = true;
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Flip")]
    [SerializeField] private PlayerController player;
    [SerializeField] private float flipThreshold = 0.1f;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform directionIndicator;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Thermal Vision")]
    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private void Start()
    {
        if (player == null)
            player = GetComponentInParent<PlayerController>();
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

        // Shoot
        if (Input.GetMouseButtonDown(0) && bulletPrefab != null && firePoint != null)
        {
            Vector2 direction = (firePoint.position - directionIndicator.position).normalized;

            // Instantiate bullet at firePoint position, with no rotation
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            // Get the GunBullet component and initialize it with direction
            GunBullet bulletScript = bullet.GetComponent<GunBullet>();
            if (bulletScript != null)
                bulletScript.Initialize(direction);

            // Increase temperature
            if (armThermalObject != null)
                armThermalObject.ChangeCurrentTemperature(20f);
            if (bodyThermalObject != null)
                bodyThermalObject.ChangeCurrentTemperature(10f); 
        }
    }
}