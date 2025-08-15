using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{
    public TMP_Text label;
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.0f, 0);
    public bool yawOnly = true;
    Camera cam;

    void LateUpdate()
    {
        if (!target) return;
        if (!cam) cam = Camera.main;

        transform.position = target.position + offset;

        if (cam)
        {
            if (yawOnly)
            {
                var dir = cam.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(-dir);
            }
            else
            {
                transform.LookAt(cam.transform, Vector3.up);
            }
        }
    }

    public void SetText(string s) { if (label) label.text = s; }
}
