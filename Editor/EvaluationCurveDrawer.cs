using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Codesign
{
    [CustomPropertyDrawer(typeof(EvaluationCurve))]
    public class EvaluationCurveDrawer : PropertyDrawer
    {
        private bool show;
        private float width = 100;
        private float height = 200;
        private int margin = 25;

        private int curveResolution = 5;
        private SerializedProperty ExponetialGain;
        private SerializedProperty LinearGain;
        private SerializedProperty Floor;

        private float max;
        private float min;
        private float range;

        private GUIStyle containerStyle;
        private GUIStyle gridLabelStyle;
        private GUIStyle PointLabelStyle;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            ExponetialGain = property.FindPropertyRelative("ExponetialGain");
            LinearGain = property.FindPropertyRelative("LinearGain");
            Floor = property.FindPropertyRelative("Floor");
            Styles();

            EditorGUI.indentLevel ++;
            EditorGUILayout.BeginVertical(containerStyle);

            EditorGUILayout.BeginHorizontal();
                show = EditorGUILayout.Foldout(show, label, true);
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"(1){Evaluate(1)}   |   ({curveResolution}){Evaluate(curveResolution)}");
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            if (property != null && show) {
                EditorGUILayout.Space(5);
                width = EditorGUIUtility.currentViewWidth - 50;
                GraphVisual();

                max = MathF.Max(Evaluate(curveResolution), Evaluate(0));
                min = MathF.Min(Evaluate(curveResolution), Evaluate(0));
                range = MathF.Abs(Evaluate(curveResolution) - Evaluate(0));

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(Floor, new GUIContent("Floor"));
                EditorGUILayout.PropertyField(LinearGain, new GUIContent("Linear Gain"));
                EditorGUILayout.PropertyField(ExponetialGain, new GUIContent("Exponetial Gain"));
                curveResolution = EditorGUILayout.IntField(new GUIContent("Evaluate Up To"), curveResolution);
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        private void GraphVisual() {
            Rect layoutRectangle = GUILayoutUtility.GetRect(200, height);
            EditorGUI.DrawRect(layoutRectangle, new Color(0.18f, 0.18f, 0.18f));
            GUI.BeginClip(layoutRectangle);
            GridCalc();
            CurveCalc();
            GUI.EndClip();
        }

        private void GridCalc()
        {
            var gridCount = 5f;
            var gridVert = (height - (margin * 2)) / (gridCount);

            for (int i = 0; i <= gridCount; i++)
            {
                Handles.color = new Color(6, 6, 6);

                float verticalLocation = (height - margin) - (gridVert * i);
                float verticalValue = (range * (i/ gridCount)) + min;

                string gridLabel = max < 20 ? verticalValue.ToString("0.#") : MathF.Round(verticalValue).ToString();

                Handles.Label(new Vector2(10 + (margin / 2), verticalLocation - (gridLabelStyle.CalcSize(new GUIContent(gridLabel)).y/2)), gridLabel, gridLabelStyle);

                Handles.DrawAAPolyLine(Texture2D.grayTexture, 0.3f, new Vector3(20 + (margin / 2), verticalLocation, 0), new Vector3(width  - (margin / 2), verticalLocation, 0));
            }
        }

        private Vector3[] CurveCalc()
        {
            var widthMultiplier = (width - (margin * 2) - 30) / (curveResolution);
            var heightMultiplier = (height - (margin*2)) / range;
            var curvePoints = new List<Vector3>();

            for (int i = 0; i <= curveResolution; i++)
            {
                var j = Evaluate(i);
                curvePoints.Add(new Vector3(i * widthMultiplier + (margin) + 30, height - margin - (j * heightMultiplier) + (min * heightMultiplier), 0));
            }

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 1, curvePoints.ToArray());

            for (int i = 0; i < curvePoints.Count; i++)
            {
                Handles.DrawSolidDisc(curvePoints[i], Vector3.back, 3);
                Vector2 point = new Vector2(i, Evaluate(i));
                string label = point.x.ToString() + "/" + point.y.ToString("0.#");
                Handles.Label(new Vector2(curvePoints[i].x, curvePoints[i].y + 5), label, PointLabelStyle);
            }

            return curvePoints.ToArray();
        }

        public void Styles()
        {
            containerStyle = new GUIStyle(EditorStyles.helpBox);
            containerStyle.padding = new RectOffset(8, 8, 5, 5);

            gridLabelStyle = GUI.skin.label;
            gridLabelStyle.fontSize = 10;
            gridLabelStyle.alignment = TextAnchor.MiddleRight;

            PointLabelStyle = GUI.skin.label;
            PointLabelStyle.fontSize = 10;
            PointLabelStyle.alignment = TextAnchor.UpperCenter;
        }

        public float Evaluate(float x)
        {
            var y = Mathf.Pow(x * LinearGain.floatValue, ExponetialGain.floatValue) + Floor.floatValue;
            return y;
        }
    }
}

