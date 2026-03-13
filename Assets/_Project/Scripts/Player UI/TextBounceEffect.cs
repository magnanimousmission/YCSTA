using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextBounceEffect : MonoBehaviour
{
    private TextMeshProUGUI _targetText;

    [Header("Pulse Settings")]
    [SerializeField] private float peakScale = 1.2f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private int pulseCount = 3;
    
    private readonly Ease _easeType = Ease.InOutSine;

    private Vector3 _originalScale;

    private void Awake()
    {
        _targetText = GetComponent<TextMeshProUGUI>();
        _originalScale = _targetText.transform.localScale;

        Play();
    }

    private void Play()
    {
        _targetText.transform.localScale = _originalScale;

        _targetText.transform
            .DOScale(_originalScale * peakScale, duration * 0.5f)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        _targetText.transform.DOKill();
    }
}