using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using App;

[CustomEditor(typeof(ToolItem))]
public class ToolItemEditor : Editor {
    public SerializedProperty
        itemName,
        itemText,
        itemSprite,
        itemValue,
        type, 
        inputType, 
        power, 
        breakRadius, 
        sustainedBreakCooldown;

    void OnEnable() {
        // Setup the SerializedProperties
        itemName = serializedObject.FindProperty("itemName");
        itemText = serializedObject.FindProperty("itemText");
        itemSprite = serializedObject.FindProperty("itemSprite");
        itemValue = serializedObject.FindProperty("value");
        type = serializedObject.FindProperty("type");
        inputType = serializedObject.FindProperty("inputType");
        power = serializedObject.FindProperty("power");
        breakRadius = serializedObject.FindProperty("breakRadius");
        sustainedBreakCooldown = serializedObject.FindProperty("sustainedBreakCooldown");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(itemName);
        EditorGUILayout.PropertyField(itemText);
        EditorGUILayout.PropertyField(itemSprite);
        EditorGUILayout.PropertyField(itemValue);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(type);

        ToolType toolType = (ToolType)type.enumValueIndex;
        ToolInputType toolInputType = (ToolInputType)inputType.enumValueIndex;

        // Draw property fields based on which should be shown
        // given the tool and input enum types.
        switch (toolType) {
            case ToolType.POINT:
                EditorGUILayout.PropertyField(inputType);
                EditorGUILayout.PropertyField(power, new GUIContent("Power"));
                break;
            case ToolType.AREA:
                EditorGUILayout.PropertyField(inputType);
                EditorGUILayout.PropertyField(power, new GUIContent("Power"));
                EditorGUILayout.PropertyField(breakRadius, new GUIContent("Break Radius"));
                break;
            case ToolType.CHISEL:

                break;
        }

        switch (toolInputType) {
            case ToolInputType.SUSTAINED:
                EditorGUILayout.PropertyField(sustainedBreakCooldown, new GUIContent("Sustained Break Cooldown"));
                break;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
