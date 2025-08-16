using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = new Vector3(0f, 1.6f, -3.5f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.4f, 0f);
    public float posLerp = 12f;
    public float rotLerp = 12f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.TransformPoint(positionOffset);
        transform.position = Vector3.Lerp(
            transform.position, desiredPos, 1f - Mathf.Exp(-posLerp * Time.deltaTime));

        Vector3 lookAt = target.position + target.TransformVector(lookAtOffset);
        var desiredRot = Quaternion.LookRotation((lookAt - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, desiredRot, 1f - Mathf.Exp(-rotLerp * Time.deltaTime));
    }
}
