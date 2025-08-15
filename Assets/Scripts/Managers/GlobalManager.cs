using UnityEngine;
using UnityEngine.SceneManagement;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Util;

namespace SmartFoxServer.Unity.Examples
{
    public class GlobalManager : MonoBehaviour
    {
        private static GlobalManager _instance;
        public static GlobalManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("GlobalManager").AddComponent<GlobalManager>();
                return _instance;
            }
        }

        private SmartFox sfs;
        private string connLostMsg;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this);
            Application.runInBackground = true;
            Debug.Log("Global Manager ready");
        }

        private void Update()
        {
            if (sfs != null)
                sfs.ProcessEvents();
        }

        private void OnApplicationQuit()
        {
            if (sfs != null && sfs.IsConnected)
                sfs.Disconnect();
        }

        public string ConnectionLostMessage
        {
            get
            {
                string m = connLostMsg;
                connLostMsg = null;
                return m;
            }
        }

        public void SetCustomError(string message)
        {
            connLostMsg = message;
        }

        public SmartFox CreateSfsClient()
        {
            sfs = new SmartFox();
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            return sfs;
        }

        public SmartFox CreateSfsClient(UseWebSocket useWebSocket)
        {
            sfs = new SmartFox(useWebSocket);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            return sfs;
        }

        public SmartFox GetSfsClient() => sfs;

        private void OnConnectionLost(BaseEvent evt)
        {
            sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs = null;

            string connLostReason = (string)evt.Params["reason"];
            Debug.Log("Connection lost: " + connLostReason);

            if (SceneManager.GetActiveScene().name != "Login")
            {
                if (connLostReason != ClientDisconnectionReason.MANUAL)
                {
                    connLostMsg = "[ERR-1002] Disconnected: ";

                    if (connLostReason == ClientDisconnectionReason.IDLE)
                        connLostMsg += "Idle timeout.";
                    else if (connLostReason == ClientDisconnectionReason.KICK)
                        connLostMsg += "Kicked by an administrator.";
                    else if (connLostReason == ClientDisconnectionReason.BAN)
                        connLostMsg += "Banned by an administrator.";
                    else
                        connLostMsg += "Unknown reason.";
                }

                SceneManager.LoadScene("Login");
            }
        }
    }
}
