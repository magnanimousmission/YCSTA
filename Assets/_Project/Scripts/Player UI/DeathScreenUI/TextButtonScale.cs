using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextButtonScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField]
    private float hoverScale = 1.1f;
    [SerializeField]
    private float scaleDuration = 0.15f;
    [SerializeField]
    private Ease hoverEase = Ease.OutBack;
    [SerializeField]
    private Ease normalEase = Ease.OutQuad;

    private Vector3 _originalScale;
    private Tween _activeTween;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        _activeTween?.Kill();
        transform.localScale = _originalScale;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        ScaleTo(_originalScale * hoverScale, hoverEase);
    }

    public void OnPointerExit(PointerEventData _)
    {
        ScaleTo(_originalScale, normalEase);
    }

    private void ScaleTo(Vector3 target, Ease ease)
    {
        _activeTween?.Kill();
        _activeTween = transform
            .DOScale(target, scaleDuration)
            .SetEase(ease)
            .SetUpdate(true);
    }
}