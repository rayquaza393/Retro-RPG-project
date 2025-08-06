#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TPC
{
    [CustomPropertyDrawer(typeof(ExtensionNameAttribute))]
    public class ExtensionNamePropertyDrawer : PropertyDrawer
    {
        private string[] Extensions => new string[] { "(null)" }.Concat(AuxiliarMethods.ExtensionNames).ToArray();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = Array.IndexOf(Extensions, property.stringValue);
            if (index < 0)
                index = 0;

            index = EditorGUI.Popup(position, label.text, index, Extensions);
            property.stringValue = Extensions[index];
        }
    }
}
#endif