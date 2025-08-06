#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TPC
{
    public class DefaultIncompatibilites : ScriptableObject
    {
        public string[] CharacterCamera = new string[] { };
        public string[] CharacterEdgeFall = new string[] { "TPC.CharacterWalkRun" };
        public string[] CharacterFreeFall = new string[] { };
        public string[] CharacterGravity = new string[] { "TPC.CharacterWalkRun" };
        public string[] CharacterGround = new string[] { };
        public string[] CharacterAutomaticJump = new string[] { };
        public string[] CharacterJump = new string[] { "TPC.CharacterGravity", "TPC.CharacterGround" };
        public string[] CharacterWalkRun = new string[] { };

        public string[] GetIncompatibilitiesOf(System.Type type) => type.ToString() switch
        {
            "TPC.CharacterCamera" => CharacterCamera,
            "TPC.CharacterEdgeFall" => CharacterEdgeFall,
            "TPC.CharacterFreeFall" => CharacterFreeFall,
            "TPC.CharacterGravity" => CharacterGravity,
            "TPC.CharacterGround" => CharacterGround,
            "TPC.CharacterAutomaticJump" => CharacterAutomaticJump,
            "TPC.CharacterJump" => CharacterJump,
            _ => null,
        };
    }

    [InitializeOnLoad]
    public static class InitIncompatibilities
    {
        static InitIncompatibilities()
        {
            string[] tmp = AssetDatabase.FindAssets("t: DefaultIncompatibilites");

            if (tmp.Length != 0)
                return;

            DefaultIncompatibilites asset = ScriptableObject.CreateInstance<DefaultIncompatibilites>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(asset, "Assets/Resources/TPCDefaultIncompatibilites.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
        }
    }
}
#endif