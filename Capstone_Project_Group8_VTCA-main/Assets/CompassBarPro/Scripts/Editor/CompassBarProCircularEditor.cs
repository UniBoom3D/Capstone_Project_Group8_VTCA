using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace LunarCatsStudio.Compass
{

    /// <summary>
    /// Custom editor script for Circular Compass script
    /// </summary>
    [CustomEditor(typeof(CompassBarProCircular))]
    public class CompassBarProCircularEditor : Editor
    {
        private SerializedProperty _headingPointer;
        private SerializedProperty _updateMode;
        // private SerializedProperty _graduationPrefab;
        private SerializedProperty _headingIndicator;
        private SerializedProperty _markersParent;
        private SerializedProperty _defaultMarkerPrefab;
        private SerializedProperty _showOutOfRangeMarkers;

        private SerializedProperty _adjustMarkerRotation;
        private SerializedProperty _zoomLevel;
        private SerializedProperty _northTransform;
        private SerializedProperty _northVector;
        // private SerializedProperty _graduationPrecision;
        // private SerializedProperty _hasGraduations;
        // private SerializedProperty _hasCardinalPointIndication;
        private SerializedProperty _cardinalsTextParent;
        private SerializedProperty _cardinalsImage;
        private SerializedProperty _adjustTextRotation;

        private CompassBarProCircular _compassBarPro;

        protected void OnEnable()
        {
            _headingPointer = serializedObject.FindProperty("_headingPointer");
            _updateMode = serializedObject.FindProperty("_updateMode");
            // _graduationPrefab = serializedObject.FindProperty("_graduationPrefab");
            _headingIndicator = serializedObject.FindProperty("_headingIndicator");
            _markersParent = serializedObject.FindProperty("_markersParent");
            _defaultMarkerPrefab = serializedObject.FindProperty("_defaultMarkerPrefab");
            _showOutOfRangeMarkers = serializedObject.FindProperty("_showOutOfRangeMarkers");
            _adjustMarkerRotation = serializedObject.FindProperty("_adjustMarkerRotation");
            _zoomLevel = serializedObject.FindProperty("_zoomLevel");
            _northTransform = serializedObject.FindProperty("_northTransform");
            _northVector = serializedObject.FindProperty("_northVector");
            // _graduationPrecision = serializedObject.FindProperty("_graduationPrecision");
            // _hasGraduations = serializedObject.FindProperty("_hasGraduations");
            // _hasCardinalPointIndication = serializedObject.FindProperty("_hasCardinalPointIndication");
            _cardinalsImage = serializedObject.FindProperty("_cardinalsImage");
            _cardinalsTextParent = serializedObject.FindProperty("_cardinalsTextParent");
            _adjustTextRotation = serializedObject.FindProperty("_adjustTextRotation");

            _compassBarPro = (CompassBarProCircular)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("General parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_headingPointer);
            EditorGUILayout.PropertyField(_headingIndicator);
            _compassBarPro._rotatingPart = (CompassBarProCircular.UPDATE_MODE)EditorGUILayout.EnumPopup(new GUIContent("Rotating part", "Select wich part of the compass will be rotating when heading updates: compass imgae or pointer needle"), _compassBarPro._rotatingPart);
            EditorGUILayout.PropertyField(_zoomLevel);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Markers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_markersParent);
            EditorGUILayout.PropertyField(_defaultMarkerPrefab);
            EditorGUILayout.PropertyField(_showOutOfRangeMarkers);
            if (_compassBarPro._rotatingPart == CompassBarProCircular.UPDATE_MODE.CARDINAL_IMAGE)
            {
                EditorGUILayout.PropertyField(_adjustMarkerRotation);
            }


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

            GUILayout.Label("Cardinals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_cardinalsImage);
            EditorGUILayout.PropertyField(_cardinalsTextParent);
            if (_compassBarPro._cardinalsTextParent != null)
            {
                _compassBarPro._cardinalIndicatorDetail = (CompassBarPro.CARDINAL_INDICATOR_DETAIL)EditorGUILayout.EnumPopup(new GUIContent("Cardinal indicator detail", "set detail level for cardinal indicators"), _compassBarPro._cardinalIndicatorDetail);
                if (_compassBarPro._cardinalIndicatorDetail != CompassBarPro.CARDINAL_INDICATOR_DETAIL.NONE)
                {
                    EditorGUILayout.PropertyField(_adjustTextRotation);
                }
            }

            GUILayout.Space(10);


            // EditorGUILayout.PropertyField(_hasGraduations, new GUIContent("Has graduation ?", "Set if to true if you want to have graduation on compass"));
            // if (_compassBarPro._hasGraduations)
            // {
            //     EditorGUI.indentLevel++;
            //     EditorGUILayout.PropertyField(_graduationPrefab);
            //     EditorGUILayout.PropertyField(_graduationPrecision);
            //     EditorGUI.indentLevel--;
            // }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                _compassBarPro.BuildCompass(true);
            }
        }
    }
}