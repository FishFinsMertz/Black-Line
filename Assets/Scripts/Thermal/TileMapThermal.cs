using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class TilemapThermal : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;   // the original tilemap material (set in Awake if not assigned)
    [SerializeField] private Material thermalMaterial;  // your thermal shader material (must have same tile texture)

    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f; // uniform temperature for this tilemap

    private TilemapRenderer tilemapRenderer;

    private static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();

        // If no normal material assigned, use the current renderer material
        if (normalMaterial == null)
            normalMaterial = tilemapRenderer.material;

        // Apply initial state
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false);
    }

    private void OnEnable()
    {
        ThermalManager.OnThermalToggled += OnThermalToggled;
    }

    private void OnDisable()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
    }

    private void OnThermalToggled(bool enabled)
    {
        if (tilemapRenderer == null) return;

        if (enabled && thermalMaterial != null)
        {
            // Create a unique instance so we can change temperature per tilemap independently
            Material instancedMat = new Material(thermalMaterial);
            instancedMat.SetFloat(TemperatureProperty, temperature);
            tilemapRenderer.material = instancedMat;
        }
        else
        {
            tilemapRenderer.material = normalMaterial;
        }
    }

    // Optional: change temperature at runtime (e.g., hot floor)
    public void SetTemperature(float newTemp)
    {
        temperature = Mathf.Clamp(newTemp, 0f, 100f);
        if (tilemapRenderer != null && tilemapRenderer.material != normalMaterial)
        {
            tilemapRenderer.material.SetFloat(TemperatureProperty, temperature);
        }
    }
}