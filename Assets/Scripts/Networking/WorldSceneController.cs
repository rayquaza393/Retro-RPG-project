using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Runtime.CompilerServices;

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
        public GameObject cameraRigPrefab;

        private readonly Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

        void Start()
        {
            Debug.Log("[BOOT] WorldSceneController.Start() called.");

            // TODO: Replace SmartFox networking with NetworkAPI version
            Debug.LogWarning("[SFS STUB] Skipped SmartFox connection check");

            // Stubbed spawn test (manual local player spawn for now)
            SpawnFakeLocalPlayer();
        }

        
        void OnDestroy()
        {
            // STUB: Clean-up SmartFox listeners removed
        }

        void SpawnFakeLocalPlayer()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return;
            }

            //var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (!localPlayerPrefab)
            {
                Debug.LogError("Missing localPlayerPrefab reference!");
                return;
            }

            //var playerObj = Instantiate(localPlayerPrefab, spawn.position, spawn.rotation);
            Debug.Log("[SPAWN] Local player stubbed into world");

            // Optional camera setup
            //BindThirdPersonOrbitCam(playerObj);
        }

        void BindThirdPersonOrbitCam(GameObject playerObj)
        {
            
            //ThirdPersonOrbitCam tpo = FindObjectOfType<ThirdPersonOrbitCam>(true);

            //if (tpo == null && cameraRigPrefab != null)
            {
                var rig = Instantiate(cameraRigPrefab);
                //tpo = rig.GetComponentInChildren<ThirdPersonOrbitCam>(true);
            }

            //if (tpo == null)
            {
                Debug.LogWarning("No ThirdPersonOrbitCam found in scene/prefab; camera not bound.");
                return;
            }

            //tpo.follow = playerObj.transform;
            //tpo.SendMessage("AutoWire", SendMessageOptions.DontRequireReceiver);

            //var cam = tpo.GetComponentInChildren<Camera>(true);
            //if (cam)
            {
                //foreach (var c in Camera.allCameras)
                   // c.gameObject.SetActive(c == cam);

                //foreach (var al in FindObjectsOfType<AudioListener>())
                    //al.enabled = (al.gameObject == cam.gameObject);

                //cam.gameObject.SetActive(true);
                //cam.enabled = true;

                //NameTag.SetBillboardCamera(cam);
            }
        }
    }
}
