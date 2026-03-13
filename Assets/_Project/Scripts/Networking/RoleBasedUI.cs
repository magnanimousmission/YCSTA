using Photon.Pun;
using UnityEngine;

public class RoleBasedUI : MonoBehaviourPunCallbacks
{
    public enum VisibilityMode
    {
        Everyone,
        HostOnly,
        NonHostOnly,
        HideForEveryone
    }

    public VisibilityMode visibility = VisibilityMode.Everyone;

    public GameObject target;

    private void Awake()
    {
        if (target == null)
            target = gameObject;
        UpdateVisibility();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        UpdateVisibility();
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool isHost = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;
        bool shouldShow = false;
        switch (visibility)
        {
            case VisibilityMode.Everyone:
                shouldShow = true;
                break;
            case VisibilityMode.HostOnly:
                shouldShow = isHost;
                break;
            case VisibilityMode.NonHostOnly:
                shouldShow = !isHost;
                break;
            case VisibilityMode.HideForEveryone:
                shouldShow = false;
                break;
        }
        if (target != null)
            target.SetActive(shouldShow);
    }
}
