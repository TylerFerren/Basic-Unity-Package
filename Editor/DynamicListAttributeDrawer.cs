using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Codesign {
    //[CustomPropertyDrawer(typeof(DynamicListAttribute))]
    public class DynamicListAttributeDrawer : PropertyDrawer
    {
        private SerializedProperty myListProp;
        private string[] typeOptions = { "Int", "Float", "String" };
        private int selectedTypeIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the type of the List from the attribute
            Type listType = fieldInfo.FieldType.GetGenericArguments()[0];

            // Get the current List object
            object listObject = fieldInfo.GetValue(property.serializedObject.targetObject);

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Draw the dropdown menu to select the list type
            position.width *= 0.5f;
            EditorGUI.LabelField(position, "Type");
            position.x += position.width;
            position.width *= 2f;
            //listType = EditorGUIUtility.ObjectField(position, listType, typeof(System.Object), false).GetType();

            // If the list type has changed, create a new List of the new type
            if (listObject != null && listType != listObject.GetType().GetGenericArguments()[0])
            {
                listObject = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
                fieldInfo.SetValue(property.serializedObject.targetObject, listObject);
            }

            EditorGUI.EndProperty();
        }
            
        private Type GetTypeFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return typeof(int);
                case 1:
                    return typeof(float);
                case 2:
                    return typeof(string);
                default:
                    return typeof(object);
            }
        }
        
    }



}
