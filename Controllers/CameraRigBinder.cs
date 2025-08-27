using UnityEngine;

/// Binds your existing ThirdPersonOrbitCam to the local player you pass in.
/// Put this on the CameraRig (the parent of your actual Camera).
[DisallowMultipleComponent]
public class UseThirdPersonOrbitCamBinder : MonoBehaviour
{
    [Tooltip("Name of the child target under the player for the camera to orbit around.")]
    public string cameraTargetName = "CameraTarget";
    [Tooltip("Height of the CameraTarget if we have to create it.")]
    public float targetHeight = 1.0f;
    [Tooltip("Detach rig to world so player movement doesn't double-transform the camera.")]
    public bool detachRigFromPlayer = true;
    [Tooltip("Deactivate other cameras & enable only this one for the local client.")]
    public bool enforceSingleActiveCamera = true;

    ThirdPersonOrbitCam orbit;

    void Awake()
    {
        orbit = GetComponentInChildren<ThirdPersonOrbitCam>(true);
        if (!orbit)
            Debug.LogWarning("UseThirdPersonOrbitCamBinder: ThirdPersonOrbitCam not found under this rig.");
    }

    /// Call this right after you Instantiate the **local** player.
    public void Bind(Transform localPlayerRoot)
    {
        if (!orbit || !localPlayerRoot) return;

        if (detachRigFromPlayer && transform.parent != null)
            transform.SetParent(null, true);

        // Ensure / create the CameraTarget child under the player
        Transform camTarget = localPlayerRoot.Find(cameraTargetName);
        if (!camTarget)
        {
            var go = new GameObject(cameraTargetName);
            go.transform.SetParent(localPlayerRoot, false);
            go.transform.localPosition = new Vector3(0f, targetHeight, 0f);
            camTarget = go.transform;
        }

        // Bind the orbit cam
        orbit.follow = localPlayerRoot;
        orbit.target = camTarget;

        // Make this the only active camera + AudioListener
        if (enforceSingleActiveCamera)
        {
            var myCam = GetComponentInChildren<Camera>(true);
            if (myCam)
            {
                foreach (var c in Camera.allCameras) c.gameObject.SetActive(c == myCam);
                foreach (var al in FindObjectsOfType<AudioListener>()) al.enabled = (al.gameObject == myCam.gameObject);
                // Tell nametags which camera to billboard against
                NameTag.SetBillboardCamera(myCam);
            }
        }
    }
}
