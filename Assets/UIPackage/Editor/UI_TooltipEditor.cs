using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CustomUI;
using CustomUI.Utils;

[CustomEditor(typeof(UI_Tooltip))]
public class UI_TooltipEditor : Editor
{
    private UI_Tooltip _target = null;
    private Graphic _graphic;

    private SerializedProperty _content;
    private SerializedProperty _contentConstraint;
    private SerializedProperty _otherConstraint;
    private SerializedProperty _mirrorConstraint;

    private SerializedProperty _activationMethod;
    private SerializedProperty _displayPivot;
    private SerializedProperty _displayYield;
    private SerializedProperty _offsetFromPointer;

    private SerializedProperty _onTooltipBegin;
    private SerializedProperty _onTooltipStay;
    private SerializedProperty _onTooltipEnd;

    private void OnEnable() 
    {
        _target = target as UI_Tooltip;
        _graphic = _target.GetComponent<Graphic>();

        _content = serializedObject.FindProperty("content");
        _contentConstraint = serializedObject.FindProperty("contentConstraint");
        _otherConstraint = serializedObject.FindProperty("otherConstraint");
        _mirrorConstraint = serializedObject.FindProperty("mirrorConstraint");

        _activationMethod = serializedObject.FindProperty("activationMethod");
        _displayPivot = serializedObject.FindProperty("displayPivot");
        _displayYield = serializedObject.FindProperty("displayYield");
        _offsetFromPointer = serializedObject.FindProperty("offsetFromPointer");

        _onTooltipBegin = serializedObject.FindProperty("onTooltipBegin");
        _onTooltipStay = serializedObject.FindProperty("onTooltipStay");
        _onTooltipEnd = serializedObject.FindProperty("onTooltipEnd");

        SetContentPosition();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!_graphic)
            EditorGUILayout.HelpBox("Missing Graphic component! This component must be accompanied by a Graphic in order to register Pointer Events. Consider adding an Image component.", MessageType.Warning);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_content, new GUIContent("Content", "The selected rectTransform under \"Content\" will become the tooltip's target to display for when events are active."));
        GUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
            SetContentPosition();
        EditorGUILayout.PropertyField(_contentConstraint, new GUIContent("Content Constraint", "Determines the border-like restraints that could be applied to the \"Content\" rect. The target's edges will be prevented from going out of these bounds when the tooltip is active."));
        Constraint contentConstraint = _target.contentConstraint;
        if (contentConstraint == Constraint.Other)
        {

            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(_otherConstraint, new GUIContent("Other Constraint", "Creates a custom constraint boundaries using another rect's position and sizes."));
            EditorGUI.indentLevel = 0;
        }
        if (contentConstraint != Constraint.None) 
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(_mirrorConstraint, new GUIContent("Mirror Constraint", "When the \"Content\" rect is out of bounds, then the tooltip will attempt to mirror the rect position following this constraint."));
            EditorGUI.indentLevel = 0;
            GUILayout.Space(10);
        }

        EditorGUILayout.PropertyField(_activationMethod, new GUIContent("Activation Method", "Controls the pointer events that display the \"Content\" rect."));
        EditorGUILayout.PropertyField(_displayPivot, new GUIContent("Display Pivot", "Determines the \"Content\" position when the tooltip is active"));
        EditorGUILayout.PropertyField(_displayYield, new GUIContent("Display Yield", "Time yielded between tooltip begin and tooltip stay"));
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_offsetFromPointer, new GUIContent("Offset From Pointer", "Sets an offset to the \"Display Pivot\" position in relation to current screen resolution."));
        if (EditorGUI.EndChangeCheck())
            SetContentPosition();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(_onTooltipBegin, new GUIContent("On Tooltip Begin"));
        EditorGUILayout.PropertyField(_onTooltipStay, new GUIContent("On Tooltip Stay"));
        EditorGUILayout.PropertyField(_onTooltipEnd, new GUIContent("On Tooltip End"));
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private void SetContentPosition()
    {
        serializedObject.ApplyModifiedProperties();
        if (!_target.content)
            return;

        UI_Projection offsetProjection = new UI_Projection(_target.content, _target.transform.position + _target.OffsetPositionFromScreen(new Vector3(_target.offsetFromPointer.x, _target.offsetFromPointer.y)));
        _target.content.transform.position = offsetProjection.WorldPosition;
    }
}
