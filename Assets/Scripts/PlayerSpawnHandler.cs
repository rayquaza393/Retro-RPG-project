using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // server decides spawn; clients sync from server

        var spawnGo = GameObject.Find("PlayerSpawn");
        if (spawnGo == null)
        {
            Debug.LogWarning("[Spawn] No 'PlayerSpawn' found in scene.");
            return;
        }

        var target = spawnGo.transform;

        if (TryGetComponent<CharacterController>(out var cc))
        {
            bool wasEnabled = cc.enabled;
            if (wasEnabled) cc.enabled = false;
            transform.SetPositionAndRotation(target.position, target.rotation);
            if (wasEnabled) cc.enabled = true;
        }
        else
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        Debug.Log($"[Spawn] Placed player at {target.position}");
    }
}
