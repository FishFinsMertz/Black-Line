using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Volume))]
public class ColdEffectsController : MonoBehaviour
{
    [Header("Cold Effects")]
    [SerializeField] private Volume coldVolume;
    [SerializeField] private float maxColdWeight = 1f;
    [SerializeField] private float warmThreshold = 70f;

    [Header("Overheat Effects")]
    [SerializeField] private Volume overheatVolume;
    [SerializeField] private float maxOverheatWeight = 1f;
    [SerializeField] private float overheatThreshold = 80f;

    [Header("Smoothing")]
    [SerializeField] private float weightSmoothSpeed = 2f;

    private GeneralThermalRegulator playerThermal;
    private float smoothedColdWeight = 0f;
    private float smoothedOverheatWeight = 0f;

    private void Start()
    {
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
        playerThermal = playerObj.GetComponent<GeneralThermalRegulator>();
    }

    private void Update()
    {
        if (playerThermal == null) return;

        float temp = playerThermal.GetBaseTemperature();

        // --- Cold target weight ---
        float targetColdWeight = 0f;
        if (temp < warmThreshold)
        {
            targetColdWeight = 1f - (temp / warmThreshold);
            targetColdWeight = Mathf.Clamp01(targetColdWeight) * maxColdWeight;
        }

        // --- Overheat target weight ---
        float targetOverheatWeight = 0f;
        if (temp > overheatThreshold)
        {
            targetOverheatWeight = (temp - overheatThreshold) / (100f - overheatThreshold);
            targetOverheatWeight = Mathf.Clamp01(targetOverheatWeight) * maxOverheatWeight;
        }

        // Smooth both weights
        smoothedColdWeight = Mathf.Lerp(smoothedColdWeight, targetColdWeight, weightSmoothSpeed * Time.deltaTime);
        smoothedOverheatWeight = Mathf.Lerp(smoothedOverheatWeight, targetOverheatWeight, weightSmoothSpeed * Time.deltaTime);

        if (coldVolume != null)
            coldVolume.weight = smoothedColdWeight;
        if (overheatVolume != null)
            overheatVolume.weight = smoothedOverheatWeight;
    }
}