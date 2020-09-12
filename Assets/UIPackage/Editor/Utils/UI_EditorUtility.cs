using UnityEngine;
using UnityEditor;

namespace CustomUI.Editors 
{
    public class UI_EditorUtility
    {
        public static bool ButtonFoldout(bool foldout, string content)
        {
            Color color = GUI.color;
            Color contentColor = GUI.contentColor;
            GUI.color = foldout ? EditorGUIUtility.isProSkin ? color - new Color(0.6f, 0.6f, 0.6f, 0f) : color - new Color(0.15f, 0.15f, 0.15f, 0f) : color;
            GUI.contentColor = foldout ? EditorGUIUtility.isProSkin ? contentColor + new Color(0.55f, 0.55f, 0.55f, 0f) : contentColor + new Color(0.1f, 0.1f, 0.1f, 0f) : contentColor;
            if (GUILayout.Button(content))
                foldout = !foldout;
            GUI.color = color;
            GUI.contentColor = contentColor;
            return foldout;
        }
    }
}