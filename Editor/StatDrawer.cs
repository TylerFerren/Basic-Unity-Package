using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Codesign
{
    [CustomPropertyDrawer(typeof(Stat))]
    public class StatDrawer : PropertyDrawer
    {
        private bool show;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.width -= 72;

            int index = property.FindPropertyRelative("category").intValue;
            int returnedIndex = EditorGUI.Popup(position, "Stat Category", index, StatManager.StatCategories);
            property.FindPropertyRelative("category").intValue = returnedIndex;

            Rect CurrentRect = new Rect(position.x + position.width + 2, position.y, 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(CurrentRect, property.FindPropertyRelative("currentValue"), GUIContent.none);

            Rect SlashRect = new Rect(CurrentRect.x + CurrentRect.width + 2, position.y, 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(SlashRect, "/");

            Rect MaxRect = new Rect(SlashRect.x + SlashRect.width + 2, position.y, 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(MaxRect, property.FindPropertyRelative("maxValue"), GUIContent.none);
            
            // Get the list of custom objects from the serialized property
            SerializedProperty levelingValues = property.FindPropertyRelative("levelingValues");

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty Values = property.FindPropertyRelative("values");

            // Draw the list using a for loop
            for (int i = 0; i < levelingValues.arraySize; i++)
            {
                SerializedProperty levelingValue = levelingValues.GetArrayElementAtIndex(i);
                
                // Create a label for this element based on its name property
                var valueLabel = new GUIContent("Value");
                // Draw the element using the custom property drawer for the custom class
                EditorGUI.PropertyField(position, levelingValue, valueLabel);

                // Move the position down for the next element
                position.y += EditorGUI.GetPropertyHeight(levelingValue, true) + EditorGUIUtility.standardVerticalSpacing;
                position.height += EditorGUI.GetPropertyHeight(levelingValue, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.EndProperty();
            ////if (label.text == "") label.text = property.displayName;
            //var labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            //show = EditorGUI.Foldout(labelPosition, show, label);

            //var valuePosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, (position.width - labelPosition.width - 3) * 0.6666f, EditorGUIUtility.singleLineHeight);
            //EditorGUI.PropertyField(valuePosition, property.FindPropertyRelative("Value"), GUIContent.none);

            //var levelPosition = new Rect(valuePosition.x + valuePosition.width + 2, position.y, valuePosition.width / 2, EditorGUIUtility.singleLineHeight);
            //EditorGUI.PropertyField(levelPosition, property.FindPropertyRelative("Level"), GUIContent.none);

            //if (property != null && show)
            //{
            //    EditorGUI.indentLevel++;
            //    EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));

            //    int index = property.FindPropertyRelative("Category").intValue;
            //    int returnedIndex = EditorGUILayout.Popup("Stat Leveling Category", index, StatManager.StatCategories);
            //    property.FindPropertyRelative("Category").intValue = returnedIndex;
            //    EditorGUI.indentLevel--;
            //}

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            int lineCount = 1 + property.FindPropertyRelative("levelingValues").arraySize;
            return EditorGUIUtility.singleLineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);
        }

    }
}