using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonWoWController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float backwardSpeed = 3.0f;
    public float strafeSpeed = 4.0f;
    public float turnSpeedDegPerSec = 240f;

    [Header("Jump / Gravity")]
    public float jumpForce = 8f;
    public float gravity = -20f;
    [Tooltip("Small downward stick when grounded so slopes/ramps feel stable.")]
    public float groundedStick = -0.6f;

    [Header("Camera")]
    public bool useCinemachineOrbit = true; // we’re using our orbit cam, but this still means “read external camera yaw”

    [Header("Animation (optional)")]
    public Animator animator;
    public string animParamForward = "Forward";
    public string animParamStrafe = "Strafe";
    public string animParamSpeed = "Speed";
    public string animParamIsGrounded = "IsGrounded";

    CharacterController cc;
    Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        var ms = Mouse.current;

        bool rmb = ms != null && ms.rightButton.isPressed;

        // --- Input ---
        float fwd = 0f, strafe = 0f, turn = 0f;

        if (kb != null)
        {
            if (kb.wKey.isPressed) fwd += 1f;
            if (kb.sKey.isPressed) fwd -= 1f;

            // Q/E always strafe
            if (kb.qKey.isPressed) strafe -= 1f;
            if (kb.eKey.isPressed) strafe += 1f;

            // A/D turn by default; with RMB they strafe instead
            bool a = kb.aKey.isPressed;
            bool d = kb.dKey.isPressed;
            if (rmb)
            {
                if (a) strafe -= 1f;
                if (d) strafe += 1f;
            }
            else
            {
                if (a) turn -= 1f;
                if (d) turn += 1f;
            }
        }

        // --- Rotation ---
        if (!rmb)
        {
            if (Mathf.Abs(turn) > 0.001f)
                transform.Rotate(Vector3.up, turn * turnSpeedDegPerSec * Time.deltaTime);
        }
        else
        {
            if (Mathf.Abs(fwd) > 0.001f || Mathf.Abs(strafe) > 0.001f)
            {
                float headingY = transform.eulerAngles.y;
                if (useCinemachineOrbit && Camera.main)
                    headingY = Camera.main.transform.eulerAngles.y;

                var targetRot = Quaternion.Euler(0f, headingY, 0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
            }
        }

        // --- Movement ---
        float speedZ = (fwd >= 0f) ? moveSpeed : backwardSpeed;
        Vector3 localMove = new Vector3(strafe * strafeSpeed, 0f, fwd * speedZ);
        Vector3 worldMove = transform.TransformDirection(localMove);
        worldMove.y = 0f;

        // --- Jump & Gravity ---
        if (cc.isGrounded)
        {
            velocity.y = groundedStick;

            if (kb != null && kb.spaceKey.wasPressedThisFrame)
                velocity.y = jumpForce;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 motion = (worldMove + new Vector3(0, velocity.y, 0)) * Time.deltaTime;
        cc.Move(motion);

        // --- Animator ---
        if (animator)
        {
            Vector3 localVel = transform.InverseTransformDirection(new Vector3(cc.velocity.x, 0, cc.velocity.z));
            float normFwd = Mathf.Clamp(localVel.z / moveSpeed, -1f, 1f);
            float normStraf = Mathf.Clamp(localVel.x / strafeSpeed, -1f, 1f);

            animator.SetFloat(animParamForward, normFwd, 0.1f, Time.deltaTime);
            animator.SetFloat(animParamStrafe, normStraf, 0.1f, Time.deltaTime);
            animator.SetFloat(animParamSpeed, new Vector2(normStraf, normFwd).magnitude, 0.1f, Time.deltaTime);

            if (!string.IsNullOrEmpty(animParamIsGrounded))
                animator.SetBool(animParamIsGrounded, cc.isGrounded);
        }
    }
}
