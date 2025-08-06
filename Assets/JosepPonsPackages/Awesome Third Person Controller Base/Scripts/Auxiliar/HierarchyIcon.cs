#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace TPC
{
    [InitializeOnLoad]
    public static class HierarchyIcon
    {
        private static Type[] typeList = new Type[] {
            typeof(CharacterManager)
        };

        static HierarchyIcon() => EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;

        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
                return;

            foreach (Type type in typeList)
            {
                Component c = gameObject.GetComponent(type);
                if (c != null)
                {
                    Texture Icon = EditorGUIUtility.ObjectContent(c, c.GetType()).image;
                    if (Icon == null)
                        return;
                    float iconWidth = 15;
                    EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
                    var padding = new Vector2(5, 0);
                    var iconDrawRect = new Rect(rect.xMax - (iconWidth + padding.x), rect.yMin, rect.width, rect.height);
                    var iconGUIContent = new GUIContent(Icon);
                    EditorGUI.LabelField(iconDrawRect, iconGUIContent);
                    EditorGUIUtility.SetIconSize(Vector2.zero);
                }
            }
        }
    }
}
#endif