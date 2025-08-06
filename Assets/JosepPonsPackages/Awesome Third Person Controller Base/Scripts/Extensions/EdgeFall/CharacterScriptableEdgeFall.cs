using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterEdgeFall", menuName = "Scriptable TPC/Character Edge Fall", order = 10)]
	public class CharacterScriptableEdgeFall : ScriptableObject
	{
		[Tooltip("Angle where we begin to slide")]
		public float SlideAngle = 20f;
		[Tooltip("Slide Speed")]
		public float SlideSpeed = 1f;
	}
}