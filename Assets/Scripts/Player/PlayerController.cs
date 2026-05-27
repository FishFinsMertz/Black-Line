using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float runAcceleration = 15f; 
    public float runDeceleration = 10f;
    public KeyCode runKey = KeyCode.LeftShift;
    public bool isFacingRight = true;

    [Header("Misc")]
    public Animator bodyAnimator;
    public Volume dmgVolume;                // assign your damage post‑processing volume here
    [Header("Damage Flash")]
    public float damageFlashMaxWeight = 0.7f;
    public float damageFlashRiseDuration = 0.1f;   // how fast it reaches max
    public float damageFlashFallDuration = 0.4f;   // how long it fades back to 0

    private GeneralThermalRegulator thermalRegulator;

    [HideInInspector] public Rigidbody2D rb { get; private set; }

    private PlayerState currentState;
    [HideInInspector] public Inventory inventory;
    private CameraController camController;

    private Coroutine damageFlashCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = new PlayerIdleState(this);
        inventory = GetComponent<Inventory>();
        thermalRegulator = GetComponent<GeneralThermalRegulator>();
        camController = Camera.main.GetComponent<CameraController>();
        currentState.Enter();

        // Ensure damage volume starts at weight 0
        if (dmgVolume != null) dmgVolume.weight = 0f;
    }

    void Update()
    {
        currentState?.Update();
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public PlayerState GetCurrentState()
    {
        return currentState;
    }

    public void TakeDamage(float tempChange)
    {
        // Apply temperature change
        thermalRegulator.ChangeGlobalBaseTemperature(tempChange);
        // Camera shake
        camController.TriggerShake(0.5f, 0.5f, 1f);
        // Damage post‑processing flash
        if (dmgVolume != null)
        {
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        float elapsed = 0f;
        // Rise to max weight
        while (elapsed < damageFlashRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageFlashRiseDuration;
            dmgVolume.weight = Mathf.Lerp(0f, damageFlashMaxWeight, t);
            yield return null;
        }
        dmgVolume.weight = damageFlashMaxWeight;

        // Fall to 0
        elapsed = 0f;
        while (elapsed < damageFlashFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageFlashFallDuration;
            dmgVolume.weight = Mathf.Lerp(damageFlashMaxWeight, 0f, t);
            yield return null;
        }
        dmgVolume.weight = 0f;
        damageFlashCoroutine = null;
    }
}