using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Linq;

public class Lights : MonoBehaviour, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("Light References")]
    [SerializeField] private Light2D[] lights;

    [Header("Pulse Settings")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 1.0f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float phaseOffset = 0f;

    [Header("On/Off")]
    [SerializeField] private bool startOn = true;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float turnOnDelay = 0f;
    [SerializeField] private float turnOffDelay = 0f;

    private float currentMultiplier = 1f;
    private float targetMultiplier = 1f;
    private bool isOn;
    private Coroutine pendingTransition;

    private void Start()
    {
        if (lights == null || lights.Length == 0)
        {
            lights = GetComponentsInChildren<Light2D>();
        }

        // Register with save system
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);

        // Apply default state (will be overridden by Load if a save exists)
        isOn = startOn;
        currentMultiplier = startOn ? 1f : 0f;
        targetMultiplier = startOn ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        currentMultiplier = Mathf.MoveTowards(currentMultiplier, targetMultiplier, transitionSpeed * Time.deltaTime);

        float baseIntensity;
        if (enablePulse)
        {
            float t = Time.time * pulseSpeed * Mathf.PI * 2f + phaseOffset * Mathf.Deg2Rad;
            baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(t) + 1f) * 0.5f);
        }
        else
        {
            baseIntensity = maxIntensity;
        }

        float finalIntensity = baseIntensity * currentMultiplier;

        foreach (Light2D light in lights)
        {
            if (light != null)
            {
                light.intensity = finalIntensity;
            }
        }
    }

    // --- Public API ---
    public void TurnOn()
    {
        if (pendingTransition != null)
            StopCoroutine(pendingTransition);
        pendingTransition = StartCoroutine(DelayedTurnOn());
    }

    private IEnumerator DelayedTurnOn()
    {
        if (turnOnDelay > 0f)
            yield return new WaitForSeconds(turnOnDelay);
        isOn = true;
        targetMultiplier = 1f;
        pendingTransition = null;
    }

    public void TurnOff()
    {
        if (pendingTransition != null)
            StopCoroutine(pendingTransition);
        pendingTransition = StartCoroutine(DelayedTurnOff());
    }

    private IEnumerator DelayedTurnOff()
    {
        if (turnOffDelay > 0f)
            yield return new WaitForSeconds(turnOffDelay);
        isOn = false;
        targetMultiplier = 0f;
        pendingTransition = null;
    }

    public void Toggle()
    {
        if (isOn)
            TurnOff();
        else
            TurnOn();
    }

    public bool IsOn() => isOn;

    // --- ISaveable ---
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
            if (pendingTransition != null)
            {
                StopCoroutine(pendingTransition);
                pendingTransition = null;
            }
            isOn = loadedOn;
            currentMultiplier = loadedOn ? 1f : 0f;
            targetMultiplier = loadedOn ? 1f : 0f;
        }
    }
}