using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;


public class RoomManager : MonoBehaviourPunCallbacks
{
    const string kRoomPropGameStarting = "GameStarting";
    const string kRoomPropGameStartMessage = "GameStartMessage";

    [Header("UI References")]
    [SerializeField] RectTransform playerListContent;
    [SerializeField] GameObject playerListEntryPrefab;

    [Header("Player Listing Colors")]
    [SerializeField] Color[] playerListingColors =
    {
        new Color(0.93f, 0.33f, 0.31f),
        new Color(0.27f, 0.68f, 0.92f),
        new Color(0.31f, 0.82f, 0.48f),
        new Color(0.97f, 0.75f, 0.25f),
    };

    readonly Dictionary<int, GameObject> _playerEntries = new Dictionary<int, GameObject>();
    readonly Dictionary<int, int> _playerColorIndexByActor = new Dictionary<int, int>();
    bool _hasShownStartLoading;

    void Start()
    {
        PhotonNetwork.EnableCloseConnection = true;

        RefreshPlayerList();
        LoadingScreen.Hide();

        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(kRoomPropGameStarting, out var startingObj) &&
            startingObj is bool isStarting &&
            isStarting)
        {
            string message = "Starting game...";
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(kRoomPropGameStartMessage, out var messageObj) &&
                messageObj is string customMessage &&
                !string.IsNullOrWhiteSpace(customMessage))
            {
                message = customMessage;
            }

            ShowStartLoading(message);
        }
    }

    void ShowStartLoading(string message)
    {
        if (_hasShownStartLoading)
            return;

        _hasShownStartLoading = true;
        LoadingScreen.Show(message);
    }

    void RefreshPlayerList()
    {
        foreach (var kvp in _playerEntries)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }

        _playerEntries.Clear();
        _playerColorIndexByActor.Clear();

        if (playerListContent == null || playerListEntryPrefab == null)
            return;

        var players = PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            AddPlayerEntry(player);
        }
    }

    void AddPlayerEntry(Player player)
    {
        if (playerListContent == null || playerListEntryPrefab == null) return;

        var listingColor = GetOrAssignListingColor(player.ActorNumber);

        var go = Instantiate(playerListEntryPrefab, playerListContent);
        if (go.TryGetComponent(out PlayerListEntry entry))
        {
            entry.SetPlayer(player, listingColor);
        }

        _playerEntries[player.ActorNumber] = go;
    }

    Color GetOrAssignListingColor(int actorNumber)
    {
        if (playerListingColors == null || playerListingColors.Length == 0)
            return Color.white;

        if (_playerColorIndexByActor.TryGetValue(actorNumber, out int existingColorIndex))
            return playerListingColors[existingColorIndex];

        int colorPoolSize = Mathf.Min(4, playerListingColors.Length);
        if (colorPoolSize <= 0)
            return Color.white;

        var availableIndices = new List<int>(colorPoolSize);
        for (int i = 0; i < colorPoolSize; i++)
        {
            if (!_playerColorIndexByActor.ContainsValue(i))
                availableIndices.Add(i);
        }

        if (availableIndices.Count == 0)
            return Color.white;

        int randomIndex = Random.Range(0, availableIndices.Count);
        int assignedColorIndex = availableIndices[randomIndex];
        _playerColorIndexByActor[actorNumber] = assignedColorIndex;
        return playerListingColors[assignedColorIndex];
    }

    void RemovePlayerEntry(int actorNumber)
    {
        _playerColorIndexByActor.Remove(actorNumber);

        if (_playerEntries.TryGetValue(actorNumber, out var go))
        {
            Destroy(go);
            _playerEntries.Remove(actorNumber);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerJoinRoomSound);
        AddPlayerEntry(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerLeaveRoomSound);
        RemovePlayerEntry(otherPlayer.ActorNumber);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene("01_Lobby");
            return;
        }
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        LoadingScreen.Show("Leaving room...");
        PhotonNetwork.LeaveRoom();
    }

    public void StartOnlineGame()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            var roomStartProps = new PhotonHashtable
            {
                { kRoomPropGameStarting, true },
                { kRoomPropGameStartMessage, "Starting game..." }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomStartProps);
        }

        StartCoroutine(StartGameWithSound());
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null)
            return;

        if (!propertiesThatChanged.TryGetValue(kRoomPropGameStarting, out var startingObj))
            return;

        if (startingObj is not bool isStarting || !isStarting)
            return;

        string message = "Starting game...";
        if (propertiesThatChanged.TryGetValue(kRoomPropGameStartMessage, out var messageObj) &&
            messageObj is string changedMessage &&
            !string.IsNullOrWhiteSpace(changedMessage))
        {
            message = changedMessage;
        }
        else if (PhotonNetwork.CurrentRoom != null &&
                 PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(kRoomPropGameStartMessage, out var roomMessageObj) &&
                 roomMessageObj is string roomMessage &&
                 !string.IsNullOrWhiteSpace(roomMessage))
        {
            message = roomMessage;
        }

        ShowStartLoading(message);
    }

    private IEnumerator StartGameWithSound()
    {
        LoadingScreen.Show("Waiting for players...");
        AudioManager.Instance?.StopBackgroundAudio(0f);
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.StartGameSound);

        float clipLength = AudioManager.Instance?.GetSfxClipLength(AudioManager.SfxClip.StartGameSound) ?? 0f;
        if (clipLength > 0f)
            yield return new WaitForSeconds(clipLength);

        PhotonNetwork.LoadLevel("04_OnlineGame");
    }

    public override void OnLeftRoom()
    {
        LoadingScreen.Show("Loading lobby...");

        var asyncOp = SceneManager.LoadSceneAsync("01_Lobby");
        if (asyncOp != null)
        {
            asyncOp.completed += _ => LoadingScreen.Hide();
            return;
        }
        
        SceneManager.LoadScene("01_Lobby");
        LoadingScreen.Hide();
    }
}
