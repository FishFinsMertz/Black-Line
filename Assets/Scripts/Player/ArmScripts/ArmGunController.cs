using UnityEngine;

public class ArmGunController : ArmController
{
    [Header("Gun Settings")]
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Gun Details")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform directionIndicator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 5f; // shots per second

    [Header("Thermal")]
    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private CameraController camController;
    private float nextFireTime = 0f;

    protected override void Start()
    {
        base.Start();
        camController = Camera.main.GetComponent<CameraController>();
    }

    protected override void Update()
    {
        base.Update(); // handles flip and armEnabled check
        if (!armEnabled) return;

        AimAtMouse();
        HandleShooting();
    }

    private void AimAtMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

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
    }

    private void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (bulletPrefab == null || firePoint == null) return;
        if (Time.time < nextFireTime) return;

        float fireDelay = fireRate > 0 ? 1f / fireRate : 0f;
        nextFireTime = Time.time + fireDelay;

        Vector2 direction = (firePoint.position - directionIndicator.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        GunBullet bulletScript = bullet.GetComponent<GunBullet>();
        if (bulletScript != null)
        {
            if (camController != null)
                camController.TriggerShake(0.5f, 0.3f, 1f);
            bulletScript.Initialize(direction);
        }

        // Increase temperature on heat‑generating objects
        if (armThermalObject != null)
            armThermalObject.ChangeCurrentTemperature(25f);
        if (bodyThermalObject != null)
            bodyThermalObject.ChangeCurrentTemperature(20f);
    }
}