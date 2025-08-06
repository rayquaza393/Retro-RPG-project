using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
    [RequireComponent(typeof(CharacterGravity))]
	[AddComponentMenu("Third Person Controller/Character Ground")]
	[DisallowMultipleComponent()]
	public class CharacterGround: CharacterExtension
	{
		#region Internal attributes and properties
		private CharacterGravity characterGravity;
        #endregion

        #region Methods
        public new void Awake()
        {
			base.Awake();

			characterGravity = GetComponent<CharacterGravity>();
        }

		protected override bool CheckConditions() => IsGrounded;

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

		protected override void Action()
		{
			characterGravity.SetGrounded?.Invoke(true);
			NewVelocity = new Vector3(NewVelocity.x, -2f, NewVelocity.z);
		}
        #endregion
    }
}