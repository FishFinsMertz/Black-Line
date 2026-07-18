using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float climbSpeed = 3f;
    public float runAcceleration = 15f;
    public float runDeceleration = 10f;
    public KeyCode runKey = KeyCode.LeftShift;
    public bool isFacingRight = true;

    [Header("Audio")]
    public AudioClip walkSound;
    public AudioClip runSound;
    public AudioClip climbSound;
    public AudioSource audioSource;

    [Header("Mouse Flip")]
    [SerializeField] private float flipThreshold = 0.5f;

    [Header("Misc")]
    public Animator bodyAnimator;
    public Volume dmgVolume;

    [Header("Damage Flash")]
    public float damageFlashMaxWeight = 0.7f;
    public float damageFlashRiseDuration = 0.1f;
    public float damageFlashFallDuration = 0.4f;

    private GeneralThermalRegulator thermalRegulator;
    [HideInInspector] public Rigidbody2D rb { get; private set; }
    private PlayerState currentState;
    [HideInInspector] public Inventory inventory;
    private CameraController camController;
    private Coroutine damageFlashCoroutine;
    private Camera mainCam;

    public enum SubState { None, WalkBack, ClimbDown, ClimbPause }
    public SubState currentSubState = SubState.None;

    public static event System.Action OnPlayerDamaged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = new PlayerIdleState(this);
        inventory = GetComponent<Inventory>();
        thermalRegulator = GetComponent<GeneralThermalRegulator>();
        camController = Camera.main.GetComponent<CameraController>();
        mainCam = Camera.main;
        currentState.Enter();

        if (dmgVolume != null) dmgVolume.weight = 0f;
    }

    private void OnDisable()
    {
        StopLoopSound();
    }

    void Update()
    {
        if (!(currentState is PlayerClimbingState) && mainCam != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            float dx = mousePos.x - transform.position.x;
            if (dx > flipThreshold && !isFacingRight)
                Flip();
            else if (dx < -flipThreshold && isFacingRight)
                Flip();
        }

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
        thermalRegulator.ChangeBaseTemperature(tempChange);
        camController.TriggerShake(0.5f, 0.5f, 1f);
        OnPlayerDamaged?.Invoke();
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
        while (elapsed < damageFlashRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageFlashRiseDuration;
            dmgVolume.weight = Mathf.Lerp(0f, damageFlashMaxWeight, t);
            yield return null;
        }
        dmgVolume.weight = damageFlashMaxWeight;
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

    public PlayerState GetPlayerCurrentState()
    {
        return currentState;
    }

    public void PlayLoopSound(AudioClip clip)
    {
        if (clip == null) return;

        StopLoopSound();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ConfigureLoop(
                audioSource,
                clip,
                minDistance: 5f,
                maxDistance: 40f
            );
        }
    }

    public void StopLoopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            AudioManager.Instance?.FadeOut(audioSource, 0f);
        }
    }
}