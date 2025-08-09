// Bootloader.cs
// One-stop launcher for Host/Client/Server with scene handoff and safe persistence.
// Unity 2022+ API (FindObjectsByType); no deprecated calls.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class Bootloader : MonoBehaviour
{
    [Header("Network")]
    [Tooltip("Optional explicit reference; falls back to NetworkManager.Singleton if null.")]
    public NetworkManager networkManager;

    [Header("Transport (optional)")]
    public TMPro.TMP_InputField addressTMP;
    public TMPro.TMP_InputField portTMP;
    public UnityEngine.UI.InputField addressInput; // legacy UI fallback
    public UnityEngine.UI.InputField portInput;    // legacy UI fallback

    [Header("Player (optional)")]
    public TMPro.TMP_InputField playerNameTMP;

    [Header("Scenes")]
    [Tooltip("Scene to load after starting Host/Server.")]
    public string gameplayScene = "Game";
    [Tooltip("Auto-load gameplay scene after StartHost?")]
    public bool autoLoadOnHost = true;
    [Tooltip("Auto-load gameplay scene after StartServer?")]
    public bool autoLoadOnServer = true;

    void Awake()
    {
        // Resolve NM
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("[Bootloader] No NetworkManager present. Add one to the scene.");
            return;
        }

        // Persist NetworkManager safely (singleton guard)
        var allNM = Object.FindObjectsByType<NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allNM.Length > 1 && networkManager != null)
        {
            // If there are multiple, keep the first created one alive; destroy newcomers from this scene.
            if (networkManager.gameObject.scene == gameObject.scene)
            {
                Debug.LogWarning("[Bootloader] Duplicate NetworkManager in this scene. Destroying this one.");
                Destroy(networkManager.gameObject);
            }
        }
        else
        {
            DontDestroyOnLoad(networkManager.gameObject);
        }

        // Do NOT persist the EventSystem; keep one per scene.
        // (Intentionally no DontDestroyOnLoad on UI/EventSystem)

        // Optional: basic log of EventSystems to help track dupes while you iterate
#if UNITY_EDITOR
        var allES = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allES.Length > 1)
            Debug.LogWarning($"[Bootloader] Multiple EventSystems detected in this scene: {allES.Length}");
#endif
    }

    // ---------- Button hooks ----------

    public void StartHost()
    {
        if (!EnsureNM()) return;
        ApplyTransportSettings();
        if (!networkManager.IsServer && !networkManager.IsClient)
        {
            if (networkManager.StartHost())
            {
                Debug.Log("[Bootloader] Host started.");
                if (autoLoadOnHost) LoadGameplay();
            }
            else Debug.LogError("[Bootloader] StartHost failed.");
        }
    }

    public void StartClient()
    {
        if (!EnsureNM()) return;
        ApplyTransportSettings();
        if (!networkManager.IsClient && !networkManager.IsServer)
        {
            if (networkManager.StartClient())
                Debug.Log("[Bootloader] Client connecting...");
            else
                Debug.LogError("[Bootloader] StartClient failed.");
        }
    }

    public void StartServer()
    {
        if (!EnsureNM()) return;
        ApplyTransportSettings();
        if (!networkManager.IsServer && !networkManager.IsClient)
        {
            if (networkManager.StartServer())
            {
                Debug.Log("[Bootloader] Server started.");
                if (autoLoadOnServer) LoadGameplay();
            }
            else Debug.LogError("[Bootloader] StartServer failed.");
        }
    }

    public void StopNetwork()
    {
        if (!EnsureNM()) return;
        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
            Debug.Log("[Bootloader] Network shutdown.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void LoadGameplay()
    {
        if (string.IsNullOrEmpty(gameplayScene))
        {
            Debug.LogWarning("[Bootloader] gameplayScene not set.");
            return;
        }
        SceneManager.LoadScene(gameplayScene);
    }

    // ---------- Optional input field relays ----------

    public void SetAddressFromField(string addr) => SetAddress(addr);
    public void SetPortFromField(string portStr)
    {
        if (ushort.TryParse(portStr, out var p)) SetPort(p);
    }
    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            PlayerPrefs.SetString("player_name", name);
    }

    // ---------- Transport helpers ----------

    void ApplyTransportSettings()
    {
        var utp = GetTransport();
        if (utp == null) return;

        string addr = ReadAddress();
        ushort port = ReadPort();

        utp.SetConnectionData(addr, port);
        // Note: If using Unity Relay later, this will change.
    }

    void SetAddress(string address)
    {
        var utp = GetTransport();
        if (utp != null && !string.IsNullOrWhiteSpace(address))
            utp.SetConnectionData(address, ReadPort());
    }

    void SetPort(ushort port)
    {
        var utp = GetTransport();
        if (utp != null)
            utp.SetConnectionData(ReadAddress(), port);
    }

    UnityTransport GetTransport()
    {
        if (!EnsureNM()) return null;
        var utp = networkManager.GetComponent<UnityTransport>();
        if (utp == null)
            Debug.LogWarning("[Bootloader] UnityTransport not found on NetworkManager.");
        return utp;
    }

    string ReadAddress()
    {
        if (addressTMP && !string.IsNullOrWhiteSpace(addressTMP.text)) return addressTMP.text;
        if (addressInput && !string.IsNullOrWhiteSpace(addressInput.text)) return addressInput.text;
        return "127.0.0.1";
    }

    ushort ReadPort()
    {
        if (portTMP && ushort.TryParse(portTMP.text, out var p1)) return p1;
        if (portInput && ushort.TryParse(portInput.text, out var p2)) return p2;
        return 7777;
    }

    bool EnsureNM()
    {
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("[Bootloader] Missing NetworkManager.");
            return false;
        }
        return true;
    }
}
