using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Util;
using Sfs2X.Entities.Data;

namespace SmartFoxServer.Unity.Examples
{
    public class LoginSceneController : MonoBehaviour
    {
        [Header("Server")]
        public string host = "127.0.0.1";
        public int tcpPort = 9933;
        public int httpPort = 8080;
        public string zone = "RetroRPG";
        public bool debug = false;

        [Header("UI")]
        public TMP_InputField nameInput;
        public TMP_InputField passwordInput; // NEW
        public Button loginButton;
        public TMP_Text errorText;

        [Header("Flow")]
        public string roomName = "World01_RM";
        public string sceneOnJoin = "World01";

        [Header("Extension (optional, for DB/profile)")]
        // If you're using a Zone or Room extension to hit SQL, set these.
        // Otherwise you can ignore this section.
        public bool requestProfileAfterLogin = true;
        public string extensionId = "db";                 // your Zone extension id
        public string profileCmd = "player.profile.get";  // command your extension handles

        private SmartFox sfs;

        private void Start()
        {
            EventSystem.current.SetSelectedGameObject(nameInput ? nameInput.gameObject : null);
            if (nameInput)
            {
                nameInput.Select();
                nameInput.ActivateInputField();
            }

            // Mask password input
            if (passwordInput)
            {
                passwordInput.contentType = TMP_InputField.ContentType.Password;
                passwordInput.ForceLabelUpdate();
            }

            string connLostMsg = GlobalManager.Instance.ConnectionLostMessage;
            if (!string.IsNullOrEmpty(connLostMsg))
                errorText.text = connLostMsg;
        }

        public void OnNameInputEndEdit()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                Connect();
        }

        public void OnPasswordInputEndEdit() // optional hook if you wire it in the inspector
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                Connect();
        }

        public void OnLoginButtonClick() => Connect();

        private void EnableUI(bool enable)
        {
            if (nameInput) nameInput.interactable = enable;
            if (passwordInput) passwordInput.interactable = enable; // NEW
            if (loginButton) loginButton.interactable = enable;
        }

        private void Connect()
        {
            EnableUI(false);
            errorText.text = "";

            var cfg = new ConfigData
            {
                Host = host,
                Port = tcpPort,
                Zone = zone,
                Debug = debug
            };

#if UNITY_WEBGL
            cfg.Port = httpPort;
#endif

#if !UNITY_WEBGL
            sfs = GlobalManager.Instance.CreateSfsClient();
#else
            sfs = GlobalManager.Instance.CreateSfsClient(UseWebSocket.WS_BIN);
#endif
            sfs.Logger.EnableConsoleTrace = debug;

            AddSmartFoxListeners();
            sfs.Connect(cfg);
        }

        private void AddSmartFoxListeners()
        {
            sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);

            // For extension replies (profile/DB, etc.)
            sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse); // NEW
        }

        private void RemoveSmartFoxListeners()
        {
            if (sfs == null) return;

            sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
            sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse); // NEW
        }

        private void OnConnection(BaseEvent evt)
        {
            if ((bool)evt.Params["success"])
            {
                string user = nameInput ? nameInput.text.Trim() : "";
                string pass = passwordInput ? passwordInput.text.Trim() : ""; 

                // Optional: include extra login params (e.g., client version, platform, etc.)
                ISFSObject loginParams = new SFSObject();
                loginParams.PutUtfString("client", Application.platform.ToString());
                loginParams.PutUtfString("ver", Application.version);

                // Send login with username + password + zone (recommended overload)
                sfs.Send(new LoginRequest(user, pass, zone, loginParams));
            }
            else
            {
                GlobalManager.Instance.SetCustomError("[ERR-1000] Connection failed; is the server running?");
                EnableUI(true);
            }
        }

        private void OnConnectionLost(BaseEvent evt)
        {
            RemoveSmartFoxListeners();
            EnableUI(true);
        }

        private void OnLogin(BaseEvent evt)
        {
            // If you are using a custom login on the server to check SQL creds,
            // you're already authenticated here. You can optionally ask your extension
            // for player profile / inventory / etc. before or after joining a room.
            if (requestProfileAfterLogin && !string.IsNullOrEmpty(extensionId))
            {
                var req = new SFSObject();
                // Add anything your extension needs to fetch profile:
                // e.g., req.PutBool("includeInventory", true);
                sfs.Send(new ExtensionRequest(profileCmd, req, null)); // Zone-level extension
            }

            // Proceed to join the starting room
            sfs.Send(new JoinRoomRequest(roomName));
        }

        private void OnLoginError(BaseEvent evt)
        {
            string msg = (string)evt.Params["errorMessage"];
            if (errorText) errorText.text = string.IsNullOrEmpty(msg) ? "Login failed." : msg;
            EnableUI(true);
            StartCoroutine(DisconnectNextFrame());
        }

        private System.Collections.IEnumerator DisconnectNextFrame()
        {
            yield return null;               // next frame prevents the blocking read race
            if (sfs != null && sfs.IsConnected) sfs.Disconnect();
        }


        private void OnRoomJoin(BaseEvent evt)
        {
            if (!string.IsNullOrEmpty(sceneOnJoin))
                SceneManager.LoadScene(sceneOnJoin);
        }

        private void OnRoomJoinError(BaseEvent evt)
        {
            GlobalManager.Instance.SetCustomError("[ERR-2000] Room join failed: " + (string)evt.Params["errorMessage"]);
            sfs.Disconnect();
            EnableUI(true);
        }

        private void OnExtensionResponse(BaseEvent evt) // NEW
        {
            // Handle replies from your Zone extension (e.g., profile data after login)
            string cmd = (string)evt.Params["cmd"];
            ISFSObject data = (ISFSObject)evt.Params["params"];

            if (cmd == profileCmd)
            {
                // Example: pull fields from your extension’s SQL result
                // var displayName = data.GetUtfString("displayName");
                // var level = data.GetInt("level");
                // var gold = data.GetInt("gold");
                // Cache into a global singleton, etc.
                // GlobalManager.Instance.PlayerProfile = new Profile{ ... };
            }
        }
    }
}
