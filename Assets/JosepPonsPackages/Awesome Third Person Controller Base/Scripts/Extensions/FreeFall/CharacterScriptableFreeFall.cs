using UnityEngine;

namespace TPC
{
	[CreateAssetMenu(fileName = "characterFreeFall", menuName = "Scriptable TPC/Character free fall", order = 12)]
	public class CharacterScriptableFreeFall : ScriptableObject
	{
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;
	}
}