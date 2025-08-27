using UnityEngine;
using Sfs2X.Entities;

public class RemotePlayerSpawner : MonoBehaviour
{
    [Header("Assign your remote player prefab here")]
    public GameObject remotePlayerPrefab;

    public GameObject SpawnRemotePlayer(User user, Vector3 startPosition, Quaternion startRotation)
    {
        if (remotePlayerPrefab == null)
        {
            Debug.LogError("[RemotePlayerSpawner] Remote player prefab not assigned.");
            return null;
        }

        // Instantiate the remote player
        GameObject go = Instantiate(remotePlayerPrefab, startPosition, startRotation);

        // Ensure RemoteEntity is attached
        RemoteEntity re = go.GetComponent<RemoteEntity>();
        if (!re)
        {
            Debug.LogWarning("[RemotePlayerSpawner] RemoteEntity not found on prefab. Adding dynamically.");
            re = go.AddComponent<RemoteEntity>();
        }

        // Optionally: assign player ID/display name to RemoteEntity here if needed

        // Register with central registry (if you're using one)
        RemoteEntityRegistry.Register(user.Id, re);

        return go;
    }

    public void DespawnRemotePlayer(User user)
    {
        RemoteEntityRegistry.Unregister(user.Id);
        // Optionally destroy the GameObject too if you’re tracking it
    }
}
