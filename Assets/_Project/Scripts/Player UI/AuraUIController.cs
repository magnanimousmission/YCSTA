using Photon.Pun;
using TMPro;
using UnityEngine;

public class AuraUIController : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 5f;

    private TextMeshProUGUI _text;
    private float _displayValue;
    private float _targetValue;
    private AuraController _auraController;

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
    
    public void Bind(AuraController auraController)
    {
        Unbind();
        _auraController = auraController;
        _auraController.OnAuraChanged += SetTarget;
        _auraController.RequestCurrentAura();
    }
    private void Unbind()
    {
        if (_auraController == null) return;
        _auraController.OnAuraChanged  -= SetTarget;
        _auraController = null;
    }

    private void TryBindLocalPlayer()
    {
        foreach (var controller in FindObjectsByType<AuraController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var photonView = controller.GetComponentInParent<PhotonView>();
            
            bool isLocal = photonView == null || photonView.IsMine;
        
            if (isLocal && controller.OwnerType == AuraOwnerType.Player)
            {
                Bind(controller);
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

        _text.text = $"Aura: {Mathf.RoundToInt(_displayValue)}";
    }

    void SetTarget(float aura)
    {
        _targetValue = aura;
        _text.text = $"Aura: {Mathf.RoundToInt(_targetValue)}";
    }
}