using UnityEngine;

public class TurnOnGenerator : MonoBehaviour
{
    [SerializeField] private string objectiveId = "turn_on_generator_01";
 
    private bool _isActive;
    private bool _hasActivated;
    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
    }
    
    private void OnEnable()
    {
        ObjectiveManager.OnSubObjectiveActivated += HandleSubObjectiveActivated;
    }
 
    private void OnDisable()
    {
        ObjectiveManager.OnSubObjectiveActivated -= HandleSubObjectiveActivated;
    }
 
    private void HandleSubObjectiveActivated(string id)
    {
        _isActive = id == objectiveId;
        if (_isActive)
        {
            _meshRenderer.enabled = true;
        }
        else
        {
            _meshRenderer.enabled = false;
        }
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || _hasActivated) return;
 
        PlayerCore player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;
 
        _hasActivated = true;
        Debug.Log("Turned on generator");
        ObjectiveManager.Instance.CompleteSubObjective();
    }
}
