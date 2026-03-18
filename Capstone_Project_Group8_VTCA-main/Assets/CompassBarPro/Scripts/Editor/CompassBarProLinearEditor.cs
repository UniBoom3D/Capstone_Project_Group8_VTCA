using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace LunarCatsStudio.Compass
{

    /// <summary>
    /// Custom editor script for Linear Compass script
    /// </summary>
    [CustomEditor(typeof(CompassBarProLinear))]
    public class CompassBarProLinearEditor : MaskEditor
    {
        private SerializedProperty _maskTexture;
        private SerializedProperty _graduationParent;
        private SerializedProperty _graduationPrefab;
        private SerializedProperty _indicatorSclaeFactor;
        private SerializedProperty _headingIndicator;
        private SerializedProperty _markersParent;
        private SerializedProperty _defaultMarkerPrefab;
        private SerializedProperty _defaultMarkerPosY;
        private SerializedProperty _visibleCovertureAngle;
        private SerializedProperty _northTransform;
        private SerializedProperty _northVector;
        private SerializedProperty _graduationPrecision;
        private SerializedProperty _hasGraduations;
        private SerializedProperty _hasCardinalPointIndication;
        private CompassBarProLinear _compassBarPro;

        protected override void OnEnable()
        {
            _maskTexture = serializedObject.FindProperty("_maskTexture");
            _graduationParent = serializedObject.FindProperty("_graduationParent");
            _graduationPrefab = serializedObject.FindProperty("_graduationPrefab");
            _indicatorSclaeFactor = serializedObject.FindProperty("_indicatorSclaeFactor");
            _headingIndicator = serializedObject.FindProperty("_headingIndicator");
            _markersParent = serializedObject.FindProperty("_markersParent");
            _defaultMarkerPrefab = serializedObject.FindProperty("_defaultMarkerPrefab");
            _defaultMarkerPosY = serializedObject.FindProperty("_defaultMarkerPosY");
            _visibleCovertureAngle = serializedObject.FindProperty("_visibleCovertureAngle");
            _northTransform = serializedObject.FindProperty("_northTransform");
            _northVector = serializedObject.FindProperty("_northVector");
            _graduationPrecision = serializedObject.FindProperty("_graduationPrecision");
            _hasGraduations = serializedObject.FindProperty("_hasGraduations");
            _hasCardinalPointIndication = serializedObject.FindProperty("_hasCardinalPointIndication");

            _compassBarPro = (CompassBarProLinear)target;
        }

        public override void OnInspectorGUI()
        {
            if (_compassBarPro.graphic && !_compassBarPro.graphic.IsActive())
            {
                EditorGUILayout.HelpBox("Masking disabled due to Graphic component being disabled.", MessageType.Warning);
            }
            serializedObject.Update();

            EditorGUILayout.LabelField("General parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_maskTexture);
            EditorGUILayout.PropertyField(_headingIndicator);
            EditorGUILayout.PropertyField(_visibleCovertureAngle);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Markers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_markersParent);
            EditorGUILayout.PropertyField(_defaultMarkerPrefab);
            EditorGUILayout.PropertyField(_defaultMarkerPosY);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("North type", EditorStyles.boldLabel);
            _compassBarPro._northType = (CompassBarPro.NORTH_TYPE)EditorGUILayout.EnumPopup(new GUIContent("North type", "Definition of the north type"), _compassBarPro._northType);
            if (_compassBarPro._northType == CompassBarPro.NORTH_TYPE.TRANSFORM)
            {
                EditorGUILayout.PropertyField(_northTransform);
            }
            else
            {
                EditorGUILayout.PropertyField(_northVector);
            }

            GUILayout.Space(10);

            GUILayout.Label("Graduations", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_graduationParent);
            EditorGUILayout.PropertyField(_hasGraduations, new GUIContent("Has graduation ?", "Set if to true if you want to have graduation on compass"));
            if (_compassBarPro._hasGraduations)
            {
                _compassBarPro._cardinalIndicatorDetail = (CompassBarPro.CARDINAL_INDICATOR_DETAIL)EditorGUILayout.EnumPopup(new GUIContent("Cardinal indicator detail", "set detail level for cardinal indicators"), _compassBarPro._cardinalIndicatorDetail);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_graduationPrefab);
                EditorGUILayout.PropertyField(_indicatorSclaeFactor);
                EditorGUILayout.PropertyField(_graduationPrecision);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                _compassBarPro.BuildCompass(true);
                _compassBarPro.ComputeMask();
            }

            if (GUILayout.Button(new GUIContent("Build", "Build the compass with actual settings")))
            {
                _compassBarPro.BuildCompass(false);
                _compassBarPro.ComputeMask();
                _compassBarPro.SetHeading(_compassBarPro._heading);
            }
        }
    }
}