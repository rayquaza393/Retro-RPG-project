using System;
using UnityEngine;

namespace TPC
{
	[RequireComponent(typeof(CharacterManager))]
	[AddComponentMenu("Third Person Controller/Character Camera")]
	[DisallowMultipleComponent()]
	public class CharacterCamera : CharacterExtension
	{
        #region Constants
        private const float THRESHOLD = 0.01f;
        #endregion

        #region Inspector 
        [Space(10)]
		[Tooltip("General config values for the camera")]
		public CharacterScriptableCamera configScriptable;
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
        #endregion

        #region Properties
        public bool CurrentDeviceMouse { get; set; }
		public Vector2 Look { get; set; }
        #endregion

        #region Internal attributes and properties
        private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;
        #endregion

        #region Methods
        protected override bool CheckConditions() => true;

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

		protected override void Action()
		{
			if (Look.sqrMagnitude >= THRESHOLD)
			{
				float deltaTimeMultiplier = CurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetYaw += Look.x * deltaTimeMultiplier;
				_cinemachineTargetPitch += Look.y * deltaTimeMultiplier;
			}

			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, configScriptable.BottomClamp, configScriptable.TopClamp);

			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + configScriptable.CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
        #endregion
    }
}