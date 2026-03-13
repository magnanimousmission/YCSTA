using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGamePlayerListManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private RectTransform playerListContent;
    [SerializeField] private GameObject playerListEntryPrefab;

    [Header("Scene Guard")]
    [SerializeField] private bool onlyRunInOnlineGameScene = true;
    [SerializeField] private string onlineGameSceneName = "04_OnlineGame";

    private readonly Dictionary<int, GameObject> _spawnedEntries = new Dictionary<int, GameObject>();

    private void Start()
    {
        if (!CanRunInCurrentScene())
            return;

        EnsureRoomLockedForMatch();
        RefreshList();
    }

    public override void OnJoinedRoom()
    {
        if (!CanRunInCurrentScene())
            return;

        EnsureRoomLockedForMatch();
        RefreshList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!CanRunInCurrentScene())
            return;

        EnsureRoomLockedForMatch();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!CanRunInCurrentScene())
            return;

        AddPlayerEntry(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!CanRunInCurrentScene())
            return;

        RemovePlayerEntry(otherPlayer.ActorNumber);
    }

    private bool CanRunInCurrentScene()
    {
        if (!onlyRunInOnlineGameScene)
            return true;

        return SceneManager.GetActiveScene().name == onlineGameSceneName;
    }

    private void RefreshList()
    {
        ClearEntries();

        if (!PhotonNetwork.InRoom)
            return;

        var players = PhotonNetwork.PlayerList;
        foreach (var player in players)
            AddPlayerEntry(player);
    }

    private void EnsureRoomLockedForMatch()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    private void AddPlayerEntry(Player player)
    {
        if (player == null || player.IsLocal || playerListContent == null || playerListEntryPrefab == null)
            return;

        if (_spawnedEntries.ContainsKey(player.ActorNumber))
            return;

        var entryObject = Instantiate(playerListEntryPrefab, playerListContent);
        var playerName = string.IsNullOrWhiteSpace(player.NickName)
            ? $"Player {player.ActorNumber}"
            : player.NickName;

        if (entryObject.TryGetComponent(out InGamePlayerListEntry entry))
        {
            entry.SetPlayerName(playerName);
        }
        else
        {
            var text = entryObject.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (text != null)
                text.text = playerName;
        }

        _spawnedEntries[player.ActorNumber] = entryObject;
    }

    private void RemovePlayerEntry(int actorNumber)
    {
        if (!_spawnedEntries.TryGetValue(actorNumber, out var entryObject))
            return;

        if (entryObject != null)
            Destroy(entryObject);

        _spawnedEntries.Remove(actorNumber);
    }

    private void ClearEntries()
    {
        foreach (var kvp in _spawnedEntries)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }

        _spawnedEntries.Clear();
    }
}
