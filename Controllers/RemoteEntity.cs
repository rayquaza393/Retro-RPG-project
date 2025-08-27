using UnityEngine;

public class RemoteEntity : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetAnimatorState(float speed)
    {
        if (animator)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    public void HandleDisplayName(string displayName)
    {
        // update name tag text or billboard
        var nameTag = GetComponentInChildren<NameTag>();
        if (nameTag)
            nameTag.SetText(displayName);
    }
}
