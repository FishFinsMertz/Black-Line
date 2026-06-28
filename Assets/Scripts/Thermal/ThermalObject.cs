using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ThermalObject : BaseThermalComponent
// Thermal objects for individual sprites
{
    [Header("Thermal Vision Parameters")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    [SerializeField] private float currentTemperature;
    [SerializeField, Range(0f, 1f)] private float fresnelPower = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightnessInfluence = 0.08f;
    [SerializeField] private Vector2 fresnelCenter = new Vector2(0.5f, 0.5f);
    [SerializeField, Range(0f, 5f)] private float fresnelRadius = 1f;
    [SerializeField, Range(0.5f, 20f)] private float temperatureLerpSpeed = 5f; 

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

    private static readonly int FresnelPowerProperty = Shader.PropertyToID("_FresnelPower");
    private static readonly int BrightnessInfluenceProperty = Shader.PropertyToID("_BrightnessInfluence");
    private static readonly int FresnelCenterProperty = Shader.PropertyToID("_FresnelCenter");
    private static readonly int FresnelRadiusProperty = Shader.PropertyToID("_FresnelRadius");

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

        currentTemperature = temperature;
    }

    void Start()
    {
        ApplyParameters();
    }

    void Update()
    {
        currentTemperature = Mathf.MoveTowards(currentTemperature, temperature, temperatureLerpSpeed * Time.deltaTime);
        ApplyParameters();
    }

    void OnEnable()
    {
        SubscribeToThermalEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromThermalEvents();
    }

    public void ApplyParameters()
    {
        if (uniqueMaterial == null) return;

        float finalTemp = currentTemperature;
        if (enablePulse)
        {
            float pulse = Mathf.Sin((Time.time + phaseOffset) * pulseSpeed * Mathf.PI * 2f);
            float variation = pulse * pulseAmplitude;
            finalTemp = Mathf.Clamp(finalTemp + variation, 0f, 100f);
        }

        uniqueMaterial.SetFloat(TemperatureProperty, finalTemp);
        uniqueMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
        uniqueMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
        uniqueMaterial.SetVector(FresnelCenterProperty, fresnelCenter);
        uniqueMaterial.SetFloat(FresnelRadiusProperty, fresnelRadius); 
    }

    // Public API
    public void SetBaseTemperature(float newTarget) => temperature = Mathf.Clamp(newTarget, 0f, 100f);
    public void ChangeBaseTemperature(float delta) => temperature = Mathf.Clamp(temperature + delta, 0f, 100f);
    public void SetCurrentTemperature(float newTarget) => currentTemperature = Mathf.Clamp(newTarget, 0f, 100f);
    public void ChangeCurrentTemperature(float delta) => currentTemperature = Mathf.Clamp(currentTemperature + delta, 0f, 100f);
    public float GetTemperature() => temperature;
    public float GetCurrentTemperature() => currentTemperature;
    public void SetTemperatureLerpSpeed(float speed) => temperatureLerpSpeed = Mathf.Max(0.1f, speed);
    public void SetFresnelPower(float newPower) => fresnelPower = Mathf.Clamp01(newPower);
    public void SetBrightnessInfluence(float newInfluence) => brightnessInfluence = Mathf.Clamp01(newInfluence);
    public void SetFresnelCenter(Vector2 center) => fresnelCenter = center;
    public void SetFresnelRadius(float radius) => fresnelRadius = Mathf.Clamp(radius, 0f, 5f);

    public void SetPulse(bool enabled, float speed = 1f, float amplitude = 20f)
    {
        enablePulse = enabled;
        pulseSpeed = speed;
        pulseAmplitude = amplitude;
    }

    protected override void OnThermalToggled(bool enabled)
    {
        SetThermalKeyword(uniqueMaterial, enabled);
    }
}