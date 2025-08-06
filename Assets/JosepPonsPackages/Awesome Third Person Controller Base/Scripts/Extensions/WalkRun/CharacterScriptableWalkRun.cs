using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterWalkRun", menuName = "Scriptable TPC/Character Walk Run", order = 10)]
	public class CharacterScriptableWalkRun : ScriptableObject
	{
		[Tooltip("Sprint speed of the character in m/s")]
		public float SlowWalkSpeed = 2f;
		[Tooltip("Move speed of the character in m/s")]
		public float WalkSpeed = 3.5f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.5f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 1f)]
		public float RotationSmoothTime = 0.5f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
	}
}