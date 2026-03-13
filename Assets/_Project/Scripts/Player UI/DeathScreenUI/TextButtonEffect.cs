using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Colors")]
    public Color normalColor   = new Color(1f,    1f,    1f,    1f); // white
    public Color hoverColor    = new Color(0.75f, 0.75f, 0.75f, 1f); // light grey
    public Color pressedColor  = new Color(0.5f,  0.5f,  0.5f, 1f); // darker grey

    [Header("Fade")]
    [Tooltip("Seconds to transition between states.")]
    public float fadeDuration = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Played once when the cursor enters the text.")]
    [SerializeField] private AudioClip hoverClip;
    [Tooltip("Optional click sound.")]
    [SerializeField] private AudioClip clickClip;

    private TextMeshProUGUI _text;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        FadeTo(hoverColor);
        PlayClip(hoverClip);
    }

    public void OnPointerExit(PointerEventData _)
    {
        FadeTo(normalColor);
    }

    public void OnPointerDown(PointerEventData _)
    {
        FadeTo(pressedColor);
        PlayClip(clickClip);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FadeTo(eventData.pointerCurrentRaycast.gameObject == gameObject
            ? hoverColor
            : normalColor);
    }
    
    private void FadeTo(Color target)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(Color target)
    {
        Color start   = _text.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed      += Time.unscaledDeltaTime;
            _text.color   = Color.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        _text.color = target;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}