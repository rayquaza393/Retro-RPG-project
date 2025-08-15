using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;

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

        private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
        private SmartFox sfs;

        private void Start()
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
            sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVarsUpdate);

            if (sfs.LastJoinedRoom != null)
                SpawnExistingUsers();
        }

        private void OnDestroy()
        {
            if (sfs == null) return;
            sfs.RemoveEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
            sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.RemoveEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVarsUpdate);
        }

        private void OnRoomJoin(BaseEvent evt)
        {
            SpawnPlayer(sfs.MySelf, true);
        }

        private void OnUserEnterRoom(BaseEvent evt)
        {
            var user = (User)evt.Params["user"];
            if (user != sfs.MySelf)
                SpawnPlayer(user, false);
        }

        private void OnUserExitRoom(BaseEvent evt)
        {
            var user = (User)evt.Params["user"];
            if (players.TryGetValue(user.Id, out var go))
            {
                Destroy(go);
                players.Remove(user.Id);
            }
        }

        private void OnUserVarsUpdate(BaseEvent evt)
        {
            var user = (User)evt.Params["user"];
            if (user == sfs.MySelf) return;

            if (!players.TryGetValue(user.Id, out var go)) return;

            var vx = user.GetVariable("px");
            var vy = user.GetVariable("py");
            var vz = user.GetVariable("pz");
            var vyaw = user.GetVariable("ry");
            if (vx == null || vy == null || vz == null || vyaw == null) return;

            var pos = new Vector3(
                (float)vx.GetDoubleValue(),
                (float)vy.GetDoubleValue(),
                (float)vz.GetDoubleValue()
            );
            float yaw = (float)vyaw.GetDoubleValue();

            var ra = go.GetComponent<RemoteAvatar>();
            if (ra != null) ra.ApplySnapshot(pos, yaw);
        }

        private void SpawnExistingUsers()
        {
            foreach (User user in sfs.LastJoinedRoom.UserList)
            {
                if (!players.ContainsKey(user.Id))
                    SpawnPlayer(user, user == sfs.MySelf);
            }
        }

        // Make sure this method is at CLASS scope (not nested inside another method)
        private void SpawnPlayer(User user, bool isLocal)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return;
            }

            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = isLocal ? localPlayerPrefab : remotePlayerPrefab;

            if (prefab == null)
            {
                Debug.LogError("Missing prefab for " + (isLocal ? "local" : "remote") + " player!");
                return;
            }

            GameObject playerObj = Instantiate(prefab, spawn.position, spawn.rotation);
            players[user.Id] = playerObj;

            // Add/init sender only to LOCAL player (if not already on prefab)
            if (isLocal)
            {
                var sender = playerObj.GetComponent<LocalNetworkSender>();
                if (sender == null) sender = playerObj.AddComponent<LocalNetworkSender>();
                sender.Init(sfs); // inject client (no GlobalManager dependency inside the sender)
            }
            else
            {
                // Seed remote from any existing vars so it doesn't pop
                var vx = user.GetVariable("px");
                var vy = user.GetVariable("py");
                var vz = user.GetVariable("pz");
                var vyaw = user.GetVariable("ry");
                if (vx != null && vy != null && vz != null && vyaw != null)
                {
                    var ra = playerObj.GetComponent<RemoteAvatar>();
                    if (ra != null)
                    {
                        var pos = new Vector3(
                            (float)vx.GetDoubleValue(),
                            (float)vy.GetDoubleValue(),
                            (float)vz.GetDoubleValue());
                        ra.ApplySnapshot(pos, (float)vyaw.GetDoubleValue());
                    }
                }
            }

            // --- Nametag ---
            if (nameTagPrefab != null)
            {
                var tagObj = Instantiate(nameTagPrefab);
                var tr = tagObj.transform;
                tr.SetParent(playerObj.transform, false);
                tr.localPosition = Vector3.up * nameTagHeight;
                tr.localRotation = Quaternion.identity;
                tr.localScale = Vector3.one;

                string displayName = user.ContainsVariable("displayName")
                    ? user.GetVariable("displayName").GetStringValue()
                    : user.Name;

                var nt = tagObj.GetComponent<NameTag>();
                if (nt != null)
                {
                    nt.target = playerObj.transform;
                    nt.offset = Vector3.up * nameTagHeight;
                    nt.SetText(displayName);
                }
                else
                {
                    var tmp = tagObj.GetComponentInChildren<TMP_Text>();
                    if (tmp != null) { tmp.text = displayName; tmp.fontSize = nameTagFontSize; }
                }
            }
        }
    }
}
