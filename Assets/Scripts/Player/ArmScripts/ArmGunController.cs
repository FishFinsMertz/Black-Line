using UnityEngine;
using System.Collections;

public class ArmGunController : ArmController
{
    [Header("Gun Settings")]
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Gun Details")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform directionIndicator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 5f;

    [Header("Recoil")]
    [SerializeField] private float recoilStrength = 0.2f;
    [SerializeField] private float recoilDuration = 0.1f;

    [Header("Muzzle Flash")]
    [SerializeField] private Animator muzzleFlashAnimator;

    [Header("Thermal")]
    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private CameraController camController;
    private float nextFireTime = 0f;
    private bool isRecoiling = false;

    protected override void Start()
    {
        base.Start();
        camController = Camera.main.GetComponent<CameraController>();
        originalLocalPosition = transform.localPosition;
    }

    private void OnDisable()
    {
        if (isRecoiling)
        {
            StopAllCoroutines();
            transform.localPosition = originalLocalPosition;
            isRecoiling = false;
        }
    }

    protected override void Update()
    {
        base.Update();
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

        // Spawn bullet
        Vector2 direction = (firePoint.position - directionIndicator.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        GunBullet bulletScript = bullet.GetComponent<GunBullet>();
        if (bulletScript != null)
        {
            if (camController != null)
                camController.TriggerShake(0.5f, 0.3f, 1f);
            bulletScript.Initialize(direction);
        }

        // Increase temperature
        if (armThermalObject != null)
            armThermalObject.ChangeCurrentTemperature(20f);
        if (bodyThermalObject != null)
            bodyThermalObject.ChangeCurrentTemperature(15f);

        if (muzzleFlashAnimator != null)
            muzzleFlashAnimator.SetTrigger("Shoot");

        if (!isRecoiling && recoilStrength > 0f)
            StartCoroutine(Recoil());
    }

    private IEnumerator Recoil()
    {
        isRecoiling = true;

        float elapsed = 0f;
        float halfDuration = recoilDuration / 2f;

        // Move backward
        Vector3 startPos = originalLocalPosition;
        Vector3 recoilPos = startPos + Vector3.left * recoilStrength;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            transform.localPosition = Vector3.Lerp(startPos, recoilPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = recoilPos;

        // Return to original
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            transform.localPosition = Vector3.Lerp(recoilPos, startPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = startPos;

        isRecoiling = false;
    }
}