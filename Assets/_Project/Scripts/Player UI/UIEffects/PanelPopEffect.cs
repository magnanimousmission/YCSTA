using DG.Tweening;
using UnityEngine;

public class PanelPopEffect : MonoBehaviour
{
    [Header("Pop Settings")]
    [SerializeField] private float punchScale   = 0.25f;
    [SerializeField] private float duration     = 0.4f;
    [SerializeField] private int   vibrato      = 5;
    [SerializeField] private float elasticity   = 0.5f;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        _rect.DOKill();
        _rect.localScale = Vector3.one;

        _rect.DOPunchScale(Vector3.one * punchScale, duration, vibrato, elasticity)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        _rect.DOKill();
        _rect.localScale = Vector3.one;
    }
}