// Assets/Scripts/OwnerEnable.cs
using UnityEngine;
using Unity.Netcode;

public class OwnerEnable : NetworkBehaviour
{
    [Header("Enable these Behaviours only for the owning client")]
    [SerializeField] Behaviour[] enableForOwner;

    [Header("Enable these GameObjects only for the owning client (e.g., camera rig)")]
    [SerializeField] GameObject[] objectsForOwner;

    public override void OnNetworkSpawn() => Apply();

    void Apply()
    {
        bool on = IsOwner;
        if (enableForOwner != null)
            foreach (var b in enableForOwner) if (b) b.enabled = on;

        if (objectsForOwner != null)
            foreach (var go in objectsForOwner) if (go) go.SetActive(on);
    }
}
