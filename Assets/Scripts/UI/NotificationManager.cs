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

    [Header("Item Notification")]
    [SerializeField] private CanvasGroup itemNotificationGroup;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;

    [SerializeField] private float itemNotificationDuration = 4f;

    private CanvasGroup bottomCanvasGroup;
    private Coroutine bottomFadeCoroutine;
    private Coroutine itemFadeCoroutine;

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

        float duration = item.displayDuration > 0 ? item.displayDuration : itemNotificationDuration;
        itemFadeCoroutine = StartCoroutine(FadeSequence(itemNotificationGroup, duration, () =>
        {
            itemFadeCoroutine = null;
            itemNotificationGroup.gameObject.SetActive(false);
        }));
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