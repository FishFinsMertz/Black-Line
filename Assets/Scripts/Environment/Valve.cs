using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class Valve : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string rotateTrigger = "Rotate";
    [SerializeField] private string idleBool = "isIdle";
    [SerializeField] private float animationCooldown = 0.5f;

    [Header("Smooth Transitions")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float openFresnelRadius = 1f;
    [SerializeField] private float closedFresnelRadius = 0f;
    [SerializeField] private float openEmissionRate = 50f;
    [SerializeField] private float closedEmissionRate = 0f;

    [Header("Light")]
    [SerializeField] private Light2D valveLight;
    [SerializeField] private float openLightIntensity = 2f;
    [SerializeField] private float closedLightIntensity = 0f;

    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticle;
    [SerializeField] private List<TileMapThermal> tilemapThermals;
    [SerializeField] private SpriteRenderer outline;

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    private bool isOpen;
    private bool isAnimating;
    private float cooldownTimer;
    private bool playerInRange;

    private ParticleSystem.EmissionModule emissionModule;
    private float currentEmissionRate;
    private float currentFresnelRadius;
    private float currentLightIntensity;

    private void Start()
    {
        isOpen = startOpen;
        if (sprayParticle != null)
            emissionModule = sprayParticle.emission;
        else
            Debug.LogWarning("Valve: sprayParticle not assigned!");

        currentEmissionRate = isOpen ? openEmissionRate : closedEmissionRate;
        currentFresnelRadius = isOpen ? openFresnelRadius : closedFresnelRadius;
        currentLightIntensity = isOpen ? openLightIntensity : closedLightIntensity;

        ApplyStateInstant(isOpen);

        if (animator != null)
            animator.SetBool(idleBool, true);

        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        // Update cooldown timer
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        bool showOutline = playerInRange && !isAnimating && cooldownTimer <= 0;
        if (outline != null && outline.enabled != showOutline)
            outline.enabled = showOutline;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isAnimating && cooldownTimer <= 0)
                ToggleValve();
        }
    }

    private void ToggleValve()
    {
        isAnimating = true;
        cooldownTimer = animationCooldown;

        if (outline != null)
            outline.enabled = false;

        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(rotateTrigger);
        }

        isOpen = !isOpen;
        StartCoroutine(SmoothTransition(isOpen));
        StartCoroutine(ReturnToIdle());
    }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(animationCooldown);
        if (animator != null)
            animator.SetBool(idleBool, true);
        isAnimating = false;
    }

    private IEnumerator SmoothTransition(bool toOpen)
    {
        float targetEmission = toOpen ? openEmissionRate : closedEmissionRate;
        float targetRadius = toOpen ? openFresnelRadius : closedFresnelRadius;
        float targetLight = toOpen ? openLightIntensity : closedLightIntensity;

        float startEmission = currentEmissionRate;
        float startRadius = currentFresnelRadius;
        float startLight = currentLightIntensity;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float lerpedEmission = Mathf.Lerp(startEmission, targetEmission, t);
            float lerpedRadius = Mathf.Lerp(startRadius, targetRadius, t);
            float lerpedLight = Mathf.Lerp(startLight, targetLight, t);

            if (sprayParticle != null)
                emissionModule.rateOverTime = lerpedEmission;

            foreach (var tm in tilemapThermals)
                if (tm != null)
                    tm.SetFresnelRadius(lerpedRadius);

            if (valveLight != null)
                valveLight.intensity = lerpedLight;

            yield return null;
        }

        // Snap to final values
        currentEmissionRate = targetEmission;
        currentFresnelRadius = targetRadius;
        currentLightIntensity = targetLight;

        if (sprayParticle != null)
            emissionModule.rateOverTime = targetEmission;
        foreach (var tm in tilemapThermals)
            if (tm != null)
                tm.SetFresnelRadius(targetRadius);
        if (valveLight != null)
            valveLight.intensity = targetLight;
    }

    private void ApplyStateInstant(bool open)
    {
        float targetEmission = open ? openEmissionRate : closedEmissionRate;
        float targetRadius = open ? openFresnelRadius : closedFresnelRadius;
        float targetLight = open ? openLightIntensity : closedLightIntensity;

        currentEmissionRate = targetEmission;
        currentFresnelRadius = targetRadius;
        currentLightIntensity = targetLight;

        if (sprayParticle != null)
            emissionModule.rateOverTime = targetEmission;
        foreach (var tm in tilemapThermals)
            if (tm != null)
                tm.SetFresnelRadius(targetRadius);
        if (valveLight != null)
            valveLight.intensity = targetLight;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
        if (outline != null)
            outline.enabled = false;
    }

    public bool IsOpen() => isOpen;
}