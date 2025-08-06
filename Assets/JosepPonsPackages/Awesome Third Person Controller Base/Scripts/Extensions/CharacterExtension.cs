using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TPC
{
    [RequireComponent(typeof(CharacterManager))]
	public abstract class CharacterExtension : MonoBehaviour
	{
		[ExtensionName]
		public string[] incompatibleExtensions;

		protected CharacterManager thirdPersonPlayer;
		protected GameObject mainCamera;
		protected Type[] incompatibleExtensionTypes;

		protected Vector3 Velocity => thirdPersonPlayer.Velocity;
		protected Vector3 NewVelocity { get => thirdPersonPlayer.NewVelocity; set => thirdPersonPlayer.NewVelocity = value; }
        protected Quaternion NewRotation { get => thirdPersonPlayer.NewRotation; set => thirdPersonPlayer.NewRotation = value; }
		protected float FloorRadius => thirdPersonPlayer.FloorRadius;
		protected float Height => thirdPersonPlayer.Height;
		protected bool IsGrounded => thirdPersonPlayer.IsGrounded;

#if UNITY_EDITOR
		private void Reset()
        {
			string[] items = AssetDatabase.FindAssets("t: DefaultIncompatibilites");
			if (items.Length == 0)
				return;
			DefaultIncompatibilites defaultIncompatibilites = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(items[0]), typeof(DefaultIncompatibilites)) as DefaultIncompatibilites;
			incompatibleExtensions = defaultIncompatibilites.GetIncompatibilitiesOf(GetType());
		}
#endif

		protected void Awake()
        {
            thirdPersonPlayer = GetComponent<CharacterManager>();
			mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

			incompatibleExtensionTypes = incompatibleExtensions == null ? new Type[] { } : incompatibleExtensions.Select(x => Type.GetType(x)).ToArray();
		}

        protected void Start()
        {
			thirdPersonPlayer.CheckConditions += CheckConditions;
			thirdPersonPlayer.RecoverIncompatible += RecoverIncompatible;
			thirdPersonPlayer.DoAction += Action;
        }

        protected void OnDestroy()
        {
			thirdPersonPlayer.CheckConditions -= CheckConditions;
			thirdPersonPlayer.RecoverIncompatible -= RecoverIncompatible;
			thirdPersonPlayer.DoAction -= Action;
        }		

		protected abstract bool CheckConditions();

		protected abstract Type[] RecoverIncompatible();

		protected abstract void Action();
    }
}