using System.Linq;
using UnityEditor;

namespace TPC
{
    [CustomEditor(typeof(CharacterManager))]
    public class CharacterManager_Editor : Editor
    {
        private CharacterManager _characterManager;

        private string[] Extensions => new string[] { "(select)" }.Concat(AuxiliarMethods.ExtensionNames).ToArray();

        private void OnEnable() => _characterManager = (CharacterManager)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AddExtension();

            EditorUtility.SetDirty(target);
        }

        private void AddExtension()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);
            int choiceExtension = EditorGUILayout.Popup("Add Extension", 0, Extensions);
            if (choiceExtension > 0)
                _characterManager.gameObject.AddComponent(AuxiliarMethods.ExtensionTypes[choiceExtension - 1]);
        }
    }
}