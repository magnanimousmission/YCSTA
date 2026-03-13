using UnityEngine;

public interface IPlayerState
{
    void Exit(PlayerCore player);
    void Enter(PlayerCore player);
    void FixedUpdate(PlayerCore player);
    string GetStateName();
}
