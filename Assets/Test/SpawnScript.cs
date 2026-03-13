using Photon.Pun;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject newPLayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero), null);
        PlayerCore core = newPLayer.GetComponentInChildren<PlayerCore>();
        core.gameObject.GetComponentInParent<LookAtMouse>().enabled = true;
        //Camera.main.enabled = true;
        core.SetIsLocal(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
