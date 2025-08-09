// PlayerSpawner.cs (Unity 2022+ safe)
// Server-authoritative player spawn for NGO

using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Assign a prefab with NetworkObject on it")]
    public GameObject playerPrefab;

    Transform[] spawnPoints;

    void Awake()
    {
        // New API: FindObjectsByType instead of deprecated FindObjectsOfType(bool)
        spawnPoints = Object.FindObjectsByType<PlayerSpawnPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            )
            .Select(p => p.transform)
            .ToArray();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.OnServerStarted += HandleServerStarted;
        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy(); // safe to call
        if (NetworkManager == null) return;
        NetworkManager.OnServerStarted -= HandleServerStarted;
        NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
    }


    void HandleServerStarted()
    {
        // Host player spawns here
        TrySpawnPlayer(NetworkManager.LocalClientId);
    }

    void HandleClientConnected(ulong clientId)
    {
        // Remote clients / dedicated server clients
        TrySpawnPlayer(clientId);
    }

    void HandleClientDisconnected(ulong clientId)
    {
        if (NetworkManager.ConnectedClients.TryGetValue(clientId, out var conn) && conn.PlayerObject != null)
        {
            conn.PlayerObject.Despawn(true);
        }
    }

    void TrySpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab not assigned.");
            return;
        }

        // Avoid double-spawn if it already exists
        if (NetworkManager.ConnectedClients.TryGetValue(clientId, out var conn) && conn.PlayerObject != null)
            return;

        var (pos, rot) = GetSpawnPose(clientId);

        var playerGO = Instantiate(playerPrefab, pos, rot);
        var netObj = playerGO.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab missing NetworkObject!");
            Destroy(playerGO);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
    }

    (Vector3, Quaternion) GetSpawnPose(ulong clientId)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // simple round-robin by clientId
            var t = spawnPoints[(int)(clientId % (ulong)spawnPoints.Length)];
            return (t.position, t.rotation);
        }
        return (Vector3.up * 1.5f, Quaternion.identity);
    }
}
