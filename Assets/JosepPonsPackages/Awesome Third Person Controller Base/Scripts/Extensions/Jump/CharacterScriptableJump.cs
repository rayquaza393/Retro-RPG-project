using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterJump", menuName = "Scriptable TPC/Character jump", order = 12)]
	public class CharacterScriptableJump : ScriptableObject
	{
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
	}
}