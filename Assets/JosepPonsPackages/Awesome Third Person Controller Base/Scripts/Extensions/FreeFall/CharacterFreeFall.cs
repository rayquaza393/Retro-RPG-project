using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
	[RequireComponent(typeof(CharacterGravity))]
	[AddComponentMenu("Third Person Controller/Character Free Fall")]
	[DisallowMultipleComponent()]
	public class CharacterFreeFall : CharacterExtension
	{
        #region Inspector
        [Space(10)]
		public CharacterScriptableFreeFall configScriptable;

		[Space(10)]
		public UnityEvent<bool> SetFreeFall;
		#endregion

		#region Internal attributes and properties
		private float _fallTimeoutDelta;
        #endregion

        #region Methods
        private new void Start()
		{
			base.Start();
			_fallTimeoutDelta = configScriptable.FallTimeout;
		}

		protected override bool CheckConditions() => true;

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

		protected override void Action()
		{
			if (IsGrounded)
				_fallTimeoutDelta = configScriptable.FallTimeout;
			else
				_fallTimeoutDelta -= Time.deltaTime;

			SetFreeFall?.Invoke(!IsGrounded && _fallTimeoutDelta < 0.0f);
		}
        #endregion
    }
}