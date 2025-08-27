using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("Camera/Third Person Orbit Cam")]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    [Header("Collision")]
    public LayerMask obstructionMask;   // everything except Player
    public float collisionRadius = 0.25f;
    public float collisionSkin = 0.15f;
    public float pushInSpeed = 50f;     // fast when hitting a wall
    public float relaxOutSpeed = 6f;    // slower easing back out
    private float currentDistance;      // smoothed distance

    [Header("Zoom")]
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("Auto-Wiring")]
    public Transform follow;                 // Player root
    public Transform target;                 // CameraTarget (auto if missing)
    public string cameraTargetName = "CameraTarget";

    [Header("Orbit")]
    public float distance = 3.2f;
    public float minPitch = -30f, maxPitch = 70f;
    public float sensitivity = 0.2f;
    public bool invertY = false;
    public bool holdRMBToOrbit = true;
    public bool lockCursorDuringOrbit = true;

    [Header("Follow / Recenter")]
    public float alignSpeedDegPerSec = 1080f;
    public float recenterDuration = 1.0f;

    float yaw, pitch;
    bool orbiting, recentering;
    float recenterT, recenterStartYaw;

    void Awake() { AutoWire();
        currentDistance = distance;
    }
    void OnEnable()
    {
        if (!follow) AutoWire();
        if (follow) yaw = follow.eulerAngles.y;
    }

    void AutoWire()
    {
        if (!follow)
        {
            var tagged = GameObject.FindWithTag("Player");
            if (tagged) follow = tagged.transform;
            else
            {
                var cc = FindObjectOfType<CharacterController>();
                if (cc) follow = cc.transform;
            }
        }

        if (!target && follow)
        {
            var t = follow.Find(cameraTargetName);
            if (t) target = t;
            else
            {
                var go = new GameObject(cameraTargetName);
                go.transform.SetParent(follow, false);
                go.transform.localPosition = new Vector3(0, 1.0f, 0);
                target = go.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (!follow || !target) { AutoWire(); if (!follow || !target) return; }

        var ms = Mouse.current;
        bool rmb = ms != null && ms.rightButton.isPressed;

        // Orbit state
        if (holdRMBToOrbit)
        {
            if (rmb && !orbiting) { orbiting = true; recentering = false; if (lockCursorDuringOrbit) Cursor.lockState = CursorLockMode.Locked; }
            else if (!rmb && orbiting) { orbiting = false; StartRecenter(); if (lockCursorDuringOrbit) Cursor.lockState = CursorLockMode.None; }
        }
        else orbiting = true;

        // Mouse look while orbiting
        if (orbiting && ms != null)
        {
            Vector2 md = ms.delta.ReadValue();
            yaw += md.x * sensitivity;
            pitch += (invertY ? md.y : -md.y) * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Recenter after release
        if (recentering)
        {
            recenterT += Time.deltaTime / recenterDuration;
            float targetYaw = follow.eulerAngles.y;
            yaw = Mathf.LerpAngle(recenterStartYaw, targetYaw, Mathf.Clamp01(recenterT));
            if (recenterT >= 1f) recentering = false;
        }
        else if (!orbiting)
        {
            float targetYaw = follow.eulerAngles.y;
            yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, alignSpeedDegPerSec * Time.deltaTime);
        }

        // Zoom with mouse wheel
        if (ms != null)
        {
            float scroll = ms.scroll.ReadValue().y;   // up = positive
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= scroll * zoomSpeed;       // no Time.deltaTime for wheel
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        // Apply camera transform + anti-clipping
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // Desired (no collision) position
        float desiredDist = distance;
        Vector3 desiredPos = target.position - rot * Vector3.forward * desiredDist;

        // SphereCast from target toward desired position
        Vector3 castDir = (desiredPos - target.position).normalized;
        if (Physics.SphereCast(
                target.position,
                collisionRadius,
                castDir,
                out RaycastHit hit,
                desiredDist,
                obstructionMask,
                QueryTriggerInteraction.Ignore))
        {
            desiredDist = Mathf.Max(0.0f, hit.distance - collisionSkin);
        }

        // Smooth distance (snap in fast, ease out slow)
        float speed = (desiredDist < currentDistance) ? pushInSpeed : relaxOutSpeed;
        currentDistance = Mathf.MoveTowards(currentDistance, desiredDist, speed * Time.deltaTime);

        // Final position
        Vector3 finalPos = target.position - rot * Vector3.forward * currentDistance;
        transform.SetPositionAndRotation(finalPos, rot);

    }

    void StartRecenter()
    {
        recentering = true;
        recenterT = 0f;
        recenterStartYaw = yaw;
    }
}
