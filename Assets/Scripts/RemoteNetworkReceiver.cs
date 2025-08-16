using UnityEngine;

public class RemoteNetworkReceiver : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;                      // auto-fill if left empty
    public bool disableRootMotion = true;
    public string groundedParam = "IsGrounded";    // exact, case-sensitive
    public string speedParam = "Speed";            // exact, case-sensitive
    public float speedSmoothing = 10f;             // higher = snappier
    public float maxSpeedForAnim = 6f;             // meters/sec that maps to Speed=1

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    public float groundProbeHeight = 2f;
    public float pivotToFoot = 1f;
    public float heightOffset = 0.02f;
    public bool debugGroundRay = false;

    [Header("Smoothing")]
    public float posLerp = 12f;
    public float rotLerp = 12f;

    // --- internal ---
    private Vector2 targetXZ;
    private Quaternion targetRot = Quaternion.identity;
    private Vector3 lastFramePos;
    private float smoothedSpeed;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator && disableRootMotion) animator.applyRootMotion = false;

        var p = transform.position;
        targetXZ = new Vector2(p.x, p.z);
        targetRot = transform.rotation;
        lastFramePos = p;

        MakePassivePhysics();
    }

    /// Call from net: we ignore incoming Y on purpose
    public void SetNetworkTransform(Vector3 pos, Quaternion rot)
    {
        targetXZ = new Vector2(pos.x, pos.z);
        targetRot = rot;
    }

    void Update()
    {
        // cache position before movement to compute speed after
        Vector3 before = transform.position;

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot,
            1f - Mathf.Exp(-rotLerp * Time.deltaTime));

        // Step toward target XZ
        Vector2 curXZ = new Vector2(before.x, before.z);
        Vector2 nextXZ = Vector2.Lerp(curXZ, targetXZ, 1f - Mathf.Exp(-posLerp * Time.deltaTime));

        // Ground probe at target XZ
        float desiredY = before.y;
        Vector3 rayStart = new Vector3(nextXZ.x, before.y + groundProbeHeight, nextXZ.y);
        if (Physics.Raycast(rayStart, Vector3.down, out var hit, groundProbeHeight + 5f, groundMask, QueryTriggerInteraction.Ignore))
        {
            desiredY = hit.point.y + pivotToFoot + heightOffset;
            if (animator) animator.SetBool(groundedParam, true);
        }
        else
        {
            if (animator) animator.SetBool(groundedParam, false);
        }

        if (debugGroundRay)
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * (groundProbeHeight + 5f), Color.green);
        }

        // Apply position
        Vector3 desiredPos = new Vector3(nextXZ.x, desiredY, nextXZ.y);
        transform.position = desiredPos;

        // --- Compute planar speed this frame and drive animator ---
        Vector3 moved = transform.position - before;            // this frame’s motion
        float rawSpeed = new Vector2(moved.x, moved.z).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, 1f - Mathf.Exp(-speedSmoothing * Time.deltaTime));
        float normSpeed = Mathf.Clamp01(smoothedSpeed / Mathf.Max(0.001f, maxSpeedForAnim));

        if (animator && !string.IsNullOrEmpty(speedParam))
            animator.SetFloat(speedParam, normSpeed);
    }

    void MakePassivePhysics()
    {
        // Ensure remote proxies don't fight physics.
        var cc = GetComponent<CharacterController>();
        if (cc) Destroy(cc);

        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        var col = GetComponent<CapsuleCollider>();
        if (!col) col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1f, 0f);
        col.height = 2f;
        col.radius = 0.35f;
        col.isTrigger = true;
    }
}
