using UnityEngine;

public class ArmSprayController : ArmController
{
    [Header("Spray Settings")]
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Spray Effect")]
    [SerializeField] private ParticleSystem sprayEffect;
    [SerializeField] private Transform fireDirection;
    [SerializeField] private float spraySpeed = 5f;
    [SerializeField] private float emissionRate = 50f; // particles per second while spraying

    [Header("Recoil")]
    [SerializeField] private float recoilOffset = 0.2f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeFrequency = 60f;

    [Header("Thermal")]
    [SerializeField] private ThermalObject armThermalObject;
    [SerializeField] private ThermalObject bodyThermalObject;

    private CameraController camController;
    private bool isSpraying = false;
    private Vector3 sprayOriginalLocalPosition;
    private Vector3 recoilTargetPosition;
    private bool isReturning = false;
    private float returnT = 0f;
    private float shakeTimer = 0f;
    private ParticleSystem.EmissionModule emissionModule;

    protected override void Start()
    {
        base.Start();
        camController = Camera.main.GetComponent<CameraController>();
        sprayOriginalLocalPosition = transform.localPosition;
        recoilTargetPosition = sprayOriginalLocalPosition + Vector3.left * recoilOffset;

        if (sprayEffect != null)
        {
            // Keep the system playing always, but emission rate zero initially
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
    }

    protected override void Update()
    {
        base.Update();
        if (!armEnabled) return;

        AimAtMouse();
        HandleShooting();

        if (isSpraying)
            UpdateSprayDirection();

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
        bool wantsSpray = Input.GetMouseButton(0);
        if (wantsSpray && !isSpraying)
            StartSpray();
        else if (!wantsSpray && isSpraying)
            StopSpray();
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

        if (sprayEffect != null)
        {
            // Set emission rate to desired value
            emissionModule.rateOverTime = emissionRate;
            UpdateSprayDirection();
            // Ensure system is playing
            if (!sprayEffect.isPlaying)
                sprayEffect.Play();
        }

        transform.localPosition = recoilTargetPosition;
        shakeTimer = 0f;

        /* IMPROVE IN THE FUTURE (Should be a constant drop)
        if (armThermalObject != null)
            armThermalObject.ChangeCurrentTemperature(-20f);
        if (bodyThermalObject != null)
            bodyThermalObject.ChangeCurrentTemperature(-15f);
            */

    }

    private void StopSpray()
    {
        isSpraying = false;

        if (sprayEffect != null)
        {
            // Set emission rate to zero – particles already emitted will continue their lifetime
            emissionModule.rateOverTime = 0f;
        }

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