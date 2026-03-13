using UnityEngine;
using TMPro;
using Photon.Pun;

public class OxygenUIController : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 5f;

    private TextMeshProUGUI _text;
    private float _displayValue;
    private float _targetValue;
    private PlayerCore _playerCore;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        TryBindLocalPlayer();
    }

    private void OnDisable()
    {
        Unbind();
    }
    
    public void Bind(PlayerCore playerCore)
    {
        Unbind();
        _playerCore = playerCore;

        var oxygen = _playerCore.GetPlayerData().oxygen;
        _displayValue = oxygen;
        _targetValue = oxygen;

        _text.text = $"Oxygen: {Mathf.RoundToInt(oxygen)}";

        _playerCore.OnOxygenChanged += SetTarget;
    }

    private void Unbind()
    {
        if (_playerCore == null) return;
        _playerCore.OnOxygenChanged -= SetTarget;
        _playerCore = null;
    }

    private void TryBindLocalPlayer()
    {
        foreach (var core in FindObjectsByType<PlayerCore>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var photonView = core.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                Bind(core);
                return;
            }
        }
    }

    private void Update()
    {
        if (Mathf.Approximately(_displayValue, _targetValue)) return;

        _displayValue = Mathf.Lerp(_displayValue, _targetValue, Time.deltaTime * lerpSpeed);

        if (Mathf.Abs(_displayValue - _targetValue) < 0.5f)
            _displayValue = _targetValue;

        _text.text = $"Oxygen: {Mathf.RoundToInt(_displayValue)}";
    }

    private void SetTarget(float current, float max)
    {
        _targetValue = current;
    }
}