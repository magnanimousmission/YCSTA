using UnityEngine;
using UnityEngine.InputSystem;

public class ConsumablePickup : MonoBehaviour
{
    [SerializeField] private ConsumableData data;

    private PlayerCore _playerInRange;

    void OnTriggerEnter(Collider other)
    {
        PlayerCore player = other.GetComponentInChildren<PlayerCore>();
        if (player == null) return;

        _playerInRange = player;
    }

    void OnTriggerExit(Collider other)
    {
        _playerInRange = null;
    }

    void Update()
    {
        if (_playerInRange == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ApplyEffect(_playerInRange);
            Destroy(transform.parent.gameObject);
        }
    }

    private void ApplyEffect(PlayerCore player)
    {
        switch (data.type)
        {
            case ConsumableType.Health:
                //player.SetPlayerHealth(player.GetPlayerHealth() + data.amount);
                break;
            case ConsumableType.Energy:
                //player.SetPlayerEnergy(player.GetPlayerEnergy() + data.amount);
                break;
        }
    }
}