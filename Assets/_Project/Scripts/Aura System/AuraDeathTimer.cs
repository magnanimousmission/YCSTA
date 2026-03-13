using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class AuraDeathTimer : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    private AuraController _auraController;
    private Coroutine _timerCoroutine;

    public event Action<float> OnTimerTick;
    public event Action OnTimerExpired;
    public event Action OnTimerCancelled;

    public float Duration => duration;

    private void Awake()
    {
        _auraController = GetComponent<AuraController>();

        if (_auraController.OwnerType == AuraOwnerType.Player)
        {
            var photonView = GetComponentInParent<PhotonView>();
            if (photonView != null && !photonView.IsMine) return;

            FindFirstObjectByType<AuraDeathSliderUI>(FindObjectsInactive.Include)?.Bind(this);
            FindFirstObjectByType<DeathScreenController>(FindObjectsInactive.Include)?.Bind(this);
        }
    }

    private void OnEnable()
    {
        _auraController.OnAuraChanged += HandleAuraChanged;
        _auraController.RequestCurrentAura();
    }
    
    private void OnDisable() => _auraController.OnAuraChanged -= HandleAuraChanged;

    private void HandleAuraChanged(float currentAura)
    {
        if (_auraController.OwnerType != AuraOwnerType.Player)
            return;

        if (currentAura <= 0f && _timerCoroutine == null)
            _timerCoroutine = StartCoroutine(Countdown());
        else if (currentAura > 0f && _timerCoroutine != null)
            CancelTimer();
    }

    private void CancelTimer()
    {
        StopCoroutine(_timerCoroutine);
        _timerCoroutine = null;
        OnTimerCancelled?.Invoke();
    }

    private IEnumerator Countdown()
    {
        var remaining = duration;

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            OnTimerTick?.Invoke(Mathf.Max(0f, remaining));
            yield return null;
        }

        _timerCoroutine = null;
        OnTimerExpired?.Invoke();
        gameObject.SetActive(false);
    }
}