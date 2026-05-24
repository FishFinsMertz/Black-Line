using UnityEngine;

public class GeneralThermalRegulator : MonoBehaviour, ISaveable
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float baseTemperature = 70f; // target
    private float currentTemperature; // actual smoothed temperature

    [Header("Smoothing")]
    [SerializeField, Range(1f, 20f)] private float smoothSpeed = 5f; // units per second

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        currentTemperature = baseTemperature;
        ApplyToAllThermalObjects();
    }

    public void ChangeGlobalBaseTemperature(float delta)
    {
        // Update the target base temperature
        baseTemperature = Mathf.Clamp(baseTemperature + delta, 0f, 100f);
    }

    private void Update()
    {
        // Smoothly move current temperature toward base temperature
        currentTemperature = Mathf.MoveTowards(currentTemperature, baseTemperature, smoothSpeed * Time.deltaTime);
        // Apply the smoothed temperature to all thermal objects
        ApplyToAllThermalObjects();

        // Testing input
        if (Input.GetKeyDown(KeyCode.J))
            ChangeGlobalBaseTemperature(5f);
        if (Input.GetKeyDown(KeyCode.K))
            ChangeGlobalBaseTemperature(-5f);
    }

    private void ApplyToAllThermalObjects()
    {
        var allThermals = GetComponentsInChildren<IHasThermal>();
        foreach (var thermal in allThermals)
        {
            if (thermal is ThermalObject to)
            {
                to.SetBaseTemperature(currentTemperature); // send smoothed value
            }
        }
    }

    public void Save(GameData data)
    {
        data.playerBaseTemperature = baseTemperature; // save target, not current
    }

    public void Load(GameData data)
    {
        baseTemperature = data.playerBaseTemperature;
        currentTemperature = baseTemperature; // instant set on load
        ApplyToAllThermalObjects();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public float GetCurrentTemperature() => currentTemperature;
}