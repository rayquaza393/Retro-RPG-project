// RemoteAvatar.cs
using UnityEngine;

public class RemoteAvatar : MonoBehaviour
{
    public float posLerp = 10f;
    public float rotLerp = 10f;

    Vector3 targetPos;
    Quaternion targetRot;
    bool first = true;

    public void ApplySnapshot(Vector3 pos, float yawDeg)
    {
        targetPos = pos;
        targetRot = Quaternion.Euler(0f, yawDeg, 0f);

        if (first)
        { // snap on first packet
            transform.position = targetPos;
            transform.rotation = targetRot;
            first = false;
        }
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * posLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotLerp);
    }
}
