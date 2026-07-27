using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class ArmSprayController : ArmController
{
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [SerializeField] private ParticleSystem sprayEffect;
    [SerializeField] private Transform fireDirection;
    [SerializeField] private float spraySpeed = 5f;
    [SerializeField] private float emissionRate = 50f;

    [SerializeField] private float ammoPerSecond = 10f;
    [SerializeField] private float tempDrainPerSecond = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private float audioFadeIn = 0.2f;
    [SerializeField] private float audioFadeOut = 0.3f;
    [SerializeField] private AudioClip reloadSound;

    [Header("Reload")]
    [SerializeField] private float reloadDuration = 1.5f;

    [SerializeField] private float recoilOffset = 0.2f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeFrequency = 60f;

    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private CameraController camController;
    private GeneralThermalRegulator thermalRegulator;
    private bool isSpraying = false;
    private Vector3 sprayOriginalLocalPosition;
    private Vector3 recoilTargetPosition;
    private bool isReturning = false;
    private float returnT = 0f;
    private float shakeTimer = 0f;
    private ParticleSystem.EmissionModule emissionModule;
    private float ammoTimer = 0f;

    private bool isReloading = false;
    private float reloadTimer = 0f;
    private Quaternion idleRotation;

    protected override void Start()
    {
        base.Start();
        camController = Camera.main.GetComponent<CameraController>();
        thermalRegulator = GetComponentInParent<GeneralThermalRegulator>();

        sprayOriginalLocalPosition = transform.localPosition;
        recoilTargetPosition = sprayOriginalLocalPosition + Vector3.left * recoilOffset;
        idleRotation = Quaternion.identity;

        if (sprayEffect != null)
        {
            emissionModule = sprayEffect.emission;
            emissionModule.rateOverTime = 0f;
            if (!sprayEffect.isPlaying)
                sprayEffect.Play();
        }
    }

    private void OnDisable()
    {
        StopSpray();
        transform.localPosition = sprayOriginalLocalPosition;
        isReturning = false;
        if (emissionModule.enabled)
            emissionModule.rateOverTime = 0f;
        isReloading = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    protected override void Update()
    {
        base.Update();
        if (!armEnabled) return;

        UpdateReload();

        if (isReloading)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, idleRotation, Time.deltaTime * 10f);
            return;
        }

        AimAtMouse();
        HandleShooting();
        HandleReload();

        if (isSpraying)
        {
            UpdateSprayDirection();

            if (thermalRegulator != null)
                thermalRegulator.ChangeGlobalCurrentTemperature(-tempDrainPerSecond * Time.deltaTime);

            if (player != null && player.inventory != null)
            {
                ammoTimer += Time.deltaTime;
                float interval = 1f / ammoPerSecond;
                if (ammoTimer >= interval)
                {
                    if (!player.inventory.UseAmmo("Spray", 1))
                    {
                        StopSpray();
                        return;
                    }
                    ammoTimer = 0f;
                }
            }
        }

        if (isReturning)
        {
            returnT += Time.deltaTime * recoilReturnSpeed;
            if (returnT >= 1f)
            {
                transform.localPosition = sprayOriginalLocalPosition;
                isReturning = false;
            }
            else
            {
                transform.localPosition = Vector3.Lerp(recoilTargetPosition, sprayOriginalLocalPosition, returnT);
            }
        }

        if (isSpraying && camController != null)
        {
            shakeTimer += Time.deltaTime;
            if (shakeTimer >= 1f / shakeFrequency)
            {
                shakeTimer = 0f;
                camController.TriggerShake(shakeIntensity, 0.1f, 1f);
            }
        }
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
        if (isReloading) return;
        bool wantsSpray = Input.GetMouseButton(0);
        if (wantsSpray && !isSpraying)
        {
            if (player != null && player.inventory != null)
            {
                var ammo = player.inventory.GetAmmo("Spray");
                if (ammo.magazine <= 0) return;
                StartSpray();
            }
            else StartSpray();
        }
        else if (!wantsSpray && isSpraying)
            StopSpray();
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (player != null && player.inventory != null)
            {
                var ammo = player.inventory.GetAmmo("Spray");
                int capacity = player.inventory.GetCapacity("Spray");
                if (ammo.reserve > 0 && ammo.magazine < capacity && !isReloading)
                {
                    if (isSpraying) StopSpray();
                    isReloading = true;
                    reloadTimer = reloadDuration;
                    if (reloadSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayOneShot(reloadSound, transform.position, volumeScale: 0.7f);
                    }
                    StartCoroutine(SmoothResetToIdle());
                }
            }
        }
    }

    private void UpdateReload()
    {
        if (!isReloading) return;
        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            isReloading = false;
            if (player != null && player.inventory != null)
                player.inventory.Reload("Spray");
        }
    }

    private IEnumerator SmoothResetToIdle()
    {
        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 targetPos = sprayOriginalLocalPosition;
        Quaternion targetRot = idleRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
    }

    private void UpdateSprayDirection()
    {
        if (sprayEffect == null || fireDirection == null) return;

        Vector3 dir = (fireDirection.position - sprayEffect.transform.position).normalized;
        var vel = sprayEffect.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(dir.x * spraySpeed);
        vel.y = new ParticleSystem.MinMaxCurve(dir.y * spraySpeed);
    }

    private void StartSpray()
    {
        isSpraying = true;
        isReturning = false;
        ammoTimer = 0f;

        if (sprayEffect != null)
        {
            emissionModule.rateOverTime = emissionRate;
            UpdateSprayDirection();
            if (!sprayEffect.isPlaying)
                sprayEffect.Play();
        }

        if (spraySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ConfigureLoop(
                audioSource,
                spraySound,
                fadeInDuration: audioFadeIn,
                volumeScale: 0.7f,
                minDistance: 3f,
                maxDistance: 50f
            );
        }

        transform.localPosition = recoilTargetPosition;
        shakeTimer = 0f;
    }

    private void StopSpray()
    {
        isSpraying = false;

        if (sprayEffect != null)
            emissionModule.rateOverTime = 0f;

        if (audioSource != null && audioSource.isPlaying)
            AudioManager.Instance?.FadeOut(audioSource, audioFadeOut);

        isReturning = true;
        returnT = 0f;
    }

    private Vector3 GetAimDirection()
    {
        if (cam == null) return Vector3.right;

        if (fireDirection != null && sprayEffect != null)
        {
            Vector3 direction = (fireDirection.position - sprayEffect.transform.position).normalized;
            if (direction != Vector3.zero)
                return direction;
        }

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 directionToMouse = (mousePos - transform.position).normalized;
        return directionToMouse != Vector3.zero ? directionToMouse : Vector3.right;
    }

    public void SetFireDirection(Transform newDirection) => fireDirection = newDirection;
}