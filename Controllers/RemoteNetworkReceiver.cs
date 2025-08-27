using UnityEngine;

public class RemoteNetworkReceiver : MonoBehaviour
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float speed;

    private RemoteEntity entity;

    void Awake()
    {
        entity = GetComponent<RemoteEntity>();
    }

    public void SetNetworkTransform(Vector3 position, Quaternion rotation, float newSpeed)
    {
        targetPosition = position;
        targetRotation = rotation;
        speed = newSpeed;

        if (entity != null)
            entity.SetAnimatorState(newSpeed);
    }

    void Update()
    {
        // Simple lerp-based smoothing
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}
