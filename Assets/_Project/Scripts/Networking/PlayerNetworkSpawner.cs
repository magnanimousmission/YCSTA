using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

[AddComponentMenu("Networking/Player Network Spawner")]
public class PlayerNetworkSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Intro Gate")]
    [SerializeField] private bool waitForIntro = true;
    [SerializeField] private CameraOrbitLerp introOrbit;

    [Header("UI")]
    [SerializeField] private GameObject[] uiGameObjects;

    private Coroutine _spawnRoutine;
    private GameObject[] _resolvedUiGameObjects;

    void Start()
    {
        BeginSpawnFlow();
    }

    public override void OnJoinedRoom()
    {
        BeginSpawnFlow();
    }

    void BeginSpawnFlow()
    {
        if (_spawnRoutine != null)
            return;

        _spawnRoutine = StartCoroutine(SpawnFlowRoutine());
    }

    IEnumerator SpawnFlowRoutine()
    {
        SetIntroUiActive(false);

        if (waitForIntro)
        {
            LoadingScreen.Hide();

            if (introOrbit == null)
                introOrbit = FindObjectOfType<CameraOrbitLerp>();

            if (introOrbit != null && introOrbit.UsesTimedStop)
            {
                while (!introOrbit.HasStopped)
                    yield return null;
            }
        }

        var localPlayerReady = TrySpawnLocalPlayer();
        if (localPlayerReady)
        {
            SetIntroUiActive(true);

            if (introOrbit != null)
                introOrbit.DisableOrbit();
        }

        _spawnRoutine = null;
    }

    bool TrySpawnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            return false;
        }

        if (playerPrefab == null)
        {
            return false;
        }

        if (LocalPlayerInstance.Instance != null)
        {
            LoadingScreen.Hide();
            return true;
        }

        var spawnPosition = GetSpawnPosition(PhotonNetwork.LocalPlayer);
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        LoadingScreen.Hide();
        return true;
    }

    void SetIntroUiActive(bool isActive)
    {
        var targets = ResolveUiTargets();
        for (var i = 0; i < targets.Length; i++)
        {
            var uiObject = targets[i];
            if (uiObject != null)
                uiObject.SetActive(isActive);
        }
    }

    GameObject[] ResolveUiTargets()
    {
        if (_resolvedUiGameObjects != null && _resolvedUiGameObjects.Length > 0)
            return _resolvedUiGameObjects;

        if (uiGameObjects != null && uiGameObjects.Length > 0)
        {
            _resolvedUiGameObjects = uiGameObjects;
            return _resolvedUiGameObjects;
        }

        // Fallback for scenes where the serialized UI list is empty in builds.
        var playerUiCanvas = GameObject.Find("PlayerUICanvas");
        if (playerUiCanvas != null)
            _resolvedUiGameObjects = new[] { playerUiCanvas };
        else
            _resolvedUiGameObjects = new GameObject[0];

        return _resolvedUiGameObjects;
    }

    Vector3 GetSpawnPosition(Player player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var index = (player.ActorNumber - 1) % spawnPoints.Length;
            if (index < 0) index = 0;
            return spawnPoints[index].position;
        }

        return Vector3.zero;
    }
}
