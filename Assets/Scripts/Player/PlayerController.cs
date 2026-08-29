using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerController : MonoBehaviour, ISaveable
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
    public AudioClip suitDamageSound;
    public AudioSource audioSource;
    public AudioClip rechargeSound;

    [Header("Critical Health Audio")]
    public AudioSource heartbeatSource;
    public AudioClip heartbeatClip;
    public AudioSource maskBreathSource;
    public AudioClip maskBreathClip;
    public float healthAudioMinVolume = 0f;
    public float healthAudioMaxVolume = 1f;
    public float heartbeatMinPitch = 0.8f;
    public float heartbeatMaxPitch = 1.5f;
    public float coldThreshold = 30f;
    public float hotThreshold = 70f;

    [Header("Mouse Flip")]
    [SerializeField] private float flipThreshold = 0.5f;

    [Header("Misc")]
    public Animator bodyAnimator;
    public Volume dmgVolume;
    public float deathDelay = 2f;

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
    public static event System.Action OnPlayerDeath;

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

        if (heartbeatSource != null && heartbeatClip != null)
            heartbeatSource.clip = heartbeatClip;
        if (maskBreathSource != null && maskBreathClip != null)
            maskBreathSource.clip = maskBreathClip;

        SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    private void OnEnable()
    {
        GameSceneManager.OnSceneLoadedWithSpawnID += HandleSpawnPoint;
    }

    private void OnDisable()
    {
        StopLoopSound();
        if (heartbeatSource != null && heartbeatSource.isPlaying)
            heartbeatSource.Stop();
        if (maskBreathSource != null && maskBreathSource.isPlaying)
            maskBreathSource.Stop();

        GameSceneManager.OnSceneLoadedWithSpawnID -= HandleSpawnPoint;
    }

    void Update()
    {
        if (!(currentState is PlayerClimbingState) && mainCam != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            float dx = mousePos.x - transform.position.x;
            if (dx > flipThreshold && !isFacingRight && currentState is not PlayerDeathState)
                Flip();
            else if (dx < -flipThreshold && isFacingRight && currentState is not PlayerDeathState)
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

        if (suitDamageSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot2D(suitDamageSound, volumeScale: 0.5f);
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

    public void SetCriticalHealthIntensity(float intensity)
    {
        if (heartbeatSource == null || heartbeatClip == null) return;
        if (maskBreathSource == null || maskBreathClip == null) return;

        if (intensity <= 0f)
        {
            if (heartbeatSource.isPlaying)
                heartbeatSource.Stop();
            heartbeatSource.volume = 0f;
            if (maskBreathSource.isPlaying)
                maskBreathSource.Stop();
            maskBreathSource.volume = 0f;
            return;
        }

        float clamped = Mathf.Clamp01(intensity);
        heartbeatSource.volume = Mathf.Lerp(healthAudioMinVolume, healthAudioMaxVolume, clamped);
        heartbeatSource.pitch = Mathf.Lerp(heartbeatMinPitch, heartbeatMaxPitch, clamped);
        maskBreathSource.volume = Mathf.Lerp(healthAudioMinVolume, healthAudioMaxVolume, clamped);

        if (!heartbeatSource.isPlaying)
            heartbeatSource.Play();
        if (!maskBreathSource.isPlaying)
            maskBreathSource.Play();
    }

    private void HandleSpawnPoint(string spawnID)
    {
        if (string.IsNullOrEmpty(spawnID))
            return;

        SceneTeleporter[] teleporters = FindObjectsByType<SceneTeleporter>(FindObjectsSortMode.None);
        foreach (SceneTeleporter tp in teleporters)
        {
            if (tp.SpawnID == spawnID)
            {
                transform.position = tp.GetSpawnPosition();
                return;
            }
        }
    }

    public void Die()
    {
        ChangeState(new PlayerDeathState(this));
    }

    public void InvokeDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    public IEnumerator ReloadAfterDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        GameSceneManager.Instance.LoadScene("DeathScreen", saveBeforeLoad: false, saveAfterLoad: false);
    }

    public void Save(GameData data)
    {
        data.playerPosition = transform.position;
    }

    public void Load(GameData data)
    {
        transform.position = data.playerPosition;
    }
}