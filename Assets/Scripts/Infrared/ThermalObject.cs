using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ThermalObject : MonoBehaviour
{
    [Header("Thermal Vision Parameters")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;
    [SerializeField, Range(0f, 1f)] private float fresnelPower = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightnessInfluence = 0.08f;

    [Header("Material")]
    [SerializeField] private Material infraredMaterial; // Drag your thermal material here

    private SpriteRenderer spriteRenderer;
    private Material uniqueMaterial;

    private static readonly int TemperatureProperty = Shader.PropertyToID("_Temperature");
    private static readonly int FresnelPowerProperty = Shader.PropertyToID("_FresnelPower");
    private static readonly int BrightnessInfluenceProperty = Shader.PropertyToID("_BrightnessInfluence");

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (infraredMaterial != null)
            uniqueMaterial = new Material(infraredMaterial);
        else
            uniqueMaterial = new Material(spriteRenderer.sharedMaterial);

        spriteRenderer.material = uniqueMaterial;
        ApplyParameters();
    }

    void Update()
    {
        // Continuously apply in case values change via inspector or external scripts
        ApplyParameters();
    }

    // Optional: call this from inspector or other scripts to force an update
    public void ApplyParameters()
    {
        if (uniqueMaterial == null) return;

        float normalizedTemp = temperature / 100f;
        uniqueMaterial.SetFloat(TemperatureProperty, normalizedTemp);
        uniqueMaterial.SetFloat(FresnelPowerProperty, fresnelPower);
        uniqueMaterial.SetFloat(BrightnessInfluenceProperty, brightnessInfluence);
    }

    // Public setters (optional – useful for gameplay events)
    public void SetTemperature(float newTemp)
    {
        temperature = Mathf.Clamp(newTemp, 0f, 100f);
    }

    public void SetFresnelPower(float newPower)
    {
        fresnelPower = Mathf.Clamp01(newPower);
    }

    public void SetBrightnessInfluence(float newInfluence)
    {
        brightnessInfluence = Mathf.Clamp01(newInfluence);
    }
}