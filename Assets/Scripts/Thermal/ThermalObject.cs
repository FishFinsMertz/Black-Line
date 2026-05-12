using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ThermalObject : MonoBehaviour
{
    [Header("Thermal Vision Parameters")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    [SerializeField, Range(0f, 1f)] private float fresnelPower = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightnessInfluence = 0.08f;

    [Header("Temperature Pulse (Beating Heart)")]
    [SerializeField] private bool enablePulse = false;
    [SerializeField, Range(0f, 5f)] private float pulseSpeed = 1f;
    [SerializeField, Range(0f, 100f)] private float pulseAmplitude = 20f;
    [SerializeField] private bool useRandomPhase = true;

    [Header("Material")]
    [SerializeField] private Material infraredMaterial;

    private SpriteRenderer spriteRenderer;
    private Material uniqueMaterial;
    private float phaseOffset = 0f;

    private static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");
    private static readonly int FresnelPowerProperty = Shader.PropertyToID("_FresnelPower");
    private static readonly int BrightnessInfluenceProperty = Shader.PropertyToID("_BrightnessInfluence");

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (infraredMaterial != null)
            uniqueMaterial = new Material(infraredMaterial);
        else
            uniqueMaterial = new Material(spriteRenderer.sharedMaterial);

        spriteRenderer.material = uniqueMaterial;

        uniqueMaterial.DisableKeyword("THERMAL_ON");

        if (useRandomPhase)
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        ApplyParameters();
        // Force initial keyword state based on ThermalManager
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
        else
            OnThermalToggled(false); 
    }

    void Update()
    {
        ApplyParameters();
    }

    void OnEnable()
    {
        ThermalManager.OnThermalToggled += OnThermalToggled;
        // Immediate sync in case ThermalManager already exists
        if (ThermalManager.Instance != null)
            OnThermalToggled(ThermalManager.Instance.IsThermalEnabled());
    }

    void OnDisable()
    {
        ThermalManager.OnThermalToggled -= OnThermalToggled;
    }

    public void ApplyParameters()
    {
        if (uniqueMaterial == null) return;

        float currentTemp = temperature;
        if (enablePulse)
        {
            float pulse = Mathf.Sin((Time.time + phaseOffset) * pulseSpeed * Mathf.PI * 2f);
            float variation = pulse * pulseAmplitude;
            currentTemp = Mathf.Clamp(temperature + variation, 0f, 100f);
        }

        uniqueMaterial.SetFloat(TemperatureProperty, currentTemp);
        uniqueMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
        uniqueMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
    }

    public void SetTemperature(float newTemp) => temperature = Mathf.Clamp(newTemp, 0f, 100f);
    public void SetFresnelPower(float newPower) => fresnelPower = Mathf.Clamp01(newPower);
    public void SetBrightnessInfluence(float newInfluence) => brightnessInfluence = Mathf.Clamp01(newInfluence);
    public void SetPulse(bool enabled, float speed = 1f, float amplitude = 20f)
    {
        enablePulse = enabled;
        pulseSpeed = speed;
        pulseAmplitude = amplitude;
    }

    private void OnThermalToggled(bool enabled)
    {
        if (uniqueMaterial == null) return;
        if (enabled)
            uniqueMaterial.EnableKeyword("THERMAL_ON");
        else
            uniqueMaterial.DisableKeyword("THERMAL_ON");
    }
}