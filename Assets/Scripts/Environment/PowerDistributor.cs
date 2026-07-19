using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

public class PowerDistributor : MonoBehaviour, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("State")]
    [SerializeField] private bool startOn = false;
    private bool isOn = false;

    [Header("Audio")]
    [SerializeField] private AudioClip bootSound;
    [SerializeField] private AudioClip humSound;
    [SerializeField] private AudioSource humSource;
    [SerializeField] private float humVolumeScale = 0.4f;
    [SerializeField] private float humFadeIn = 2f;
    [SerializeField] private float humFadeOut = 2f;

    [Header("Heat Source (Optional)")]
    [SerializeField] private bool requireHeatSource = false;
    [SerializeField] private TileMapThermal sourceTilemap;

    [Header("Blade Settings")]
    [SerializeField] private Transform bladeTransform;
    [SerializeField] private float spinAcceleration = 100f;
    [SerializeField] private float spinDeceleration = 80f;
    [SerializeField] private float maxSpinSpeed = 720f;
    [SerializeField] private bool spinClockwise = true;

    private float currentSpinSpeed = 0f;

    [Header("Affected Thermal Objects")]
    [SerializeField] private List<ThermalObject> affectedThermalObjects = new List<ThermalObject>();
    [SerializeField] private List<TileMapThermal> affectedTilemaps = new List<TileMapThermal>();
    [SerializeField] private float onTemperature = 100f;
    [SerializeField] private float offTemperature = 50f;

    [Header("Messages")]
    [SerializeField] private string failMessage;

    [Header("Events")]
    public UnityEvent onTurnOn;
    public UnityEvent onTurnOff;
    public UnityEvent onFail;

    private void Start()
    {
        if (humSource == null)
            humSource = gameObject.AddComponent<AudioSource>();

        SaveManager.Instance?.Register(this);
        isOn = startOn;
        if (isOn)
        {
            if (!IsHeatSourceValid())
                isOn = false;
        }
        ApplyState();
    }

    private void Update()
    {
        if (requireHeatSource && sourceTilemap != null)
        {
            bool sourceValid = IsHeatSourceValid();
            if (isOn && !sourceValid)
            {
                TurnOffInternal(false);
            }
            else if (!isOn && sourceValid && startOn)
            {
                TurnOnInternal(false);
            }
        }

        float direction = spinClockwise ? -1f : 1f;

        if (isOn && bladeTransform != null)
        {
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, maxSpinSpeed, spinAcceleration * Time.deltaTime);
            bladeTransform.Rotate(0f, 0f, currentSpinSpeed * direction * Time.deltaTime);
        }
        else if (bladeTransform != null)
        {
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, 0f, spinDeceleration * Time.deltaTime);
            bladeTransform.Rotate(0f, 0f, currentSpinSpeed * direction * Time.deltaTime);
        }
    }

    private bool IsHeatSourceValid()
    {
        if (!requireHeatSource || sourceTilemap == null)
            return true;
        return sourceTilemap.GetCurrentTemperature() > 50f;
    }

    private void ApplyAffectedObjectsTemperature(float temp)
    {
        foreach (var thermal in affectedThermalObjects)
            if (thermal != null) thermal.SetBaseTemperature(temp);

        foreach (var tilemap in affectedTilemaps)
            if (tilemap != null) tilemap.SetTemperature(temp);
    }

    private void ApplyState()
    {
        float targetTemp = isOn ? onTemperature : offTemperature;
        ApplyAffectedObjectsTemperature(targetTemp);
    }

    public void TurnOn()
    {
        TurnOnInternal(true);
    }

    private void TurnOnInternal(bool triggerEvents)
    {
        if (isOn) return;
        if (requireHeatSource && !IsHeatSourceValid())
        {
            if (!string.IsNullOrEmpty(failMessage) && NotificationManager.Instance != null)
                NotificationManager.Instance.NotifyBottom(failMessage);

            Debug.Log($"PowerDistributor {name} cannot turn on – heat source not valid.");

            if (triggerEvents)
                onFail.Invoke();

            return;
        }
        isOn = true;
        ApplyState();
        if (triggerEvents)
            onTurnOn.Invoke();

        if (bootSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(bootSound, transform.position, volumeScale: 0.7f);
        }

        if (humSound != null && AudioManager.Instance != null && humSource != null)
        {
            AudioManager.Instance.ConfigureLoop(
                humSource,
                humSound,
                volumeScale: humVolumeScale,
                fadeInDuration: humFadeIn,
                maxDistance: 20f
            );
        }
    }

    public void TurnOff()
    {
        TurnOffInternal(true);
    }

    private void TurnOffInternal(bool triggerEvents)
    {
        if (!isOn) return;
        isOn = false;
        ApplyState();
        if (triggerEvents)
            onTurnOff.Invoke();

        if (humSource != null && humSource.isPlaying && AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOut(humSource, humFadeOut);
        }
    }

    public void Toggle()
    {
        if (isOn)
            TurnOff();
        else
            TurnOn();
    }

    public bool IsOn() => isOn;

    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isOn ? "On" : "Off" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            bool loadedOn = cs.state == "On";
            if (loadedOn != isOn)
            {
                if (loadedOn)
                    TurnOnInternal(false);
                else
                    TurnOffInternal(false);
            }
        }
        ApplyState();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
        if (humSource != null && humSource.isPlaying)
            humSource.Stop();
    }
}