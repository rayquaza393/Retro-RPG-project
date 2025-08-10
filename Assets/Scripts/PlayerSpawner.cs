// Assets/Scripts/PlayerSpawner.cs
using System.Linq;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
[DefaultExecutionOrder(-5000)]
public class PlayerSpawner : NetworkBehaviour
{
    [Header("Assign a prefab with NetworkObject")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points (use PlayerSpawnPoint component)")]
    [SerializeField] private bool includeInactiveSpawnPoints = true;

    [Header("Fallback")]
    [SerializeField] private Vector3 fallbackPosition = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 fallbackEuler = Vector3.zero;

    [Header("Logs")]
    [SerializeField] private bool verboseLogs = true;

    private Transform[] _spawnPoints;

    private void Awake()
    {
        var found = Object.FindObjectsByType<PlayerSpawnPoint>(
            includeInactiveSpawnPoints ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        _spawnPoints = found.Select(p => p.transform).ToArray();
        if (verboseLogs) Debug.Log($"[PlayerSpawner] Found spawn points: {_spawnPoints.Length}");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var nm = NetworkManager.Singleton;
        if (!nm)
        {
            Debug.LogError("[PlayerSpawner] No NetworkManager.Singleton.");
            return;
        }
        if (!playerPrefab)
        {
            Debug.LogError("[PlayerSpawner] Player prefab not assigned.");
            return;
        }
        if (!playerPrefab.GetComponent<NetworkObject>())
        {
            Debug.LogError("[PlayerSpawner] Player prefab missing NetworkObject.");
            return;
        }
        // FIXED: check entries safely
        bool registered = nm.NetworkConfig.Prefabs != null &&
                          nm.NetworkConfig.Prefabs.Prefabs != null &&
                          nm.NetworkConfig.Prefabs.Prefabs.Any(e => e != null && e.Prefab == playerPrefab);
        if (!registered)
        {
            Debug.LogError("[PlayerSpawner] Player prefab NOT in NetworkManager → NetworkPrefabs.");
            return;
        }

        nm.OnClientConnectedCallback += HandleClientConnected;
        nm.OnClientDisconnectCallback += HandleClientDisconnected;

        // Spawn for clients already connected (host included)
        foreach (var c in nm.ConnectedClientsList)
            TrySpawnPlayer(c.ClientId);

        if (verboseLogs) Debug.Log("[PlayerSpawner] OnNetworkSpawn complete.");
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnClientConnectedCallback -= HandleClientConnected;
            nm.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId) => TrySpawnPlayer(clientId);

    private void HandleClientDisconnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (nm != null &&
            nm.ConnectedClients.TryGetValue(clientId, out var conn) &&
            conn.PlayerObject != null)
        {
            if (verboseLogs) Debug.Log($"[PlayerSpawner] Despawning player for {clientId}");
            conn.PlayerObject.Despawn(true);
        }
    }

    private void TrySpawnPlayer(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (nm.ConnectedClients.TryGetValue(clientId, out var conn) && conn.PlayerObject != null)
        {
            if (verboseLogs) Debug.Log($"[PlayerSpawner] Client {clientId} already has PlayerObject.");
            return;
        }

        var (pos, rot) = GetSpawnPose(clientId);
        var go = Instantiate(playerPrefab, pos, rot);
        var netObj = go.GetComponent<NetworkObject>();
        if (verboseLogs) Debug.Log($"[PlayerSpawner] Spawning player {clientId} at {pos}");
        netObj.SpawnAsPlayerObject(clientId, destroyWithScene: true);
    }

    private (Vector3, Quaternion) GetSpawnPose(ulong clientId)
    {
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            var t = _spawnPoints[(int)(clientId % (ulong)_spawnPoints.Length)];
            return (t.position, t.rotation);
        }
        return (fallbackPosition, Quaternion.Euler(fallbackEuler));
    }
}
