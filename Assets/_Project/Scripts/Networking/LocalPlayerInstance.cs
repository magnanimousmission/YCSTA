using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
public class LocalPlayerInstance : MonoBehaviourPun
{
    public static GameObject Instance { get; private set; }

    void Awake()
    {
        if (!photonView.IsMine)
        {
            DisableRemoteViewComponents();
            return;
        }

        if (Instance == null)
        {
            Instance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("LocalPlayerInstance already exists. Destroying duplicate.", gameObject);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == gameObject)
            Instance = null;
    }

    private void DisableRemoteViewComponents()
    {
        var root = transform.root;

        var cam = root.GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        var listener = root.GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = false;
    }
}
