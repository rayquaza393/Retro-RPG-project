using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class NameTag : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, 0f);

    [Header("Billboard")]
    public bool billboard = true;
    public Camera cameraOverride;               // optional per-instance override

    private static Camera s_BillboardCam;       // set once per local client
    private TMP_Text _tmp;

    public static void SetBillboardCamera(Camera cam)
    {
        s_BillboardCam = cam;
    }

    void Awake()
    {
        _tmp = GetComponentInChildren<TMP_Text>();
    }

    void LateUpdate()
    {
        // 1) Follow
        if (target)
            transform.position = target.position + offset;

        if (!billboard) return;

        // 2) Pick camera
        Camera cam = cameraOverride
                     ? cameraOverride
                     : (s_BillboardCam
                        ? s_BillboardCam
                        : (Camera.main ? Camera.main : GetAnyEnabledCamera()));

        if (!cam) return;

        // 3) Face camera without inheriting roll
        Vector3 toCam = transform.position - cam.transform.position;
        if (toCam.sqrMagnitude < 0.000001f) return;

        transform.rotation = Quaternion.LookRotation(toCam, cam.transform.up);
    }

    Camera GetAnyEnabledCamera()
    {
        foreach (var c in Camera.allCameras)
            if (c && c.enabled && c.gameObject.activeInHierarchy)
                return c;
        return null;
    }

    public void SetText(string s)
    {
        if (!_tmp) _tmp = GetComponentInChildren<TMP_Text>();
        if (_tmp) _tmp.text = s;
    }
}
