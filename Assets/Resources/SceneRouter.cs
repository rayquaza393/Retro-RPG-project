// Assets/Scripts/Bootloader.cs
// Robust Bootloader for Unity Netcode (Unity 6.x)
// - Keeps single NetworkManager alive
// - Starts Host/Client/Server
// - Loads target scenes additively
// - Sets FIRST non-startup loaded scene active
// - Unloads Startup after handoff (no name matching)

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
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private bool ensureUnityTransport = true;
        [SerializeField] private string address = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        [Header("Startup")]
        [SerializeField] private StartMode autoStartMode = StartMode.None;
        [SerializeField] private float autoStartDelay = 0.1f;
        [Tooltip("Scenes to load (additive) after networking starts (host/server). Clients sync from host.")]
        [SerializeField] private List<string> scenesToLoad = new() { "GameScene" };
        [SerializeField] private bool unloadStartupScene = true;
        [SerializeField] private bool makeLoadedSceneActive = true;

        [Header("Diagnostics")]
        [SerializeField] private bool verboseLogs = true;

        private bool _initialized;
        private bool _startupUnloaded;
        private bool _activeSet;
        private Scene _startupScene;

        private void Awake()
        {
            // Singleton
            if (_instance != null && _instance != this)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Duplicate instance; destroying this one.");
                Destroy(gameObject);
                return;
            }
            _instance = this;

            _startupScene = gameObject.scene;      // remember Startup scene
            DontDestroyOnLoad(gameObject);         // move to DontDestroyOnLoad

            EnsureNetworkManager();
            ConfigureTransport();

            // Keep NM alive
            DontDestroyOnLoad(networkManager.gameObject);

            // Hooks
            if (networkManager != null && networkManager.SceneManager != null)
            {
                networkManager.SceneManager.OnLoadEventCompleted += OnNetworkedLoadCompleted;
            }
            SceneManager.sceneLoaded += OnLocalSceneLoaded;

            _initialized = true;

            if (verboseLogs)
                Debug.Log($"[Bootloader] Awake in scene '{_startupScene.name}'.");
        }

        private void Start()
        {
            if (autoStartMode != StartMode.None)
                Invoke(nameof(DoAutoStart), autoStartDelay);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;

            if (networkManager != null && networkManager.SceneManager != null)
                networkManager.SceneManager.OnLoadEventCompleted -= OnNetworkedLoadCompleted;

            SceneManager.sceneLoaded -= OnLocalSceneLoaded;
        }

        // UI hooks
        public void StartAsHost()
        {
            EnsureInitialized();
            if (!NetworkManager.Singleton || NetworkManager.Singleton.IsListening) return;

            if (verboseLogs) Debug.Log("[Bootloader] Start as Host");
            if (NetworkManager.Singleton.StartHost())
                LoadTargetScenesAsHostOrServer();
            else
                Debug.LogError("[Bootloader] StartHost failed.");
        }

        public void StartAsClient()
        {
            EnsureInitialized();
            var nm = NetworkManager.Singleton;
            if (!nm || nm.IsListening) return;

            if (verboseLogs) Debug.Log($"[Bootloader] Start as Client ({address}:{port})");
            if (!nm.StartClient())
                Debug.LogError("[Bootloader] StartClient failed.");
        }

        public void StartAsServer()
        {
            EnsureInitialized();
            var nm = NetworkManager.Singleton;
            if (!nm || nm.IsListening) return;

            if (verboseLogs) Debug.Log("[Bootloader] Start as Server");
            if (nm.StartServer())
                LoadTargetScenesAsHostOrServer();
            else
                Debug.LogError("[Bootloader] StartServer failed.");
        }

        public void Shutdown()
        {
            var nm = NetworkManager.Singleton;
            if (nm && nm.IsListening)
            {
                if (verboseLogs) Debug.Log("[Bootloader] Shutdown");
                nm.Shutdown();
            }
        }

        // Internals
        private void DoAutoStart()
        {
            switch (autoStartMode)
            {
                case StartMode.Host: StartAsHost(); break;
                case StartMode.Client: StartAsClient(); break;
                case StartMode.Server: StartAsServer(); break;
            }
        }

        private void EnsureInitialized()
        {
            if (!_initialized) Awake();
        }

        private void EnsureNetworkManager()
        {
            if (!networkManager)
            {
                networkManager = GetComponent<NetworkManager>();
                if (!networkManager)
                {
                    networkManager = gameObject.AddComponent<NetworkManager>();
                    if (verboseLogs) Debug.Log("[Bootloader] Added NetworkManager to Bootloader GO.");
                }
            }
        }

        private void ConfigureTransport()
        {
            if (!ensureUnityTransport || !networkManager) return;

            var ut = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (!ut)
            {
                ut = networkManager.gameObject.GetComponent<UnityTransport>() ?? networkManager.gameObject.AddComponent<UnityTransport>();
                networkManager.NetworkConfig.NetworkTransport = ut;
                if (verboseLogs) Debug.Log("[Bootloader] UnityTransport attached and bound.");
            }
            ut.SetConnectionData(address, port);
        }

        private void LoadTargetScenesAsHostOrServer()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;

            if (nm.SceneManager == null)
            {
                if (verboseLogs) Debug.LogWarning("[Bootloader] NetworkSceneManager null; loading locally.");
                foreach (var s in scenesToLoad)
                    if (!string.IsNullOrWhiteSpace(s))
                        SceneManager.LoadScene(s, LoadSceneMode.Additive);
                return;
            }

            foreach (var s in scenesToLoad)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (verboseLogs) Debug.Log($"[Bootloader] Networked load: {s} (Additive)");
                nm.SceneManager.LoadScene(s, LoadSceneMode.Additive);
            }
        }

        // Handoffs
        private void OnNetworkedLoadCompleted(string sceneName, LoadSceneMode mode, List<ulong> ok, List<ulong> timeouts)
        {
            if (verboseLogs)
                Debug.Log($"[Bootloader] NGO load complete → {sceneName} | clients OK: {ok.Count} | timeouts: {timeouts.Count}");

            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
                StartCoroutine(MakeActiveThenUnloadStartup_Co(scene));
        }

        private void OnLocalSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (verboseLogs)
                Debug.Log($"[Bootloader] Local load → {scene.name} | mode: {mode}");

            StartCoroutine(MakeActiveThenUnloadStartup_Co(scene));
        }

        private IEnumerator MakeActiveThenUnloadStartup_Co(Scene loaded)
        {
            // Ignore our own startup scene
            if (loaded == _startupScene) yield break;

            // Wait until fully loaded (paranoia)
            while (!loaded.IsValid() || !loaded.isLoaded)
                yield return null;

            // Set active once, to the first non-startup scene
            if (makeLoadedSceneActive && !_activeSet)
            {
                if (verboseLogs) Debug.Log($"[Bootloader] Setting active scene → {loaded.name}");
                SceneManager.SetActiveScene(loaded);
                _activeSet = true;
            }

            // Unload Startup after handoff
            if (unloadStartupScene && !_startupUnloaded)
            {
                if (_startupScene.IsValid() && _startupScene.isLoaded && _startupScene != loaded)
                {
                    if (verboseLogs) Debug.Log($"[Bootloader] Unloading startup scene: {_startupScene.name}");
                    _startupUnloaded = true;
                    SceneManager.UnloadSceneAsync(_startupScene);
                }
            }

            if (verboseLogs)
            {
                var active = SceneManager.GetActiveScene();
                Debug.Log($"[Bootloader] Active: {active.name} | Startup unloaded: {_startupUnloaded}");
            }
        }
    }
}
