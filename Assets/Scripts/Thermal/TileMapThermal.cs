using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class TileMapThermal : BaseThermalComponent
{
    [Header("Thermal Material")]
    [SerializeField] private Material thermalMaterial;

    [Header("Thermal Settings")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    private float currentTemperature;
    [SerializeField, Range(0.5f, 50f)] private float temperatureLerpSpeed = 5f;
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
            enabled = false;
            return;
        }
        instanceMaterial = new Material(thermalMaterial);
        DisableThermalKeyword(instanceMaterial);
        tilemapRenderer.material = instanceMaterial;
        currentTemperature = temperature;
    }

    private void Update()
    {
        currentTemperature = Mathf.MoveTowards(currentTemperature, temperature, temperatureLerpSpeed * Time.deltaTime);
        ApplyParameters();
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
        instanceMaterial.SetFloat(TemperatureProperty, currentTemperature);
        instanceMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
        instanceMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
        instanceMaterial.SetVector(FresnelCenterProperty, fresnelCenter);
        instanceMaterial.SetFloat(FresnelRadiusProperty, fresnelRadius);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && isThermalOn && instanceMaterial != null)
        {
            ApplyParameters();
        }
    }
#endif

    public void SetTemperature(float newTemp)
    {
        temperature = Mathf.Clamp(newTemp, 0f, 100f);
    }

    public float GetCurrentTemperature() => currentTemperature;

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