using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class TilemapThermal : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material thermalMaterial;

    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    [SerializeField, Range(0f, 1f)] private float fresnelPower = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightnessInfluence = 0.08f;
    [SerializeField] private Vector2 fresnelCenter = new Vector2(0.5f, 0.5f);
    [SerializeField, Range(0.1f, 5f)] private float fresnelRadius = 1f;

    private TilemapRenderer tilemapRenderer;
    private Material currentMaterial;      // reference to the instanced material (when thermal active)
    private bool isThermalOn = false;

    private static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");
    private static readonly int FresnelPowerProperty = Shader.PropertyToID("_FresnelPower");
    private static readonly int BrightnessInfluenceProperty = Shader.PropertyToID("_BrightnessInfluence");
    private static readonly int FresnelCenterProperty = Shader.PropertyToID("_FresnelCenter");
    private static readonly int FresnelRadiusProperty = Shader.PropertyToID("_FresnelRadius");

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (normalMaterial == null)
            normalMaterial = tilemapRenderer.material;
    }

    private void OnEnable()
    {
        ThermalManager.OnThermalToggled += OnThermalToggled;
        // Apply initial state
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false);
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
            // Create unique instance for this tilemap (so each tilemap can have its own temperature)
            currentMaterial = new Material(thermalMaterial);
            ApplyParametersToMaterial(currentMaterial);
            tilemapRenderer.material = currentMaterial;
            isThermalOn = true;
        }
        else
        {
            // Switch back to normal material (shared)
            tilemapRenderer.material = normalMaterial;
            isThermalOn = false;
            currentMaterial = null;
        }
    }

    private void ApplyParametersToMaterial(Material mat)
    {
        if (mat == null) return;
        mat.SetFloat(TemperatureProperty, temperature);
        mat.SetFloat(FresnelPowerProperty, fresnelPower);
        mat.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
        mat.SetVector(FresnelCenterProperty, fresnelCenter);
        mat.SetFloat(FresnelRadiusProperty, fresnelRadius);
    }

    // Public setters (similar to ThermalObject)
    public void SetTemperature(float newTemp)
    {
        temperature = Mathf.Clamp(newTemp, 0f, 100f);
        if (isThermalOn && currentMaterial != null)
            currentMaterial.SetFloat(TemperatureProperty, temperature);
    }

    public void SetFresnelPower(float power)
    {
        fresnelPower = Mathf.Clamp01(power);
        if (isThermalOn && currentMaterial != null)
            currentMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
    }

    public void SetBrightnessInfluence(float influence)
    {
        brightnessInfluence = Mathf.Clamp01(influence);
        if (isThermalOn && currentMaterial != null)
            currentMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
    }

    public void SetFresnelCenter(Vector2 center)
    {
        fresnelCenter = center;
        if (isThermalOn && currentMaterial != null)
            currentMaterial.SetVector(FresnelCenterProperty, fresnelCenter);
    }

    public void SetFresnelRadius(float radius)
    {
        fresnelRadius = Mathf.Clamp(radius, 0.1f, 5f);
        if (isThermalOn && currentMaterial != null)
            currentMaterial.SetFloat(FresnelRadiusProperty, fresnelRadius);
    }
}