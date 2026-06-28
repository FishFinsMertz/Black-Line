using UnityEngine;

public class GeneralThermalRegulator : MonoBehaviour, ISaveable, ITemperatureChangeable
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float baseTemperature = 70f; 
    [SerializeField, Range(0f, 100f)] private float battery = 100f;
    private float currentTemperature; 
    private float initialBaseTemperature;

    [Header("Smoothing")]
    [SerializeField, Range(1f, 20f)] private float smoothSpeed = 5f;
    [SerializeField, Range(1f, 20f)] private float batterySmoothSpeed = 5f;

    [Header("Natural Drain & Drift")]
    [SerializeField, Range(0f, 10f)] private float batteryDrainRate = 0.5f;
    [SerializeField, Range(0.1f, 10f)] private float baseTemperatureDriftSpeed = 1f;

    [Header("Critical State (Battery = 0)")]
    [SerializeField, Range(0f, 10f)] private float criticalDriftSpeed = 2f;
    [SerializeField] private bool criticalDriftEnabled = true;                

    private float currentBatterySmoothed;

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        initialBaseTemperature = baseTemperature;
        currentTemperature = baseTemperature;
        currentBatterySmoothed = battery;
        ApplyToAllThermalObjects();
    }

    private void Update()
    {
        // Natural battery drain
        if (battery > 0f)
        {
            float drain = batteryDrainRate * Time.deltaTime;
            battery = Mathf.Max(0f, battery - drain);
        }

        currentBatterySmoothed = Mathf.MoveTowards(currentBatterySmoothed, battery, batterySmoothSpeed * Time.deltaTime);

        // Temperature drift logic
        if (battery > 0f)
        {
            // Normal drift: return to initial temperature
            if (!Mathf.Approximately(baseTemperature, initialBaseTemperature))
            {
                float step = baseTemperatureDriftSpeed * Time.deltaTime;
                baseTemperature = Mathf.MoveTowards(baseTemperature, initialBaseTemperature, step);
            }
        }
        else if (criticalDriftEnabled && battery <= 0f)
        {
            // Critical drift: move towards 0°C (freezing) when battery empty
            if (!Mathf.Approximately(baseTemperature, 0f))
            {
                float step = criticalDriftSpeed * Time.deltaTime;
                baseTemperature = Mathf.MoveTowards(baseTemperature, 0f, step);
            }
        }

        currentTemperature = Mathf.MoveTowards(currentTemperature, baseTemperature, smoothSpeed * Time.deltaTime);
        ApplyToAllThermalObjects();

        // Testing input – battery changes
        if (Input.GetKeyDown(KeyCode.J))
            AddBattery(20f);
        if (Input.GetKeyDown(KeyCode.K))
            AddBattery(-20f);
        if (Input.GetKeyDown(KeyCode.H))
            battery = 100f;
    }

    public void ChangeBaseTemperature(float delta) // Takes from battery first
    {
        if (delta == 0) return;

        float remaining = delta;
        if (battery > 0)
        {
            float absorb = Mathf.Min(battery, Mathf.Abs(delta));
            battery -= absorb;
            remaining = delta - (delta > 0 ? absorb : -absorb);
        }

        if (Mathf.Abs(remaining) > 0.01f)
        {
            baseTemperature = Mathf.Clamp(baseTemperature + remaining, 0f, 100f);
        }
    }

    public void ChangeGlobalCurrentTemperature(float delta)
    {
        currentTemperature = Mathf.Clamp(currentTemperature + delta, 0f, 100f);
        ApplyToAllThermalObjects();
    }

    private void ApplyToAllThermalObjects()
    {
        var allThermals = GetComponentsInChildren<ITemperatureChangeable>();
        foreach (var thermal in allThermals)
        {
            if (thermal is ThermalObject to)
            {
                to.SetBaseTemperature(currentTemperature);
            }
        }
    }

    public void Save(GameData data)
    {
        data.playerBaseTemperature = baseTemperature;
        data.playerBatteryAmt = battery;
    }

    public void Load(GameData data)
    {
        baseTemperature = data.playerBaseTemperature;
        battery = data.playerBatteryAmt;
        currentBatterySmoothed = battery;
        currentTemperature = baseTemperature;
        ApplyToAllThermalObjects();
        initialBaseTemperature = baseTemperature;
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public float GetCurrentTemperature() => currentTemperature;
    public float GetBaseTemperature() => baseTemperature;
    public float GetBatteryLevel() => battery;
    public float GetSmoothedBattery() => currentBatterySmoothed;

    public void AddBattery(float amount)
    {
        battery = Mathf.Clamp(battery + amount, 0f, 100f);
    }
}