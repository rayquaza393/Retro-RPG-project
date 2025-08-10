// Assets/Scripts/HideOnNetworkStart.cs
using UnityEngine;
using Unity.Netcode;

public class HideOnNetworkStart : MonoBehaviour
{
    [SerializeField] private GameObject[] toHide;

    private void OnEnable()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnServerStarted += HandleStarted;
            nm.OnClientStarted += HandleStarted;
        }
    }

    private void OnDisable()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnServerStarted -= HandleStarted;
            nm.OnClientStarted -= HandleStarted;
        }
    }

    private void Update()
    {
        // Safety net in case the event fires before we subscribed, or we enter play already listening
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsListening)
        {
            HandleStarted();
        }
    }

    private void HandleStarted()
    {
        foreach (var go in toHide) if (go) go.SetActive(false);
        enabled = false; // done — stop Update and unsub on disable
    }
}
