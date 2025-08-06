using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DemoExtension : TPC.CharacterExtension
{
    private Animator _animator;
    private float time;

    public new void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

    protected override bool CheckConditions()
    {
        if (IsGrounded && Velocity.x == 0 && Velocity.z == 0) // player on ground and it is not moving
            time += Time.deltaTime;
        else
        {
            time = 0;
            _animator.SetBool("DemoDance", false); // Dissable the action
        }
        return time > 2;
    }

    protected override void Action() => _animator.SetBool("DemoDance", true); // Performs the action
}
