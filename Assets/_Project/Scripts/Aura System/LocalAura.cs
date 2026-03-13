using UnityEngine;
using Photon.Pun;

public class LocalAura : MonoBehaviourPun
{
    public GameObject Aura;

    void Start()
    {
        if (!photonView.IsMine)
            Aura.SetActive(false);
    }
}