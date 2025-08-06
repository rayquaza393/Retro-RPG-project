using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterGravity", menuName = "Scriptable TPC/Character gravity", order = 11)]
	public class CharacterScriptableGravity : ScriptableObject
	{
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;
	}
}