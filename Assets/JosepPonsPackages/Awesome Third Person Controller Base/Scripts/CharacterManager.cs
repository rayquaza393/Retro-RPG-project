using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TPC
{    
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Third Person Controller/Character Manager")]
    [DisallowMultipleComponent()]
    public class CharacterManager : MonoBehaviour
    {
        public delegate bool DelegateCheckConditions();
        public delegate Type[] DelegateRecoverIncompatible();
        public delegate void DelegateDoAction();

        public DelegateCheckConditions CheckConditions;
        public DelegateRecoverIncompatible RecoverIncompatible;
        public DelegateDoAction DoAction;

        public Vector3 NewVelocity { get; set; }
        public Quaternion NewRotation { get; set; }
        public Vector3 Velocity { get => _controller.velocity; private set => _controller.Move(value * Time.deltaTime); }
        public Quaternion Rotation { get => transform.rotation; private set => transform.rotation = value; }
        public float FloorRadius => _controller.radius;
        public float Height => _controller.height;
        public bool IsGrounded => _controller.isGrounded;

        private CharacterController _controller;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Start()
        {
            NewVelocity = Velocity;
            NewRotation = Rotation;
        }

        private void Update()
        {
            NewVelocity = Velocity;
            NewRotation = Rotation;

            List<Type> checkedExtensions = CheckExtensions();
            checkedExtensions = RemoveIncompatible(checkedExtensions);
            InvokeActions(checkedExtensions);
            Rotation = NewRotation;
            Velocity = NewVelocity;
        }

        private List<Type> CheckExtensions()
        {
            List<Type> checkedExtensions = new List<Type>();

            if (CheckConditions != null)
                foreach (DelegateCheckConditions check in CheckConditions.GetInvocationList())
                    if (check.Invoke())
                        checkedExtensions.Add(check.Method.DeclaringType);
            return checkedExtensions;
        }

        private List<Type> RemoveIncompatible(List<Type> checkedExtensions)
        {
            List<Type> checkedIncompatible = new List<Type>();
            if (RecoverIncompatible != null)
                foreach (DelegateRecoverIncompatible incompatible in RecoverIncompatible.GetInvocationList())
                {
                    if (checkedExtensions.Contains(incompatible.Method.DeclaringType))
                        checkedIncompatible.AddRange(incompatible.Invoke());
                }

            checkedExtensions = checkedExtensions.Except(checkedIncompatible).ToList();
            return checkedExtensions;
        }

        private void InvokeActions(List<Type> checkedExtensions)
        {
            if (DoAction == null)
                return;
            foreach (DelegateDoAction action in DoAction.GetInvocationList())
                if (checkedExtensions.Contains(action.Method.DeclaringType))
                    action.Invoke();
        }
    }
}