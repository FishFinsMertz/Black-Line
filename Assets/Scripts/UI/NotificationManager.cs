using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Simple Text Notification")]
    [SerializeField] private TextMeshProUGUI bottomText;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float textNotificationDuration = 4f;

    [Header("Key Item Notification")]
    [SerializeField] private CanvasGroup itemNotificationGroup;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private float itemNotificationDuration = 4f;
    [SerializeField] private AudioClip keyItemNotificationSound;

    [Header("Top Notification")]
    [SerializeField] private TextMeshProUGUI topText;
    [SerializeField] private Color goodColor = Color.green;
    [SerializeField] private Color badColor = Color.red;
    [SerializeField] private float flashCycleDuration = 0.5f;
    [SerializeField] private int defaultFlashCycles = 6;

    [Header("Location Notification")]
    [SerializeField] private AudioClip locationNotificationSound;
    [SerializeField] private float locationNotificationDelay = 1f;
    [SerializeField] private CanvasGroup locationNotificationGroup;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI temperatureText;

    private CanvasGroup bottomCanvasGroup;
    private Coroutine bottomFadeCoroutine;
    private Coroutine itemFadeCoroutine;
    private Coroutine topFlashCoroutine;
    private Coroutine locationFadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bottomText != null)
        {
            bottomCanvasGroup = bottomText.GetComponent<CanvasGroup>();
            if (bottomCanvasGroup == null)
                bottomCanvasGroup = bottomText.gameObject.AddComponent<CanvasGroup>();
            bottomCanvasGroup.alpha = 0f;
            bottomText.gameObject.SetActive(false);
        }

        if (itemNotificationGroup != null)
        {
            itemNotificationGroup.alpha = 0f;
            itemNotificationGroup.gameObject.SetActive(false);
        }

        if (topText != null)
        {
            topText.gameObject.SetActive(false);
            topText.alpha = 0f;
        }

        if (locationText != null && temperatureText != null)
        {
            locationText.gameObject.SetActive(false);
            locationText.alpha = 0f;
            temperatureText.gameObject.SetActive(false);
            temperatureText.alpha = 0f;
        }
    }

    public void NotifyLocation(string location, string temperature)
    {
        if (locationText == null || temperatureText == null) return;

        if (locationFadeCoroutine != null)
            StopCoroutine(locationFadeCoroutine);

        locationFadeCoroutine = StartCoroutine(DelayedLocationNotification(location, temperature));
    }

    private IEnumerator DelayedLocationNotification(string location, string temperature)
    {
        yield return new WaitForSeconds(locationNotificationDelay);

        AudioManager.Instance?.PlayOneShot2D(keyItemNotificationSound, null, 1f);

        locationText.text = location;
        temperatureText.text = $"Ambient Temperature: {temperature}C";
        locationText.gameObject.SetActive(true);
        temperatureText.gameObject.SetActive(true);
        locationText.alpha = 1f;
        temperatureText.alpha = 1f;

        yield return FadeSequence(locationNotificationGroup, textNotificationDuration, () =>
        {
            locationText.gameObject.SetActive(false);
            temperatureText.gameObject.SetActive(false);
            locationFadeCoroutine = null;
        });
    }

    public void NotifyBottom(string msg)
    {
        if (bottomText == null || bottomCanvasGroup == null) return;

        if (bottomFadeCoroutine != null)
            StopCoroutine(bottomFadeCoroutine);

        bottomText.text = msg;
        bottomText.gameObject.SetActive(true);
        bottomCanvasGroup.alpha = 0f;

        bottomFadeCoroutine = StartCoroutine(FadeSequence(bottomCanvasGroup, textNotificationDuration, () =>
        {
            bottomFadeCoroutine = null;
            bottomText.gameObject.SetActive(false);
        }));
    }

    public void ShowItemNotification(ItemData item)
    {
        if (item == null || itemNotificationGroup == null) return;

        if (itemFadeCoroutine != null)
            StopCoroutine(itemFadeCoroutine);

        if (itemNameText != null)
            itemNameText.text = item.itemName;
        if (itemIconImage != null)
            itemIconImage.sprite = item.itemIcon;

        itemNotificationGroup.gameObject.SetActive(true);
        itemNotificationGroup.alpha = 0f;

        if (keyItemNotificationSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot2D(keyItemNotificationSound, null, 1f);
        }

        float duration = item.displayDuration > 0 ? item.displayDuration : itemNotificationDuration;
        itemFadeCoroutine = StartCoroutine(FadeSequence(itemNotificationGroup, duration, () =>
        {
            itemFadeCoroutine = null;
            itemNotificationGroup.gameObject.SetActive(false);
        }));
    }

    public void NotifyTop(string message, bool isGood = true, int cycles = -1)
    {
        if (topText == null) return;

        if (topFlashCoroutine != null)
            StopCoroutine(topFlashCoroutine);

        topText.text = message;
        topText.color = isGood ? goodColor : badColor;
        topText.gameObject.SetActive(true);
        topText.alpha = 1f;

        int cycleCount = cycles > 0 ? cycles : defaultFlashCycles;
        topFlashCoroutine = StartCoroutine(FlashTop(cycleCount));
    }

    private IEnumerator FlashTop(int totalCycles)
    {
        float halfCycle = flashCycleDuration / 2f;

        for (int i = 0; i < totalCycles; i++)
        {
            float elapsed = 0f;
            while (elapsed < halfCycle)
            {
                elapsed += Time.deltaTime;
                topText.alpha = Mathf.Lerp(0f, 1f, elapsed / halfCycle);
                yield return null;
            }
            topText.alpha = 1f;

            elapsed = 0f;
            while (elapsed < halfCycle)
            {
                elapsed += Time.deltaTime;
                topText.alpha = Mathf.Lerp(1f, 0f, elapsed / halfCycle);
                yield return null;
            }
            topText.alpha = 0f;
        }

        topText.gameObject.SetActive(false);
        topFlashCoroutine = null;
    }

    private IEnumerator FadeSequence(CanvasGroup targetGroup, float displayDuration, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            targetGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        targetGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            targetGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        targetGroup.alpha = 0f;

        onComplete?.Invoke();
    }
}