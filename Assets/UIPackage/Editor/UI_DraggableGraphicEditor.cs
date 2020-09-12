using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CustomUI.Utils;

namespace CustomUI.Editors
{
    [CustomEditor(typeof(UI_DraggableGraphic))]
    public class UI_DraggableGraphicEditor : Editor
    {
        private UI_DraggableGraphic _target;
        private Graphic _graphic;

        private SerializedProperty _targetRect;

        private SerializedProperty _dragConstraint;
        private SerializedProperty _dragPivot;
        private SerializedProperty _otherConstraint;

        private bool _eventsFoldout = false;
        private SerializedProperty _onDragBegin;
        private SerializedProperty _onDrag;
        private SerializedProperty _onDragEnd;


        private void OnEnable()
        {
            _target = target as UI_DraggableGraphic;
            _graphic = _target.GetComponent<Graphic>();

            _targetRect = serializedObject.FindProperty("targetRect");

            _dragConstraint = serializedObject.FindProperty("dragConstraint");
            _dragPivot = serializedObject.FindProperty("dragPivot");
            _otherConstraint = serializedObject.FindProperty("otherConstraint");

            _onDragBegin = serializedObject.FindProperty("onDragBegin");
            _onDrag = serializedObject.FindProperty("onDrag");
            _onDragEnd = serializedObject.FindProperty("onDragEnd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!_graphic)
                EditorGUILayout.HelpBox("Missing Graphic component! This component must be accompanied by a Graphic in order to register Pointer Events. Consider adding an Image component.", MessageType.Warning);

            EditorGUILayout.PropertyField(_targetRect, new GUIContent("Target Rect", "When dragging is active, this rect will become the main target to move."));
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(_dragConstraint, new GUIContent("Drag Constraint", "Determines the border-like restraints that could be applied to the \"Target Rect\". The target's edges will be prevented from going out of these bounds when dragging is active."));
            if ((Constraint)_dragConstraint.enumValueIndex == Constraint.Other)
            {

                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(_otherConstraint, new GUIContent("Other Constraint", "Creates a custom constraint boundaries using another Rect's position and sizes."));
                EditorGUI.indentLevel = 0;
                GUILayout.Space(10);
            }
            EditorGUILayout.PropertyField(_dragPivot, new GUIContent("Drag Pivot", "Determines the target's drag position in relation to pivot type."));

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            _eventsFoldout = UI_EditorUtility.ButtonFoldout(_eventsFoldout, "Events");
            if (_eventsFoldout)
            {
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(_onDragBegin, new GUIContent("On Drag Begin"));
                EditorGUILayout.PropertyField(_onDrag, new GUIContent("On Drag"));
                EditorGUILayout.PropertyField(_onDragEnd, new GUIContent("On Drag End"));
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

}