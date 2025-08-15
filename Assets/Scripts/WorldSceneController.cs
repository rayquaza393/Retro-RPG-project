using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // For TextMeshPro
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
        public GameObject nameTagPrefab; // <-- Slot in Inspector

        [Header("Nametag Settings")]
        public float nameTagHeight = 2.0f;
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

            if (sfs.LastJoinedRoom != null)
                SpawnExistingUsers();
        }

        private void OnRoomJoin(BaseEvent evt)
        {
            SpawnPlayer(sfs.MySelf, true);
        }

        private void OnUserEnterRoom(BaseEvent evt)
        {
            User user = (User)evt.Params["user"];
            if (user != sfs.MySelf)
                SpawnPlayer(user, false);
        }

        private void OnUserExitRoom(BaseEvent evt)
        {
            User user = (User)evt.Params["user"];
            if (players.ContainsKey(user.Id))
            {
                Destroy(players[user.Id]);
                players.Remove(user.Id);
            }
        }

        private void SpawnExistingUsers()
        {
            foreach (User user in sfs.LastJoinedRoom.UserList)
            {
                if (!players.ContainsKey(user.Id))
                    SpawnPlayer(user, user == sfs.MySelf);
            }
        }

        private void SpawnPlayer(User user, bool isLocal)
        {
            if (spawnPoints.Length == 0)
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

            // --- Nametag spawn ---
            if (nameTagPrefab != null)
            {
                GameObject tagObj = Instantiate(nameTagPrefab, playerObj.transform);
                tagObj.transform.localPosition = Vector3.up * nameTagHeight;

                // Try to get displayName from UserVariables, fallback to username
                string displayName = user.ContainsVariable("displayName")
                    ? user.GetVariable("displayName").GetStringValue()
                    : user.Name;

                TMP_Text tmpText = tagObj.GetComponentInChildren<TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = displayName;
                    tmpText.fontSize = nameTagFontSize;
                }
            }
        }
    }
}
