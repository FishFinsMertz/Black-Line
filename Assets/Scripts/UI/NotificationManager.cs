using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI bottomText;
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup bottomCanvasGroup;
    private Coroutine bottomFadeCoroutine;

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
    }

    public void NotifyBottom(string msg, float duration)
    {
        if (bottomText == null || bottomCanvasGroup == null) return;

        if (bottomFadeCoroutine != null)
            StopCoroutine(bottomFadeCoroutine);

        bottomText.text = msg;
        bottomText.gameObject.SetActive(true);
        bottomCanvasGroup.alpha = 0f;

        bottomFadeCoroutine = StartCoroutine(FadeSequence(bottomText, bottomCanvasGroup, duration, () =>
        {
            bottomFadeCoroutine = null;
        }));
    }

    private IEnumerator FadeSequence(TextMeshProUGUI targetText, CanvasGroup targetGroup, float displayDuration, System.Action onComplete)
    {
        // Fade in
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
        targetText.gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}