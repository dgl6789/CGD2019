using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using App;

[CustomEditor(typeof(ToolItem))]
public class ToolItemEditor : Editor {
    public SerializedProperty 
        type, 
        inputType, 
        power, 
        breakRadius, 
        sustainedBreakCooldown;

    void OnEnable()
    {
        // Setup the SerializedProperties
        type = serializedObject.FindProperty("type");
        inputType = serializedObject.FindProperty("inputType");
        power = serializedObject.FindProperty("power");
        breakRadius = serializedObject.FindProperty("breakRadius");
        sustainedBreakCooldown = serializedObject.FindProperty("sustainedBreakCooldown");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(inputType);
        EditorGUILayout.PropertyField(power);

        ToolType toolType = (ToolType)type.enumValueIndex;
        ToolInputType toolInputType = (ToolInputType)inputType.enumValueIndex;

        // Draw property fields based on which should be shown
        // given the tool and input enum types.
        switch (toolType)
        {
            case ToolType.AREA:
                EditorGUILayout.PropertyField(breakRadius, new GUIContent("Break Radius"));
                break;
        }

        switch (toolInputType)
        {
            case ToolInputType.SUSTAINED:
                EditorGUILayout.PropertyField(sustainedBreakCooldown, new GUIContent("Sustained Break Cooldown"));
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }
}
