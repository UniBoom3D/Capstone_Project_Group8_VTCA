using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// This class handles the Circular Compass behavior
    /// </summary>
    public class CompassBarProCircular : CompassBarPro
    {
        public enum UPDATE_MODE { CARDINAL_IMAGE, POINTER };


        #region Params
        /*
        * Public Parameters
        */
        [Tooltip("The image of the compass's pointer")]
        public RectTransform _headingPointer;

        [Tooltip("The update mode of the compass: rotate compass or rotate pointer when heading changes")]
        public UPDATE_MODE _rotatingPart = UPDATE_MODE.CARDINAL_IMAGE;

        [Tooltip("Zoom level of the compass used to place markers and calculate distance with player")]
        [Range(1, 100)]
        public float _zoomLevel = 5.0f;

        [Tooltip("Set to true if you want markers that are far away to be shown in compass. They will be displayed in border of the compass")]
        public bool _showOutOfRangeMarkers = false;

        [Tooltip("The parent GameObject of cardinals text elements")]
        public GameObject _cardinalsTextParent;

        [Tooltip("Reference to the cardinals image of the Compass")]
        public RectTransform _cardinalsImage;

        [Tooltip("Set to true if you want the cardinal text elements never to rotate")]
        public bool _adjustTextRotation = true;

        [Tooltip("Set to true if you want the markers elements never to rotate")]
        public bool _adjustMarkerRotation = true;


        /// <summary>
        /// List of RectTransform displaying cardinal values
        /// </summary>
        /// <typeparam name="RectTransform"></typeparam>
        /// <returns></returns>
        private List<RectTransform> _cardinalsText = new List<RectTransform>();

        /// <summary>
        /// Default zoom factor to adapt markers position in compass 
        /// </summary>
        private const float DISTANCE_REDUCTION_FACTOR = 1 / 100f;

        public const float MIN_ZOOM = 1;
        public const float MAX_ZOOM = 20;

        #endregion


        protected override void OnEnable()
        {
            base.OnEnable();
            GetCardinalIndicators();
        }

        /// <summary>
        /// Get the reference to the cardinal text GameObjects
        /// </summary>
        private void GetCardinalIndicators()
        {
            if (_cardinalsText.Count != 8)
            {
                _cardinalsText.Clear();
                RectTransform[] rects = _cardinalsTextParent.transform.GetComponentsInChildren<RectTransform>(true);
                foreach (RectTransform rt in rects)
                {
                    if (rt != _cardinalsTextParent.GetComponent<RectTransform>())
                    {
                        _cardinalsText.Add(rt);
                    }
                }
            }
        }

        /// <summary>
        /// Build the Compass
        /// </summary>
        public override void BuildCompass(bool keepMarkers)
        {
            // Debug.Log("[Compass] Building compass ");

            GetCardinalIndicators();

            // Reset elements rotation
            if (_cardinalsTextParent != null)
            {
                _cardinalsTextParent.transform.localRotation = Quaternion.identity;
            }
            foreach (RectTransform rt in _cardinalsText)
            {
                if (_adjustTextRotation)
                {
                    rt.rotation = Quaternion.identity;
                }
                else
                {
                    if (rt.gameObject.name.Equals("N"))
                    {
                        rt.rotation = Quaternion.identity;
                    }
                    else if (rt.gameObject.name.Equals("S"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    else if (rt.gameObject.name.Equals("E"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 270);
                    }
                    else if (rt.gameObject.name.Equals("W"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 90);
                    }
                    else if (rt.gameObject.name.Equals("NE"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 315);
                    }
                    else if (rt.gameObject.name.Equals("NW"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 45);
                    }
                    else if (rt.gameObject.name.Equals("SE"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 225);
                    }
                    else if (rt.gameObject.name.Equals("SW"))
                    {
                        rt.rotation = Quaternion.Euler(0, 0, 135);
                    }
                }
            }
            if (_cardinalsImage != null)
            {
                _cardinalsImage.localRotation = Quaternion.identity;
            }

            _headingPointer.localRotation = Quaternion.Euler(0, 0, 180);
            _markersParent.transform.localRotation = Quaternion.identity;

            // Manage cardinals indications
            if (_cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.FOUR_INDICATOR)
            {
                foreach (RectTransform rt in _cardinalsText)
                {
                    if (rt.name.Equals("NE") || rt.name.Equals("SE") || rt.name.Equals("SW") || rt.name.Equals("NW"))
                    {
                        rt.gameObject.SetActive(false);
                    }
                    else
                    {
                        rt.gameObject.SetActive(true);
                    }
                }
            }
            else if (_cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.EIGHT_INDICATOR)
            {
                foreach (RectTransform rt in _cardinalsText)
                {
                    rt.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (RectTransform rt in _cardinalsText)
                {
                    rt.gameObject.SetActive(false);
                }
            }

            SetHeading(_heading);
        }


        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="marker"></param>
        public override void AddMarker(string id, Transform marker)
        {
            AddMarker(id, marker, _defaultMarkerPrefab, 0, 0);
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab)
        {
            AddMarker(id, marker, prefab, 0, 0);
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab, int posY)
        {
            AddMarker(id, marker, prefab, 0, 0);
        }

        public override void AddMarker(string id, float heading, int posY)
        {
            AddMarker(id, null, _defaultMarkerPrefab, heading, posY);
        }

        public override void AddMarker(string id, float heading)
        {
            AddMarker(id, null, _defaultMarkerPrefab, heading, 0);
        }

        public override void AddMarker(string id, GameObject prefab, float heading)
        {
            AddMarker(id, null, prefab, heading, 0);
        }

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="id"></param>
        /// <param name="markerTransform"></param>
        /// <param name="prefab"></param>
        /// <param name="heading"></param>
        /// <param name="posY"></param>
        public override void AddMarker(string id, Transform markerTransform, GameObject prefab, float heading, int posY)
        {
            // Instantiate and initiate the new marker GameObject
            if (prefab != null)
            {
                GameObject go = GameObject.Instantiate(prefab, _markersParent.transform);
                go.name = prefab.name + "_" + id;

                // Create a new CompassMarkerUI
                CompassMarkerUI marker = new CompassMarkerUI();
                marker._id = id;
                marker.graphic = go.GetComponent<Graphic>();
                marker.transform = markerTransform;
                if (markerTransform == null)
                {
                    marker.isAbsolute = true;
                }
                marker.heading = heading;

                _markers.Add(marker);

                UpdateMarkers(_heading);
            }
            else
            {
                Debug.LogError("[Compass] Could not add marker because no marker prefab has been set");
            }
        }

        /// <summary>
        /// Set the zoom level of the circular compass
        /// This will change the distance between markers and the center of the compass
        /// </summary>
        /// <param name="zoom"></param>
        public void SetZoom(float zoom)
        {
            _zoomLevel = zoom;
        }

        /// <summary>
        /// Return the zoom level
        /// </summary>
        /// <returns></returns>
        public float GetZoom()
        {
            return _zoomLevel;
        }

        /// <summary>
        /// Decrease the zoom level
        /// </summary>
        /// <param name="deltaZoom"></param>
        public void DecreaseZoom(float deltaZoom)
        {
            _zoomLevel = Mathf.Clamp(_zoomLevel - deltaZoom, MIN_ZOOM, MAX_ZOOM);
        }

        /// <summary>
        /// Increase the zoom level
        /// </summary>
        /// <param name="deltaZoom"></param>
        public void IncreaseZoom(float deltaZoom)
        {
            _zoomLevel = Mathf.Clamp(_zoomLevel + deltaZoom, MIN_ZOOM, MAX_ZOOM);
        }

        /// <summary>
        /// Update the heading indicator value of the compass
        /// </summary>
        /// <param name="heading"></param>
        public override void UpdateHeading(float heading)
        {
            base.UpdateHeading(heading);

            if (_rotatingPart == UPDATE_MODE.CARDINAL_IMAGE)
            {
                if (_cardinalsTextParent != null)
                {
                    _cardinalsTextParent.transform.localRotation = Quaternion.Euler(0, 0, heading);
                }
                if (_adjustTextRotation)
                {
                    foreach (RectTransform rt in _cardinalsText)
                    {
                        rt.rotation = Quaternion.identity;
                    }
                }
                if (_cardinalsImage != null)
                {
                    _cardinalsImage.localRotation = Quaternion.Euler(0, 0, heading);
                }
            }
            else
            {
                _headingPointer.localRotation = Quaternion.Euler(0, 0, 180 - heading);
            }
        }

        /// <summary>
        /// Update the compass with new value of heading
        /// </summary>
        /// <param name="heading"></param>
        public override void UpdateGraduations(float heading)
        {
            // Nothing to do here
        }

        /// <summary>
        /// Update the markers
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="playerOrientation"></param>
        public override void UpdateMarkers(float heading, Transform playerOrientation = null)
        {
            if (_rotatingPart == UPDATE_MODE.CARDINAL_IMAGE)
            {
                _markersParent.transform.localRotation = Quaternion.Euler(0, 0, heading);
            }

            foreach (CompassMarkerUI marker in _markers)
            {
                if (marker != null)
                {
                    float radius = _rectTransform.rect.size.x / 2;

                    if (!marker.isAbsolute && marker.transform != null)
                    {
                        float distance = 1f;
                        if (playerOrientation != null)
                        {
                            marker.heading = heading - AngleSigned(marker.transform.position - playerOrientation.position, playerOrientation.forward, Vector3.up);
                            distance = Vector3.Distance(marker.transform.position, playerOrientation.position);
                        }
                        radius = DISTANCE_REDUCTION_FACTOR * _zoomLevel * distance * _rectTransform.rect.size.x / 2;
                    }

                    if (_showOutOfRangeMarkers)
                    {
                        radius = Mathf.Clamp(radius, 0, _rectTransform.rect.size.x / 2);
                        marker.graphic.enabled = true;
                    }
                    else
                    {
                        if (radius > _rectTransform.rect.size.x / 2)
                        {
                            marker.graphic.enabled = false;
                        }
                        else
                        {
                            marker.graphic.enabled = true;
                        }
                    }
                    marker.graphic.transform.localPosition = new Vector3(Mathf.Cos(-Mathf.Deg2Rad * marker.heading + Mathf.PI / 2), Mathf.Sin(-Mathf.Deg2Rad * marker.heading + Mathf.PI / 2), 0) * radius;

                    if (_rotatingPart == UPDATE_MODE.CARDINAL_IMAGE)
                    {
                        if (_adjustMarkerRotation)
                        {
                            marker.graphic.transform.rotation = Quaternion.identity;
                        }
                    }
                }
            }
        }

        public override void ComputeMask()
        {
            // Nothing to do here
        }
    }
}
