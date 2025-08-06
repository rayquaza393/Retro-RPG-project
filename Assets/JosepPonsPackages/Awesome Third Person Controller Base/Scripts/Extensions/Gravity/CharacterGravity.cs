using System;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
    [RequireComponent(typeof(CharacterManager))]
    [AddComponentMenu("Third Person Controller/Character Gravity")]
    [DisallowMultipleComponent()]
    public class CharacterGravity : CharacterExtension
	{
        #region Inspector 
        [Space(10)]
		public CharacterScriptableGravity configScriptable;

        [Space(10)]
        public UnityEvent<bool> SetGrounded;
        #endregion

        #region Methods
        protected new void Awake()
        {
            base.Awake();

            if (configScriptable == null)
                configScriptable = ScriptableObject.CreateInstance<CharacterScriptableGravity>();
        }

        protected override bool CheckConditions() => !IsGrounded;

		protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

        protected override void Action()
		{
			SetGrounded?.Invoke(false);
			NewVelocity = new Vector3(NewVelocity.x, Velocity.y + configScriptable.Gravity * Time.deltaTime, NewVelocity.z);
		}
        #endregion
    }
}