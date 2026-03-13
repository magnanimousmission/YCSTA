using UnityEngine;
 
public class ComputerAccess : MonoBehaviour
{
    [SerializeField] private string objectiveId = "computer_access_01";
 
    private bool _isActive;
    private bool _hasAccessed;
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
        if (!_isActive || _hasAccessed) return;
 
        PlayerCore player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;
 
        _hasAccessed = true;
        Debug.Log("Computer accessed");
        ObjectiveManager.Instance.CompleteMainObjective();
    }
}