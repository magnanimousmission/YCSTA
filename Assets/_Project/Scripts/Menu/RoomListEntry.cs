using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Photon.Realtime;

public class RoomListEntry : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerCountText;
    [SerializeField] Button joinButton;

    string _roomName;
    UnityAction<string> _joinCallback;

    void Reset()
    {
        roomNameText = GetComponentInChildren<TMP_Text>();
        joinButton = GetComponentInChildren<Button>();
    }

    public void SetJoinCallback(UnityAction<string> callback)
    {
        _joinCallback = callback;
    }

    public void SetRoomInfo(RoomInfo info)
    {
        _roomName = info.Name;

        if (roomNameText != null)
            roomNameText.text = info.Name;

        if (playerCountText != null)
            playerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => _joinCallback?.Invoke(_roomName));
        }
    }
}
