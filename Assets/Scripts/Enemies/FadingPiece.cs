using UnityEngine;
using System.Collections;

public class FadingPiece : MonoBehaviour
{
    [Header("Fade Settings")]
    public float delayBeforeFade = 5f;   // time to fly before fading starts
    public float fadeDuration = 10f;       // how long the fade takes

    private SpriteRenderer sr;
    private Color startColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on {name}. Destroying in {delayBeforeFade} seconds.");
            Destroy(gameObject, delayBeforeFade);
            return;
        }

        startColor = sr.color;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // Wait before fading
        if (delayBeforeFade > 0f)
            yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}