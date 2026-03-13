using UnityEngine;

public class PlantBomb : MonoBehaviour
{
    [SerializeField] private string objectiveId = "plant_bomb_01";
 
    private bool _isActive;
    private bool _hasPlanted;
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
        if (!_isActive || _hasPlanted) return;
 
        PlayerCore player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;
 
        _hasPlanted = true;
        Debug.Log("Planted Bomb");
        ObjectiveManager.Instance.CompleteMainObjective();
    }
    
}