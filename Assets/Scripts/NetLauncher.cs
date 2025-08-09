using Unity.Netcode;
using UnityEngine;

public class NetLauncher : MonoBehaviour
{
    public void StartHost()
    {
        Debug.Log("Starting as Host...");
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Debug.Log("Starting as Client...");
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        Debug.Log("Starting Dedicated Server...");
        NetworkManager.Singleton.StartServer();
    }
}
