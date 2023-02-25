using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Codesign
{
    [CustomPropertyDrawer(typeof(LevelingValue<>))]
    public class LevelingValueDrawer : PropertyDrawer
    {
        private bool show;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //if (label.text == "") label.text = property.displayName;

            var labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            show = EditorGUI.Foldout(labelPosition, show, property.displayName);

            var valuePosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, (position.width - labelPosition.width - 3) * 0.6666f , EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(valuePosition, property.FindPropertyRelative("Value"), GUIContent.none);

            var levelPosition = new Rect(valuePosition.x + valuePosition.width + 2, position.y, valuePosition.width/2, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(levelPosition, property.FindPropertyRelative("Level"), GUIContent.none);

            if (property != null && show)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));

                int index = property.FindPropertyRelative("Category").intValue;
                int returnedIndex = EditorGUILayout.Popup("Stat Leveling Category", index , StatManager.StatCategories);
                property.FindPropertyRelative("Category").intValue = returnedIndex;
                EditorGUI.indentLevel--;
            }

        }
    }
}
