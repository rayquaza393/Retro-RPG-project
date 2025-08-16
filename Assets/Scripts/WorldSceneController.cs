using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace SmartFoxServer.Unity.Examples
{
    public class WorldSceneController : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject localPlayerPrefab;
        public GameObject remotePlayerPrefab;
        public GameObject nameTagPrefab;

        [Header("Nametag Settings")]
        public float nameTagHeight = 1.0f;
        public int nameTagFontSize = 14;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [Header("Camera")]
        // Optional: if your ThirdPersonOrbitCam lives under a prefab rig, assign it here.
        // If you already have a rig in the scene, leave this null.
        public GameObject cameraRigPrefab;

        private readonly Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
        private SmartFox sfs;

        void Start()
        {
            sfs = GlobalManager.Instance.GetSfsClient();
            if (sfs == null || !sfs.IsConnected)
            {
                GlobalManager.Instance.SetCustomError("[ERR-1001] Lost connection or never connected.\nPlease try logging in again.");
                SceneManager.LoadScene("Login");
                return;
            }

            sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
            sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

            if (sfs.LastJoinedRoom != null)
                SpawnExistingUsers();
        }

        void OnDestroy()
        {
            if (sfs == null) return;
            sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
            sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        }

        void OnRoomJoin(BaseEvent evt)
        {
            if (sfs.MySelf != null)
                SpawnPlayer(sfs.MySelf, true);
        }

        void OnUserEnterRoom(BaseEvent evt)
        {
            var user = (User)evt.Params["user"];
            if (user != null && user != sfs.MySelf)
                SpawnPlayer(user, false);
        }

        void OnUserExitRoom(BaseEvent evt)
        {
            var user = (User)evt.Params["user"];
            if (user != null && players.TryGetValue(user.Id, out var go))
            {
                Destroy(go);
                players.Remove(user.Id);
            }
        }

        void SpawnExistingUsers()
        {
            foreach (User user in sfs.LastJoinedRoom.UserList)
            {
                if (!players.ContainsKey(user.Id))
                    SpawnPlayer(user, user == sfs.MySelf);
            }
        }

        // ---------- Net transform ingest ----------
        void OnExtensionResponse(BaseEvent evt)
        {
            string cmd = (string)evt.Params["cmd"];
            var data = (ISFSObject)evt.Params["params"];

            if (cmd == "player.pos")
            {
                int uid = data.ContainsKey("id") ? data.GetInt("id") : -1;
                if (uid == -1) return;

                if (players.TryGetValue(uid, out var go))
                {
                    var recv = go.GetComponent<RemoteNetworkReceiver>();
                    if (!recv) recv = go.AddComponent<RemoteNetworkReceiver>();

                    // Position (XZ) + rotation (yaw or quaternion)
                    float x = data.ContainsKey("x") ? data.GetFloat("x") : 0f;
                    float z = data.ContainsKey("z") ? data.GetFloat("z") : 0f;
                    Vector3 pos = new Vector3(x, 0f, z);

                    Quaternion rot;
                    if (data.ContainsKey("qx"))
                    {
                        rot = new Quaternion(
                            data.GetFloat("qx"), data.GetFloat("qy"),
                            data.GetFloat("qz"), data.GetFloat("qw"));
                    }
                    else
                    {
                        float yaw = data.ContainsKey("yaw") ? data.GetFloat("yaw") : 0f;
                        rot = Quaternion.Euler(0f, yaw, 0f);
                    }

                    recv.SetNetworkTransform(pos, rot);
                }
            }
        }

        // ---------- Spawning ----------
        void SpawnPlayer(User user, bool isLocal)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return;
            }

            var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = isLocal ? localPlayerPrefab : remotePlayerPrefab;

            if (!prefab)
            {
                Debug.LogError($"Missing prefab for {(isLocal ? "local" : "remote")} player!");
                return;
            }

            var playerObj = Instantiate(prefab, spawn.position, spawn.rotation);
            players[user.Id] = playerObj;

            if (isLocal)
            {
                BindThirdPersonOrbitCam(playerObj);

                var sender = playerObj.GetComponent<LocalNetworkSender>();
                if (!sender) sender = playerObj.AddComponent<LocalNetworkSender>();
                sender.Init(sfs);
            }
            else
            {
                DisableAnyEmbeddedCameras(playerObj);
                if (!playerObj.GetComponent<RemoteNetworkReceiver>())
                    playerObj.AddComponent<RemoteNetworkReceiver>();
            }

            // Nametag
            if (nameTagPrefab)
            {
                var tagObj = Instantiate(nameTagPrefab);
                tagObj.transform.SetParent(playerObj.transform, false);
                tagObj.transform.localPosition = Vector3.up * nameTagHeight;

                string displayName = user.ContainsVariable("displayName")
                    ? user.GetVariable("displayName").GetStringValue()
                    : user.Name;

                var nt = tagObj.GetComponent<NameTag>();
                if (nt)
                {
                    nt.target = playerObj.transform;
                    nt.offset = Vector3.up * nameTagHeight;
                    nt.SetText(displayName);
                }

                var tmp = tagObj.GetComponentInChildren<TMP_Text>();
                if (tmp)
                {
                    tmp.text = displayName;
                    tmp.fontSize = nameTagFontSize;
                }
            }
        }

        // Prefer your existing scene rig with ThirdPersonOrbitCam; if not found, optionally spawn a rig prefab that has it.
        void BindThirdPersonOrbitCam(GameObject playerObj)
        {
            ThirdPersonOrbitCam tpo = FindObjectOfType<ThirdPersonOrbitCam>(true);

            if (tpo == null && cameraRigPrefab != null)
            {
                var rig = Instantiate(cameraRigPrefab);
                tpo = rig.GetComponentInChildren<ThirdPersonOrbitCam>(true);
            }

            if (tpo == null)
            {
                Debug.LogWarning("No ThirdPersonOrbitCam found in scene/prefab; camera not bound.");
                return;
            }

            // Bind to player; the script's AutoWire will ensure/attach CameraTarget if missing
            tpo.follow = playerObj.transform;
            // Nudge AutoWire in case follow/target need creating
            tpo.SendMessage("AutoWire", SendMessageOptions.DontRequireReceiver);

            // Make sure only THIS camera is active
            var cam = tpo.GetComponentInChildren<Camera>(true);
            if (cam)
            {
                foreach (var c in Camera.allCameras)
                    c.gameObject.SetActive(c == cam);

                foreach (var al in FindObjectsOfType<AudioListener>())
                    al.enabled = (al.gameObject == cam.gameObject);

                cam.gameObject.SetActive(true);
                cam.enabled = true;

                //tells nametag which camera to billboard against
                NameTag.SetBillboardCamera(cam);
            }
        }

        void DisableAnyEmbeddedCameras(GameObject playerObj)
        {
            foreach (var c in playerObj.GetComponentsInChildren<Camera>(true))
            {
                c.enabled = false;
                c.gameObject.SetActive(false);
            }
            foreach (var al in playerObj.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;
        }
    }
}
