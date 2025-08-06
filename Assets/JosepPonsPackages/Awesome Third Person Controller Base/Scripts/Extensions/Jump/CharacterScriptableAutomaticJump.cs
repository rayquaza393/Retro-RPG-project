using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterAutomaticJump", menuName = "Scriptable TPC/Character Automatic Jump", order = 12)]
	public class CharacterScriptableAutomaticJump : ScriptableObject
	{
		[Tooltip("Min speed to perform a jump")]
		public float MinVelocity = 4f;
		[Tooltip("Height of the wall jumped vs the current jump height")]
		[Range(0f, 1f)]
		public float WallProportionalHeight = .9f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask Layers = -1;
	}
}