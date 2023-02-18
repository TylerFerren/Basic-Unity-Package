using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LevelingValue<>))]
public class LevelingValueDrawer : PropertyDrawer
{
    private bool show;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.width -= 48;
        show = EditorGUI.Foldout(position, show, "");
        EditorGUI.PropertyField(position, property.FindPropertyRelative("Value"), label);
        EditorGUI.PropertyField(new Rect(position.xMax + 3, position.yMin, 45, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("Level"), GUIContent.none);
        
        if (property != null && show)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));
            EditorGUI.indentLevel--;
        }

    }
}
