using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] RectTransform roomListContent;
    [SerializeField] GameObject roomListEntryPrefab;
    [SerializeField] GameObject noRoomsText;

    [Header("Create Room")]
    [SerializeField] TMP_InputField createRoomNameInput;
    [SerializeField] byte createRoomMaxPlayers = 4;

    bool _isLeavingToMainMenu;

    //Room information
    string _pendingRoomName;
    byte _pendingMaxPlayers;
    bool _hasPendingCreateRoom;

    //Keep track of instantiated room list entries so we can update/remove them
    readonly Dictionary<string, GameObject> _roomListEntries = new Dictionary<string, GameObject>();

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            LoadingScreen.Show("Joining lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (_isLeavingToMainMenu)
        {
            PhotonNetwork.Disconnect();
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
            return;

        if (_hasPendingCreateRoom)
        {
            PhotonNetwork.JoinLobby();
            return;
        }

        if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
            PhotonNetwork.JoinLobby();
    }

    public override void OnLeftLobby()
    {
        if (_isLeavingToMainMenu)
            PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"OnDisconnected: {cause}");

        if (_isLeavingToMainMenu)
        {
            _isLeavingToMainMenu = false;
            SceneManager.LoadScene("00_MainMenu");
        }
    }

    public void OnBackPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        _isLeavingToMainMenu = true;

        if (!PhotonNetwork.IsConnected)
        {
            _isLeavingToMainMenu = false;
            SceneManager.LoadScene("00_MainMenu");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            return;
        }

        PhotonNetwork.Disconnect();
    }

    public override void OnJoinedLobby()
    {
        LoadingScreen.Hide();
        UpdateNoRoomsText();

        if (_hasPendingCreateRoom)
        {
            CreateRoom(_pendingRoomName, _pendingMaxPlayers);
            _hasPendingCreateRoom = false;
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList)
                RemoveRoomFromList(roomInfo.Name);
            else
                AddOrUpdateRoomListEntry(roomInfo);
        }

        UpdateNoRoomsText();
    }

    void AddOrUpdateRoomListEntry(RoomInfo info)
    {
        if (_roomListEntries.TryGetValue(info.Name, out var existingEntry))
        {
            if (existingEntry.TryGetComponent(out RoomListEntry entry))
                entry.SetRoomInfo(info);

            return;
        }

        if (roomListEntryPrefab == null || roomListContent == null)
            return;

        var room = Instantiate(roomListEntryPrefab, roomListContent);
        if (room.TryGetComponent(out RoomListEntry entryComponent))
        {
            entryComponent.SetRoomInfo(info);
            entryComponent.SetJoinCallback(JoinRoom);
        }

        _roomListEntries[info.Name] = room;
    }

    void RemoveRoomFromList(string roomName)
    {
        if (_roomListEntries.TryGetValue(roomName, out var room))
        {
            Destroy(room);
            _roomListEntries.Remove(roomName);
        }
    }

    void UpdateNoRoomsText()
    {
        if (noRoomsText == null) return;
        noRoomsText.SetActive(_roomListEntries.Count == 0);
    }

    public void JoinRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return;

        LoadingScreen.Show($"Joining room {roomName}...");
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnCreateRoomPressed()
    {
        if (createRoomNameInput == null)
            return;

        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        var roomName = createRoomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
            roomName = $"Room_{Random.Range(1000, 9999)}";

        LoadingScreen.Show($"Creating room {roomName}...");
        EnsureLobbyAndCreateRoom(roomName, createRoomMaxPlayers);
    }

    void EnsureLobbyAndCreateRoom(string roomName, byte maxPlayers)
    {
        _pendingRoomName = roomName;
        _pendingMaxPlayers = maxPlayers;
        _hasPendingCreateRoom = true;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerJoinRoomSound);
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
            return;

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            return;
        }

        CreateRoom(roomName, maxPlayers);
    }

    public override void OnLeftRoom()
    {
        if (_hasPendingCreateRoom)
            PhotonNetwork.JoinLobby();
    }

    public void CreateRoom(string roomName, byte maxPlayers = 4)
    {
        if (string.IsNullOrEmpty(roomName)) return;

        var options = new RoomOptions
        {
            MaxPlayers = maxPlayers
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnJoinedRoom()
    {
        LoadingScreen.Show("Loading room...");
        var asyncOp = SceneManager.LoadSceneAsync("02_Room");
        asyncOp.completed += _ => LoadingScreen.Hide();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LoadingScreen.Hide();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        LoadingScreen.Hide();
    }
}

