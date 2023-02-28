using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Codesign
{
    [CustomPropertyDrawer(typeof(EvaluationCurve))]
    public class EvaluationCurveDrawer : PropertyDrawer
    {
        private bool show;

        [UnityEngine.Range(2, 250)] private int curveResolution = 50;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            show = EditorGUI.Foldout(rect, show, label);
            if (show)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("Floor"), new GUIContent("Floor"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("LinearGain"), new GUIContent("Linear Gain"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ExponetialGain"), new GUIContent("Exponetial Gain"));
                EditorGUI.indentLevel--;
            }
        }

        public Rect GetNextPosition(Rect rect, int position) {
            Rect nextPosition = rect;
            nextPosition.y += EditorGUIUtility.singleLineHeight * position;
            return nextPosition;
        }

        private float ExponetialGain;
        private float LinearGain;
        private float Floor;


        private void v1Drawer(SerializedProperty property)
        {
            ExponetialGain = property.FindPropertyRelative("ExponetialGain").floatValue;
            LinearGain = property.FindPropertyRelative("LinearGain").floatValue;
            Floor = property.FindPropertyRelative("Floor").floatValue;

            EditorGUILayout.BeginHorizontal();

            var rectWidth = EditorGUIUtility.currentViewWidth * 0.5f;
            var rectHeight = 200;

            Rect layoutRectangle = GUILayoutUtility.GetRect(rectWidth, rectHeight);
            EditorGUI.DrawRect(layoutRectangle, new Color(0.2f, 0.2f, 0.2f));
            GUI.BeginClip(layoutRectangle);

            GridCalc(rectWidth, rectHeight);

            var extent = Evaluate(curveResolution);

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                2,
                CurveCalc(rectWidth, rectHeight)
                );
            GUI.EndClip();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ExponetialGain"), new GUIContent("Exponetial Gain"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("LinearGain"), new GUIContent("Linear Gain"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Floor"), new GUIContent("Floor"));
            curveResolution = EditorGUILayout.IntField(new GUIContent("Evaluate Up To"), curveResolution);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);
        }

        private Vector3[] CurveCalc(float width, float height)
        {
            var curvePoints = new List<Vector3>();
            var widthMultiplier = width / curveResolution;
            var extent = Evaluate(curveResolution);
            var heightMultiplier = height / extent;
            for (int i = 0; i < curveResolution; i++)
            {
                var j = Evaluate(i);
                curvePoints.Add(new Vector3(i * widthMultiplier, height - j * heightMultiplier, 0));
            }

            GUI.Label(new Rect(curvePoints[1].x + 10, curvePoints[1].y - 5, 50, 15), new Vector2Int(1, Mathf.RoundToInt(Evaluate(1))).ToString());

            Handles.DrawSolidDisc(curvePoints[1], Vector3.back, 5);

            Handles.DrawSolidDisc(curvePoints.LastOrDefault(), Vector3.back, 5);
            Handles.Label(curvePoints.LastOrDefault() - new Vector3(50, -5, 0), new Vector2Int(curveResolution, Mathf.RoundToInt(extent)).ToString());
            return curvePoints.ToArray();
        }

        private void GridCalc(float width, float height)
        {
            var extent = Evaluate(curveResolution);
            var heightMultiplier = height / extent;
            if (extent < 300)
            {
                var gridVert = 10 * heightMultiplier;
                for (int i = 0; i < 300 / gridVert; i++)
                {
                    Handles.color = new Color(50, 50, 50);
                    Handles.Label(new Vector3(5, height - gridVert * i), (i * 10).ToString());
                    Handles.DrawAAPolyLine(
                       Texture2D.whiteTexture,
                       0.3f,
                       new Vector3(30, height - gridVert * i, 0),
                       new Vector3(width, height - gridVert * i, 0)
                       );
                }
            }
            if (100 < extent && extent < 3000)
            {
                var gridVert = 100 * heightMultiplier;
                for (int i = 0; i < 3000 / gridVert; i++)
                {
                    Handles.DrawAAPolyLine(
                       Texture2D.whiteTexture,
                       1,
                       new Vector3(0, height - gridVert * i, 0),
                       new Vector3(width, height - gridVert * i, 0)
                       );
                }
            }


        }

        public float Evaluate(float x)
        {
            var y = Mathf.Pow(Mathf.Abs(x * LinearGain - 1), ExponetialGain) + Floor;
            return y;
        }
    }
}

