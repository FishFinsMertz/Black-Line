using UnityEngine;

public class GeneralThermalRegulator : MonoBehaviour, ISaveable
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float baseTemperature = 70f; 
    [SerializeField, Range(0f, 100f)] private float battery = 100f;
    private float currentTemperature; 

    [Header("Smoothing")]
    [SerializeField, Range(1f, 20f)] private float smoothSpeed = 5f; // units per second (temp smoothing)
    [SerializeField, Range(1f, 20f)] private float batterySmoothSpeed = 5f; // units per second (battery UI smoothing)

    [Header("Natural Drain")]
    [SerializeField, Range(0f, 10f)] private float batteryDrainRate = 0.5f; // battery lost per second (independent of temperature)

    private float currentBatterySmoothed; // for UI display (smoothly follows actual battery)

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        currentTemperature = baseTemperature;
        currentBatterySmoothed = battery;
        ApplyToAllThermalObjects();
    }

    private void Update()
    {
        // Natural battery drain (only if battery > 0)
        if (battery > 0f)
        {
            float drain = batteryDrainRate * Time.deltaTime;
            battery = Mathf.Max(0f, battery - drain);
        }

        // Smooth battery value for UI
        currentBatterySmoothed = Mathf.MoveTowards(currentBatterySmoothed, battery, batterySmoothSpeed * Time.deltaTime);

        // Smooth current temperature toward base temperature
        currentTemperature = Mathf.MoveTowards(currentTemperature, baseTemperature, smoothSpeed * Time.deltaTime);
        ApplyToAllThermalObjects();

        // Testing input
        if (Input.GetKeyDown(KeyCode.J))
            ChangeGlobalBaseTemperature(5f);
        if (Input.GetKeyDown(KeyCode.K))
            ChangeGlobalBaseTemperature(-5f);
        if (Input.GetKeyDown(KeyCode.H))
            battery = 100f; // debug: recharge instantly
    }

    public void ChangeGlobalBaseTemperature(float delta)
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
        //Debug.Log($"Current Temperature changed by {delta}. New current temp: {currentTemperature}");
        ApplyToAllThermalObjects();
    }

    private void ApplyToAllThermalObjects()
    {
        var allThermals = GetComponentsInChildren<IHasThermal>();
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
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    // Public getters
    public float GetCurrentTemperature() => currentTemperature;
    public float GetBaseTemperature() => baseTemperature;
    public float GetBatteryLevel() => battery;
    public float GetSmoothedBattery() => currentBatterySmoothed; // for UI

    public void AddBattery(float amount)
    {
        battery = Mathf.Clamp(battery + amount, 0f, 100f);
        // Smoothed value will catch up naturally in Update
    }
}