using UnityEngine;

[AddComponentMenu("Player/Single Player Spawner")]
public class SinglePlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Behavior")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool preventDuplicatePlayer = true;

    void Start()
    {
        if (spawnOnStart)
            TrySpawnPlayer();
    }

    public void TrySpawnPlayer()
    {
        if (playerPrefab == null)
            return;

        if (preventDuplicatePlayer && FindObjectOfType<PlayerCore>() != null)
        {
            LoadingScreen.Hide();
            return;
        }

        Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        LoadingScreen.Hide();
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var index = Random.Range(0, spawnPoints.Length);
            return spawnPoints[index].position;
        }

        return Vector3.zero;
    }
}
