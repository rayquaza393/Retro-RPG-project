using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
	[RequireComponent(typeof(CharacterJump))]
	[AddComponentMenu("Third Person Controller/Character Automatic Jump")]
	[DisallowMultipleComponent()]
	public class CharacterAutomaticJump : CharacterExtension
	{
		#region Inspector
		public CharacterScriptableAutomaticJump configScriptable;
		#endregion

		#region Internal attributes and properties
		private CharacterScriptableJump CharacterScriptableJump => _characterJump.configScriptable;
		private CharacterScriptableGravity CharacterScriptableGravity => _characterGravity.configScriptable;
		private Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0, Velocity.z);

		private CharacterJump _characterJump;
		private CharacterGravity _characterGravity;

		private Vector3 _gizmoPosition;
		private float _gizmoTime;
        #endregion

        #region Methods
        protected new void Awake()
		{
			base.Awake();
			_characterJump = GetComponent<CharacterJump>();
			_characterGravity = GetComponent<CharacterGravity>();

			if (configScriptable == null)
				configScriptable = ScriptableObject.CreateInstance<CharacterScriptableAutomaticJump>();
		}

		protected new void Start() => base.Start();

		protected override bool CheckConditions()
		{
			if (!IsGrounded || configScriptable.MinVelocity > HorizontalVelocity.magnitude)
				return false;
			float time = Mathf.Sqrt(-2 * CharacterScriptableJump.JumpHeight / CharacterScriptableGravity.Gravity);

			bool ret =
				Physics.CheckBox(transform.position + HorizontalVelocity * time + new Vector3(0, (Height + .1f) / 2, 0),
				new Vector3(2 * FloorRadius, Height - .1f, .1f) / 2, transform.rotation, configScriptable.Layers) &&
				Physics.CheckBox(transform.position + HorizontalVelocity * time + new Vector3(0, (CharacterScriptableJump.JumpHeight + .1f) / 2, 0),
				new Vector3(2 * FloorRadius, CharacterScriptableJump.JumpHeight - .1f, .1f) / 2, transform.rotation, configScriptable.Layers) &&
				!Physics.CheckBox(transform.position + HorizontalVelocity * time + new Vector3(0, CharacterScriptableJump.JumpHeight + Height / 2, 0),
				new Vector3(FloorRadius, Height / 2, HorizontalVelocity.magnitude * time), transform.rotation, configScriptable.Layers) &&
				!Physics.CheckBox(transform.position + HorizontalVelocity * time / 3 + new Vector3(0, (Height + .1f) / 2, 0),
				new Vector3(FloorRadius, Height / 2, HorizontalVelocity.magnitude * time / 3), transform.rotation, configScriptable.Layers) &&
				!Physics.CheckBox(transform.position + HorizontalVelocity * (time + time * 2 / 3) + new Vector3(0, (Height + .1f) / 2, 0),
				new Vector3(FloorRadius, Height / 2, HorizontalVelocity.magnitude * time / 3), transform.rotation, configScriptable.Layers);
			return ret;
		}

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

		protected override void Action() => _characterJump.Jump = true;

		private void OnDrawGizmosSelected()
        {
            if (_characterGravity == null || CharacterScriptableGravity == null || configScriptable == null || HorizontalVelocity.magnitude < configScriptable.MinVelocity)
                return;

            float time = Mathf.Sqrt(-2 * CharacterScriptableJump.JumpHeight / CharacterScriptableGravity.Gravity);
            if (IsGrounded)
            {
                _gizmoPosition = transform.position;
                _gizmoTime = -time;
            }

            Gizmos.color = Color.yellow;
            JumpLine(time);

            Gizmos.color = Color.green;
            DrawWiredCube(_gizmoPosition + HorizontalVelocity * time / 3 + new Vector3(0, CharacterScriptableJump.JumpHeight / 2, 0), new Vector3(2 * FloorRadius, CharacterScriptableJump.JumpHeight, HorizontalVelocity.magnitude * time * 2 / 3));
            DrawWiredCube(_gizmoPosition + HorizontalVelocity * (time + time * 2 / 3) + new Vector3(0, CharacterScriptableJump.JumpHeight / 2, 0), new Vector3(2 * FloorRadius, CharacterScriptableJump.JumpHeight, HorizontalVelocity.magnitude * time * 2 / 3));
            DrawWiredCube(_gizmoPosition + HorizontalVelocity * time + new Vector3(0, CharacterScriptableJump.JumpHeight + Height / 2, 0), new Vector3(2 * FloorRadius, Height, HorizontalVelocity.magnitude * time * 2));

            Gizmos.color = Color.red;
            if (Height < CharacterScriptableJump.JumpHeight)
                DrawWiredCube(_gizmoPosition + HorizontalVelocity * time + new Vector3(0, Height / 2, 0), new Vector3(2 * FloorRadius, Height, .1f));
            DrawWiredCube(_gizmoPosition + HorizontalVelocity * time + new Vector3(0, CharacterScriptableJump.JumpHeight * .45f, 0), new Vector3(2 * FloorRadius, CharacterScriptableJump.JumpHeight * .9f, .1f));
        }

        private void JumpLine(float time)
        {
            Vector3 lastPos = _gizmoPosition;
            float t = _gizmoTime;
            do
            {
                t += time / 10;
                if (t > time)
                    t = time;
                Vector3 newPos = _gizmoPosition + HorizontalVelocity * (time + t)
                    + new Vector3(0, CharacterScriptableJump.JumpHeight + (CharacterScriptableGravity.Gravity / 2) * t * t, 0);
                Gizmos.DrawLine(lastPos, newPos);
                lastPos = newPos;
            } while (t < time);
        }

        private void DrawWiredCube(Vector3 position, Vector3 size)
		{
			Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(HorizontalVelocity.normalized, Vector3.up), transform.lossyScale);
			Gizmos.DrawWireCube(Vector3.zero, size);
		}
		#endregion
	}
}