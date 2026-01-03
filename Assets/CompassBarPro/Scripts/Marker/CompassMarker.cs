using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// A simple helper script that adds amarker to the compass
    /// </summary>
    public class CompassMarker : MonoBehaviour
    {

        public enum MARKER_REFERENCE { TRANSFORM, ABSOLUTE_HEADING }

        /// <summary>
        /// Reference to the Compass script
        /// </summary>
        public ICompassBarPro _compass;

        /// <summary>
        /// The UI prefab of this marker
        /// </summary>
        public GameObject _prefabUI;

        /// <summary>
        /// Reference to the transform of the Marker.null If null, the transform of the GameObject to which this script is attached to will be taken into account.
        /// In case of TRANSFORM type
        /// </summary>
        [Tooltip("The Transform component of the object to mark in the scene. If null, the transform of this GameObject will be taken into account")]
        public Transform _target;

        /// <summary>
        /// The heading value of the marker in case of ABSOLUTE_HEADING type
        /// </summary>
        [Range(0, 360)]
        public float _heading = 0;

        /// <summary>
        /// The id of the marker
        /// </summary>
        public string _id;

        /// <summary>
        /// If set to true, this marker will be added to the compass at startup
        /// </summary>
        public bool _addOnStart = true;

        /// <summary>
        /// The marker reference: Is it a GameObject in the scene or an absolute heading value ?
        /// </summary>
        public MARKER_REFERENCE _markerReference = MARKER_REFERENCE.TRANSFORM;

        // Start is called before the first frame update
        void Start()
        {
            if (_addOnStart)
            {
                AddMarker();
            }
        }

        /// <summary>
        /// Add the marker to the compass
        /// </summary>
        public void AddMarker()
        {
            if (_compass != null)
            {
                if (_markerReference == MARKER_REFERENCE.TRANSFORM)
                {
                    if (_target != null)
                    {
                        _compass.AddMarker(_id, _target, _prefabUI);
                    }
                    else
                    {
                        _compass.AddMarker(_id, transform, _prefabUI);
                    }
                }
                else
                {
                    _compass.AddMarker(_id, _prefabUI, _heading);
                }
            }
            else
            {
                Debug.LogError("[Compass] Could not add marker " + _id + " because no compass reference has been set");
            }
        }
    }
}