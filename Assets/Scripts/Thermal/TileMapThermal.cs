using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class TileMapThermal : BaseThermalComponent
{
    [Header("Thermal Material")]
    [SerializeField] private Material thermalMaterial; // Your thermal shader material

    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    [SerializeField, Range(0f, 1f)] private float fresnelPower = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightnessInfluence = 0.08f;
    [SerializeField] private Vector2 fresnelCenter = new Vector2(0.5f, 0.5f);
    [SerializeField, Range(0f, 5f)] private float fresnelRadius = 1f;

    private TilemapRenderer tilemapRenderer;
    private Material instanceMaterial;
    private bool isThermalOn = false;

    private static readonly int FresnelPowerProperty = Shader.PropertyToID("_FresnelPower");
    private static readonly int BrightnessInfluenceProperty = Shader.PropertyToID("_BrightnessInfluence");
    private static readonly int FresnelCenterProperty = Shader.PropertyToID("_FresnelCenter");
    private static readonly int FresnelRadiusProperty = Shader.PropertyToID("_FresnelRadius");

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (thermalMaterial == null)
        {
            Debug.LogError("TileMapThermal: No thermal material assigned!", this);
            enabled = false;
            return;
        }
        instanceMaterial = new Material(thermalMaterial);
        DisableThermalKeyword(instanceMaterial);
        tilemapRenderer.material = instanceMaterial;
    }

    private void OnEnable()
    {
        SubscribeToThermalEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromThermalEvents();
    }

    protected override void OnThermalToggled(bool enabled)
    {
        if (instanceMaterial == null) return;
        isThermalOn = enabled;
        SetThermalKeyword(instanceMaterial, enabled);
        if (enabled)
        {
            ApplyParameters();
        }
    }

    private void ApplyParameters()
    {
        if (instanceMaterial == null) return;
        instanceMaterial.SetFloat(TemperatureProperty, temperature);
        instanceMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
        instanceMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
        instanceMaterial.SetVector(FresnelCenterProperty, fresnelCenter);
        instanceMaterial.SetFloat(FresnelRadiusProperty, fresnelRadius);
    }

    public void SetTemperature(float newTemp)
    {
        temperature = Mathf.Clamp(newTemp, 0f, 100f);
        if (isThermalOn && instanceMaterial != null)
            instanceMaterial.SetFloat(TemperatureProperty, temperature);
    }

    public void SetFresnelPower(float power)
    {
        fresnelPower = Mathf.Clamp01(power);
        if (isThermalOn && instanceMaterial != null)
            instanceMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
    }

    public void SetBrightnessInfluence(float influence)
    {
        brightnessInfluence = Mathf.Clamp01(influence);
        if (isThermalOn && instanceMaterial != null)
            instanceMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
    }

    public void SetFresnelCenter(Vector2 center)
    {
        fresnelCenter = center;
        if (isThermalOn && instanceMaterial != null)
            instanceMaterial.SetVector(FresnelCenterProperty, fresnelCenter);
    }

    public void SetFresnelRadius(float radius)
    {
        fresnelRadius = Mathf.Clamp(radius, 0f, 5f);
        if (isThermalOn && instanceMaterial != null)
            instanceMaterial.SetFloat(FresnelRadiusProperty, fresnelRadius);
    }
}