using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    public float duration = 1f;
    public float delay = 0f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        _canvasGroup.alpha = 0f;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }
}