using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListEntry : MonoBehaviourPunCallbacks

{
    [Header("Name")]
    [SerializeField] TMP_Text playerNameText;

    [Header("Status")]
    [SerializeField] TMP_Text playerStatusText;

    [Header("Actions")]
    [SerializeField] GameObject kickButtonObject;

    [Header("Listing Color")]
    [SerializeField] Graphic playerColorGraphic;

    private Player _player;
    private Button _kickButton;

    private void Awake()
    {
        if (playerColorGraphic == null)
        {
            var colorTransform = transform.Find("PlayerColor");
            if (colorTransform != null)
                playerColorGraphic = colorTransform.GetComponent<Graphic>();
        }

        if (kickButtonObject == null)
        {
            var kickButtonTransform = transform.Find("KickButton");
            if (kickButtonTransform != null)
                kickButtonObject = kickButtonTransform.gameObject;
        }

        if (kickButtonObject != null)
        {
            _kickButton = kickButtonObject.GetComponent<Button>();
            if (_kickButton != null)
                _kickButton.onClick.AddListener(OnKickButtonPressed);
        }

        ApplyKickVisibility();
    }

    private void OnDestroy()
    {
        if (_kickButton != null)
            _kickButton.onClick.RemoveListener(OnKickButtonPressed);
    }

    public void SetPlayer(Player player)
    {
        SetPlayer(player, Color.white);
    }

    public void SetPlayer(Player player, Color listingColor)
    {
        _player = player;
        

        var displayName = string.IsNullOrEmpty(player.NickName)
            ? $"Player {player.ActorNumber}"
            : player.NickName;
    
        playerNameText.text = displayName;
    
        var status = player.IsLocal ? "(You)" : string.Empty;
        playerStatusText.text = status;

        if (playerColorGraphic != null)
            playerColorGraphic.color = listingColor;
    

        ApplyKickVisibility();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        ApplyKickVisibility();
    }

    private void ApplyKickVisibility()
    {
        if (kickButtonObject == null || _player == null)
            return;

        bool showKick = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && !_player.IsLocal;
        kickButtonObject.SetActive(showKick);
    }

    private void OnKickButtonPressed()
    {
        if (_player == null)
            return;

        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            return;

        if (_player.IsLocal)
            return;

        bool kicked = PhotonNetwork.CloseConnection(_player);
        if (!kicked)
        {
            Debug.LogWarning($"Failed to kick {_player.NickName}. Ensure RoomOptions.EnableCloseConnection is true when creating the room.");
        }
    }
}
