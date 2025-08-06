using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
    [RequireComponent(typeof(CharacterManager))]
    [AddComponentMenu("Third Person Controller/Character Walk Run")]
    [DisallowMultipleComponent()]
    public class CharacterWalkRun : CharacterExtension
    {
        #region Inspector
        [Space(10)]
        public CharacterScriptableWalkRun configScriptable;

        [Space(10)]
        public UnityEvent<float, float> SetDirectionalSpeed;
        public UnityEvent<float> SetMotionSpeed;
        #endregion

        #region Properties
        public Vector2 Movement { get; set; }
        public bool Sprint { get; set; }
        public float MoveMagnitude => Movement != Vector2.zero ? Movement.magnitude : 1;
        #endregion

        #region Internal attributes and properties
        private float _speed;
        private float _animationSpeed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _rotation;
        private Vector3 _targetDirection;
        private Vector3 _directionalSpeed;
        private float _cameraRotation;
        private float _motionSpeed;
        #endregion

        #region Methods
        protected override bool CheckConditions() => true;

        protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

        protected override void Action()
        {
            Rotate();
            WalkRun();
            SetFeedback();
        }

        private void SetFeedback()
        {
            _directionalSpeed = Quaternion.Euler(0.0f, _rotationVelocity, 0.0f) * Vector3.forward * _animationSpeed;
            SetDirectionalSpeed?.Invoke(Mathf.Round(_directionalSpeed.z * 100) / 100, Mathf.Round(_directionalSpeed.x * 100) / 100);
            SetMotionSpeed?.Invoke(Mathf.Round(_motionSpeed * 100) / 100);
        }

        private void Rotate()
        {
            Vector3 inputDirection = new Vector3(Movement.x, 0.0f, Movement.y).normalized;

            if (Movement != Vector2.zero)
            {
                _cameraRotation = Mathf.Lerp(_cameraRotation, mainCamera.transform.eulerAngles.y, configScriptable.RotationSmoothTime);
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraRotation;
                _targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            }
            _rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, configScriptable.RotationSmoothTime);
            NewRotation = Quaternion.Euler(0.0f, _rotation, 0.0f);
        }

        private void WalkRun()
        {
            float targetSpeed = (Sprint ? configScriptable.SprintSpeed : configScriptable.WalkSpeed) * MoveMagnitude;
            _motionSpeed = 1;
            if (Movement == Vector2.zero)
                targetSpeed = 0.0f;
            else if (targetSpeed < configScriptable.SlowWalkSpeed)
            {
                targetSpeed = configScriptable.SlowWalkSpeed;
                _motionSpeed = MoveMagnitude * configScriptable.SlowWalkSpeed / configScriptable.WalkSpeed;
            }

            float currentHorizontalSpeed = new Vector2(Velocity.x, Velocity.z).magnitude;

            float speedOffset = 0.1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * MoveMagnitude, Time.deltaTime * configScriptable.SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
                _speed = targetSpeed;
            _animationSpeed = Mathf.Lerp(_animationSpeed, targetSpeed, Time.deltaTime * configScriptable.SpeedChangeRate);
            NewVelocity = _targetDirection.normalized * _speed + new Vector3(0, NewVelocity.y, 0);
        }
        #endregion
    }
}