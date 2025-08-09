// Assets/Scripts/Bootloader.cs
// Drop-in Bootstrap for Unity Netcode for GameObjects
// - Ensures a single persistent NetworkManager
// - Optional auto-start as Host/Client/Server
// - Loads target scenes additively (via NGO SceneManager when hosting)
// Tested with Unity Netcode for GameObjects (com.unity.netcode.gameobjects)
// and Unity Transport (com.unity.transport)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace RetroRPG.Boot
{
    public enum StartMode { None, Host, Client, Server }

    [DefaultExecutionOrder(-10000)]
    public class Bootloader : MonoBehaviour
    {
        private static Bootloader _instance;

        [Header("Networking")]
        [Tooltip("If not assigned, the Bootloader will FindOrAdd a NetworkManager on this GameObject.")]
        [SerializeField] private NetworkManager networkManager;

        [Tooltip("Auto-create UnityTransport if missing on the NetworkManager.")]
        [SerializeField] private bool ensureUnityTransport = true;

        [Tooltip("Transport address (used by UnityTransport).")]
        [SerializeField] private string address = "127.0.0.1";

        [Tooltip("Transport port (used by UnityTransport).")]
        [SerializeField] private ushort port = 7777;

        [Header("Startup")]
        [Tooltip("Optional: automatically start networking on play.")]
        [SerializeField] private StartMode autoStartMode = StartMode.None;

        [Tooltip("Optional delay before auto-start (seconds).")]
        [SerializeField] private float autoStartDelay = 0.1f;

        [Tooltip("Scenes to load after networking starts. For Host/Server these are loaded via NetworkSceneManager; for Client, the server will sync scenes.")]
        [SerializeField] private List<string> scenesToLoad = new List<string> { "GameScene" };

        [Tooltip("Unload the Startup scene after target scenes are loaded.")]
        [SerializeField] private bool unloadStartupScene = true;

        [Header("Diagnostics")]
        [SerializeField] private bool verboseLogs = true;

        private bool _initialized;

        private void Awake()
        {
            // Singleton Bootloader
            if (_instance != null && _instance != this)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Duplicate instance found, destroying this one.");
                Destroy(gameObject);
                return;
            }
            _instance = this;

            DontDestroyOnLoad(gameObject);

            // Ensure/Configure NetworkManager
            EnsureNetworkManager();
            ConfigureTransport();

            // Keep NetworkManager alive across scenes
            DontDestroyOnLoad(networkManager.gameObject);

            // Subscribe to NGO scene events for logging
            if (networkManager != null && networkManager.SceneManager != null)
            {
                networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }

            _initialized = true;
        }

        private void Start()
        {
            if (autoStartMode != StartMode.None)
            {
                // Small delay to allow editor/playmode initialization
                Invoke(nameof(DoAutoStart), autoStartDelay);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;

            if (networkManager != null && networkManager.SceneManager != null)
            {
                networkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }

        // -------- PUBLIC BUTTON HOOKS (wire these to your UI) --------

        public void StartAsHost()
        {
            EnsureInitialized();
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Starting as Host…");
                if (NetworkManager.Singleton.StartHost())
                {
                    LoadTargetScenesAsHostOrServer();
                }
                else
                {
                    Debug.LogError("[Bootloader] StartHost failed.");
                }
            }
        }

        public void StartAsClient()
        {
            EnsureInitialized();
            var nm = NetworkManager.Singleton;
            if (nm != null && !nm.IsListening)
            {
                if (verboseLogs) Debug.Log($"[Bootloader] Starting as Client… ({address}:{port})");
                if (!nm.StartClient())
                {
                    Debug.LogError("[Bootloader] StartClient failed.");
                }
            }
        }

        public void StartAsServer()
        {
            EnsureInitialized();
            var nm = NetworkManager.Singleton;
            if (nm != null && !nm.IsListening)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Starting as Server…");
                if (nm.StartServer())
                {
                    LoadTargetScenesAsHostOrServer();
                }
                else
                {
                    Debug.LogError("[Bootloader] StartServer failed.");
                }
            }
        }

        public void Shutdown()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Shutting down network…");
                nm.Shutdown();
            }
        }

        // ----------------- INTERNALS -----------------

        private void DoAutoStart()
        {
            switch (autoStartMode)
            {
                case StartMode.Host: StartAsHost(); break;
                case StartMode.Client: StartAsClient(); break;
                case StartMode.Server: StartAsServer(); break;
                case StartMode.None: /* noop */         break;
            }
        }

        private void EnsureInitialized()
        {
            if (!_initialized) Awake();
        }

        private void EnsureNetworkManager()
        {
            // If user assigned one in Inspector, prefer it
            if (networkManager == null)
            {
                networkManager = GetComponent<NetworkManager>();
                if (networkManager == null)
                {
                    // Create one on this GameObject
                    networkManager = gameObject.AddComponent<NetworkManager>();
                    if (verboseLogs) Debug.Log("[Bootloader] No NetworkManager found; added one to Bootloader GameObject.");
                }
            }

            // Make sure the static Singleton points to ours
            if (NetworkManager.Singleton != networkManager)
            {
                // This will be set when the component enables; just sanity log
                if (verboseLogs) Debug.Log("[Bootloader] NetworkManager instance prepared.");
            }
        }

        private void ConfigureTransport()
        {
            if (!ensureUnityTransport || networkManager == null) return;

            var ut = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (ut == null)
            {
                ut = networkManager.gameObject.GetComponent<UnityTransport>();
                if (ut == null) ut = networkManager.gameObject.AddComponent<UnityTransport>();
                networkManager.NetworkConfig.NetworkTransport = ut;

                if (verboseLogs) Debug.Log("[Bootloader] UnityTransport added and bound to NetworkManager.");
            }

            // Configure address/port
            ut.SetConnectionData(address, port);
        }

        private void LoadTargetScenesAsHostOrServer()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;

            if (nm.SceneManager == null)
            {
                Debug.LogWarning("[Bootloader] NetworkSceneManager is null; loading scenes locally (non-networked).");
                LoadScenesLocally();
                return;
            }

            // Host/Server authoritative scene loading
            foreach (var sceneName in scenesToLoad)
            {
                if (string.IsNullOrWhiteSpace(sceneName)) continue;
                if (verboseLogs) Debug.Log($"[Bootloader] (Networked) Loading scene: {sceneName} (Additive)");
                nm.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }

            if (unloadStartupScene)
            {
                // Defer unloading until at least one load completes to avoid bouncing
                TryUnloadStartupSceneDeferred();
            }
        }

        private void TryUnloadStartupSceneDeferred()
        {
            StartCoroutine(UnloadAfterFrame());
        }

        private IEnumerator UnloadAfterFrame()
        {
            // wait 1 frame
            yield return null;

            var active = SceneManager.GetActiveScene();
            var thisScene = gameObject.scene;
            if (unloadStartupScene && thisScene.IsValid() && thisScene.isLoaded && thisScene != active)
            {
                if (verboseLogs) Debug.Log($"[Bootloader] Unloading startup scene: {thisScene.name}");
                SceneManager.UnloadSceneAsync(thisScene);
            }
        }

        private void LoadScenesLocally()
        {
            foreach (var sceneName in scenesToLoad)
            {
                if (string.IsNullOrWhiteSpace(sceneName)) continue;
                if (verboseLogs) Debug.Log($"[Bootloader] (Local) Loading scene: {sceneName} (Additive)");
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }

            if (unloadStartupScene)
            {
                var thisScene = gameObject.scene;
                if (thisScene.IsValid() && thisScene.isLoaded)
                {
                    if (verboseLogs) Debug.Log($"[Bootloader] Unloading startup scene: {thisScene.name}");
                    SceneManager.UnloadSceneAsync(thisScene);
                }
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!verboseLogs) return;

            Debug.Log($"[Bootloader] Networked scene load completed: {sceneName} | Mode: {mode} | Clients OK: {clientsCompleted.Count} | Timeouts: {clientsTimedOut.Count}");
        }
    }
}
