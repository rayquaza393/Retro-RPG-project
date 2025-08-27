using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RemoteAnimatorController : MonoBehaviour
{
    public string speedParam = "Speed";
    public float smoothTime = 5f;

    private Animator animator;
    private float targetSpeed = 0f;
    private float currentSpeed = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetRemoteSpeed(float speed)
    {
        targetSpeed = speed;
    }

    void Update()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * smoothTime);
        animator.SetFloat(speedParam, currentSpeed);
    }
}
