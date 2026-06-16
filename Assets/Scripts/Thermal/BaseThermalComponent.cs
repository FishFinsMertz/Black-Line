using UnityEngine;
public abstract class BaseThermalComponent : MonoBehaviour, IHasThermal
{
    protected static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");

    protected void SubscribeToThermalEvents()
    {
        ThermalManager.OnThermalToggled += OnThermalToggled;
        
        // Apply initial state if manager is available
        if (ThermalManager.Instance != null)
        {
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        }
    }

    protected void UnsubscribeFromThermalEvents()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
    }

    protected abstract void OnThermalToggled(bool enabled);

    protected void EnableThermalKeyword(Material material)
    {
        if (material != null)
            material.EnableKeyword("THERMAL_ON");
    }

    protected void DisableThermalKeyword(Material material)
    {
        if (material != null)
            material.DisableKeyword("THERMAL_ON");
    }

    protected void SetThermalKeyword(Material material, bool enabled)
    {
        if (material == null) return;
        if (enabled)
            EnableThermalKeyword(material);
        else
            DisableThermalKeyword(material);
    }

    protected void SetMaterialFloat(Material material, int propertyId, float value)
    {
        if (material != null)
            material.SetFloat(propertyId, value);
    }

    protected void SetMaterialVector(Material material, int propertyId, Vector2 value)
    {
        if (material != null)
            material.SetVector(propertyId, value);
    }
}
