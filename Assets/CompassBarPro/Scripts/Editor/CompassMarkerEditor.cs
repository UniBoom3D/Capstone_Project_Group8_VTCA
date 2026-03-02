using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// Custom editor script for Compass script
    /// </summary>
    //[CanEditMultipleObjects]
    [CustomEditor(typeof(CompassMarker))]
    public class CompassMarkerEditor : Editor
    {
        private CompassMarker _compassMarker;

        private SerializedProperty _prefabUI;

        private SerializedProperty _target;

        public void OnEnable()
        {
            _compassMarker = (CompassMarker)target;

            _prefabUI = serializedObject.FindProperty("_prefabUI");
            _target = serializedObject.FindProperty("_target");
        }

        public override void OnInspectorGUI()
        {
            _compassMarker._compass = (ICompassBarPro)EditorGUILayout.ObjectField("Compass", _compassMarker._compass, typeof(ICompassBarPro), true);

            EditorGUILayout.PropertyField(_prefabUI);
            _compassMarker._id = EditorGUILayout.TextField("Marker's id", _compassMarker._id);
            _compassMarker._addOnStart = EditorGUILayout.Toggle("Add on start", _compassMarker._addOnStart);

            EditorGUILayout.Space();
            _compassMarker._markerReference = (CompassMarker.MARKER_REFERENCE)EditorGUILayout.EnumPopup(new GUIContent("Marker type", "Reference of the marker:\n- Transform component in scene\n- Absolute heading value"), _compassMarker._markerReference);
            if (_compassMarker._markerReference == CompassMarker.MARKER_REFERENCE.TRANSFORM)
            {
                EditorGUILayout.PropertyField(_target);
            }
            else
            {
                _compassMarker._heading = Mathf.Clamp(EditorGUILayout.FloatField("Heading", _compassMarker._heading), 0, 360);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
