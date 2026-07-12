using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLifeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image batteryBarImage;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private Image outlineImage;

    [Header("Recharger UI")]
    [SerializeField] private CanvasGroup rechargerGroup;
    [SerializeField] private TextMeshProUGUI rechargerCountText;
    [SerializeField] private string rechargerID = "BatteryRecharger";

    [Header("Body Colors")]
    [SerializeField] private Color batteryColor = Color.white;
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private Color coldColor = Color.blue;
    [SerializeField] private Color hotColor = Color.red;

    [Header("Transition Speed")]
    [SerializeField] private float colorTransitionSpeed = 5f;

    [Header("Outline Flash")]
    [SerializeField] private float flashThreshold = 20f;
    [SerializeField] private float flashSpeed = 2f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color flashNormalColor = Color.white;

    [Header("Damage Flash")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.2f;

    private GeneralThermalRegulator thermal;
    private Inventory inventory;
    private float flashTimer = 0f;
    private Coroutine damageFlashCoroutine;

    private void Awake()
    {
        thermal = FindFirstObjectByType<GeneralThermalRegulator>();
        inventory = FindFirstObjectByType<Inventory>();
        if (thermal == null)
            Debug.LogWarning("PlayerLifeUI: No GeneralThermalRegulator found.");
        if (inventory == null)
            Debug.LogWarning("PlayerLifeUI: No Inventory found.");
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerDamaged += FlashDamage;
        if (inventory != null)
        {
            inventory.OnConsumablesChanged += UpdateRechargerUI;
            inventory.OnConsumableUsed += OnConsumableUsed;
            UpdateRechargerUI();
        }
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDamaged -= FlashDamage;
        if (inventory != null)
        {
            inventory.OnConsumablesChanged -= UpdateRechargerUI;
            inventory.OnConsumableUsed -= OnConsumableUsed;
        }
    }

    private void Start()
    {
        if (bodyImage != null)
            bodyImage.color = batteryColor;
        if (outlineImage != null)
            outlineImage.color = flashNormalColor;
    }

    private void Update()
    {
        if (thermal == null) return;

        float batteryRaw = thermal.GetBatteryLevel();
        float batteryPercent = batteryRaw / 100f;
        float temperature = thermal.GetCurrentTemperature();

        if (batteryBarImage != null)
            batteryBarImage.fillAmount = batteryPercent;

        if (batteryText != null)
            batteryText.text = Mathf.Round(batteryRaw).ToString() + "%";

        Color targetColor = batteryColor;
        if (batteryRaw <= 0.5f)
            targetColor = GetTemperatureColor(temperature);

        if (bodyImage != null)
            bodyImage.color = Color.Lerp(bodyImage.color, targetColor, colorTransitionSpeed * Time.deltaTime);

        if (outlineImage != null)
        {
            if (batteryRaw <= flashThreshold)
            {
                flashTimer += Time.deltaTime * flashSpeed;
                float t = (Mathf.Sin(flashTimer * Mathf.PI * 2f) + 1f) * 0.5f;
                if (damageFlashCoroutine == null)
                    outlineImage.color = Color.Lerp(flashNormalColor, flashColor, t);
            }
            else
            {
                outlineImage.color = flashNormalColor;
                flashTimer = 0f;
            }
        }
    }

    private void OnConsumableUsed(string id, int amount)
    {
        if (id == rechargerID)
            UpdateRechargerUI();
    }

    private void UpdateRechargerUI()
    {
        if (inventory == null) return;
        int count = inventory.GetConsumableCount(rechargerID);
        bool hasAny = count > 0;

        if (rechargerGroup != null)
        {
            rechargerGroup.alpha = hasAny ? 1f : 0f;
            rechargerGroup.interactable = hasAny;
            rechargerGroup.blocksRaycasts = hasAny;
        }

        if (rechargerCountText != null)
            rechargerCountText.text = $"x{count}";
    }

    private Color GetTemperatureColor(float temperature)
    {
        if (temperature <= 50f)
        {
            float t = Mathf.InverseLerp(50f, 0f, temperature);
            return Color.Lerp(neutralColor, coldColor, t);
        }
        else
        {
            float t = Mathf.InverseLerp(50f, 100f, temperature);
            return Color.Lerp(neutralColor, hotColor, t);
        }
    }

    private void FlashDamage()
    {
        FlashDamage(damageFlashColor, damageFlashDuration);
    }

    private void FlashDamage(Color flashColor, float duration)
    {
        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlashRoutine(flashColor, duration));
    }

    private System.Collections.IEnumerator DamageFlashRoutine(Color flashColor, float duration)
    {
        Color origBody = bodyImage != null ? bodyImage.color : Color.white;
        Color origOutline = outlineImage != null ? outlineImage.color : Color.white;

        if (bodyImage != null) bodyImage.color = flashColor;
        if (outlineImage != null) outlineImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (bodyImage != null)
                bodyImage.color = Color.Lerp(flashColor, origBody, t);
            if (outlineImage != null)
                outlineImage.color = Color.Lerp(flashColor, origOutline, t);
            yield return null;
        }

        if (bodyImage != null) bodyImage.color = origBody;
        if (outlineImage != null) outlineImage.color = origOutline;
        damageFlashCoroutine = null;
    }
}