using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
	[RequireComponent(typeof(CharacterGravity))]
	[AddComponentMenu("Third Person Controller/Character Jump")]
	[DisallowMultipleComponent()]
	public class CharacterJump : CharacterExtension
	{
        #region Inspector
		[Space(10)]
		public CharacterScriptableJump configScriptable;

		[Space(10)]
		public UnityEvent SetJump;
		#endregion

		#region Properties
		public bool Jump
		{
			get => _jumpTime >= Time.time;
			set
			{
				if (Jump)
					return;

				_jumpTime = value ? Time.time + configScriptable.JumpTimeout : -1;
			}
		}
		#endregion

		#region Internal attributes and properties
		private CharacterScriptableGravity CharacterScriptableGravity => _characterGravity.configScriptable;

		private CharacterGravity _characterGravity;

		private float _jumpTime = -1;
        #endregion

        #region Methods
        protected new void Awake()
		{
			base.Awake();

			if (configScriptable == null)
				configScriptable = ScriptableObject.CreateInstance<CharacterScriptableJump>();

			_characterGravity = GetComponent<CharacterGravity>();
		}

		protected new void Start() => base.Start();

		protected override bool CheckConditions() => IsGrounded && Jump;

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

		protected override void Action()
		{
			NewVelocity = new Vector3(NewVelocity.x, Mathf.Sqrt(configScriptable.JumpHeight * -2f * CharacterScriptableGravity.Gravity), NewVelocity.z);
			SetJump?.Invoke();
		}
        #endregion
    }
}