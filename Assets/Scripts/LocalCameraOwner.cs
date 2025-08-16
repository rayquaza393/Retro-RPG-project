using UnityEngine;

public class LocalCameraOwner : MonoBehaviour
{
    public Camera playerCamera;   // assign in prefab (child camera); if null we’ll find one

    void Start()
    {
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>(true);
        if (playerCamera == null) { Debug.LogError("No player camera found on local prefab."); return; }

        // Make sure ONLY our camera is active
        foreach (var cam in Camera.allCameras)
            if (cam != playerCamera) cam.gameObject.SetActive(false);

        playerCamera.gameObject.SetActive(true);
        playerCamera.tag = "MainCamera"; // optional; helps with third-party bits

        // Ensure a single AudioListener
        var listeners = FindObjectsOfType<AudioListener>();
        foreach (var al in listeners)
            al.enabled = (al.gameObject == playerCamera.gameObject);

        // Tell NameTags which camera to face
        NameTag.SetBillboardCamera(playerCamera);
    }
}
