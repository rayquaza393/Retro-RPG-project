using UnityEngine;

namespace TPC
{
    [CreateAssetMenu(fileName = "characterCamera", menuName = "Scriptable TPC/Character Camera", order = 2)]
	public class CharacterScriptableCamera :  ScriptableObject
	{
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera")]
		public float CameraAngleOverride = 0.0f;
	}
}