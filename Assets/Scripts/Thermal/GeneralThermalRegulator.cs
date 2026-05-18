using UnityEngine;

public class GeneralThermalRegulator : MonoBehaviour, ISaveable
{
    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float baseTemperature = 70f;

    private void Start()
    {
        // Register with the save system
        SaveManager.Instance?.Register(this);
        
        // Apply initial temperature to all existing thermal objects
        ApplyToAllThermalObjects();
    }

    public void ChangeGlobalBaseTemperature(float delta)
    {
        // Update the central base temperature
        baseTemperature = Mathf.Clamp(baseTemperature + delta, 0f, 100f);
        // Apply the new base temperature to all active thermal objects
        ApplyToAllThermalObjects();
    }

    private void ApplyToAllThermalObjects()
    {
        // Find all active IHasThermal components in the entire hierarchy
        var allThermals = GetComponentsInChildren<IHasThermal>();
        foreach (var thermal in allThermals)
        {
            if (thermal is ThermalObject to)
            {
                to.SetBaseTemperature(baseTemperature);
            }
        }
    }

    // Testing – modify temperature with J/K
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            ChangeGlobalBaseTemperature(10f);
        if (Input.GetKeyDown(KeyCode.K))
            ChangeGlobalBaseTemperature(-10f);
    }

    // --- ISaveable implementation ---
    public void Save(GameData data)
    {
        data.playerBaseTemperature = baseTemperature;
    }

    public void Load(GameData data)
    {
        baseTemperature = data.playerBaseTemperature;
        // Sync with all children after loading
        ApplyToAllThermalObjects();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }
}