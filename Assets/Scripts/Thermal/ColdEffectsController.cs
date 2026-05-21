using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Volume))]
public class ColdEffectsController : MonoBehaviour
{
    [Header("Cold Effects")]
    [SerializeField] private Volume coldVolume;                 // Assign in Inspector (the existing cold volume)
    [SerializeField] private float maxColdWeight = 1f;
    [SerializeField] private float minColdWeight = 0f;
    [SerializeField] private float warmThreshold = 70f;

    [Header("Overheat Effects")]
    [SerializeField] private Volume overheatVolume;             // Assign your OverheatEffectPPr Volume here
    [SerializeField] private float maxOverheatWeight = 1f;
    [SerializeField] private float overheatThreshold = 80f;     // Above this temperature, overheat starts

    private ThermalObject playerThermal;

    private void Start()
    {
        // Ensure both volumes are assigned (fallback to GetComponent for coldVolume if needed)
        if (coldVolume == null)
            coldVolume = GetComponent<Volume>();

        FindPlayerThermal();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerThermal();
    }

    private void FindPlayerThermal()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("No GameObject with tag 'Player' found. Temperature effects won't work.");
            playerThermal = null;
            return;
        }

        Transform bodyTransform = playerObj.transform.Find("Body");
        if (bodyTransform != null)
            playerThermal = bodyTransform.GetComponent<ThermalObject>();
        else
            playerThermal = playerObj.GetComponent<ThermalObject>();
    }

    private void Update()
    {
        if (playerThermal == null) return;

        float temp = playerThermal.GetTemperature();

        // --- Cold effects (below warmThreshold) ---
        float coldWeight = 0f;
        if (temp < warmThreshold)
        {
            coldWeight = 1f - (temp / warmThreshold);
            coldWeight = Mathf.Clamp01(coldWeight) * maxColdWeight;
        }
        if (coldVolume != null)
            coldVolume.weight = coldWeight;

        // --- Overheat effects (above overheatThreshold) ---
        float overheatWeight = 0f;
        if (temp > overheatThreshold)
        {
            overheatWeight = (temp - overheatThreshold) / (100f - overheatThreshold);
            overheatWeight = Mathf.Clamp01(overheatWeight) * maxOverheatWeight;
        }
        if (overheatVolume != null)
            overheatVolume.weight = overheatWeight;
    }
}