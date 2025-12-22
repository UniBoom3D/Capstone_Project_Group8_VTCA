using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// Abstract class for compass for high level management
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public abstract class CompassBarPro : ICompassBarPro
    {
        #region Params
        public enum NORTH_TYPE { DIRECTION, TRANSFORM };
        public enum CARDINAL_INDICATOR_DETAIL { NONE, FOUR_INDICATOR, EIGHT_INDICATOR };

        /*
        * Public Parameters
        */
        [Tooltip("Text component displaying the heading value")]
        public Text _headingIndicator;
        [Tooltip("Default prefab for markers")]
        public GameObject _defaultMarkerPrefab;
        [Tooltip("The parent GameObject for all markers")]
        public GameObject _markersParent;
        [Tooltip("Angular range (in degree) covered by the visible part of the compass")]
        [Range(5, 360)] public int _visibleCovertureAngle = 270;
        public NORTH_TYPE _northType = NORTH_TYPE.DIRECTION;
        [Tooltip("Define a transform GameObject that represents the north")]
        public Transform _northTransform;
        [Tooltip("Definition of the north by an arbitrary constant direction")]
        public Vector3 _northVector = new Vector3(1, 0, 0);
        [Tooltip("The angular range between two graduations")]
        [Range(5, 90)] public int _graduationPrecision = 15;
        public bool _hasGraduations = true;
        public CARDINAL_INDICATOR_DETAIL _cardinalIndicatorDetail = CARDINAL_INDICATOR_DETAIL.FOUR_INDICATOR;

        #endregion

        #region private_methodes
        /// <summary>
        /// The rect transform of this component
        /// </summary>
        protected RectTransform _rectTransform;

        /// <summary>
        /// List of all Text component displaying the value for each graduations
        /// </summary>
        /// <typeparam name="Text"></typeparam>
        /// <returns></returns>
        [SerializeField]
        protected List<Text> _graduationsValue = new List<Text>();

        /// <summary>
        /// List of all Text component displaying the graduation line | for each graduations
        /// </summary>
        /// <typeparam name="Text"></typeparam>
        /// <returns></returns>
        [SerializeField]
        protected List<Text> _graduationsLine = new List<Text>();

        /// <summary>
        /// The list of markers
        /// </summary>
        /// <typeparam name="CompassMarkerUI"></typeparam>
        /// <returns></returns>
        protected List<CompassMarkerUI> _markers = new List<CompassMarkerUI>();

        /// <summary>
        /// The heading value in degree
        /// </summary>
        public float _heading = 0;

        /// <summary>
        /// Amount of visible graduation in compass
        /// </summary>
        [SerializeField]
        protected int _graduationCount;

        /// <summary>
        /// Distance in pixels bewteen two graduations
        /// </summary>
        [SerializeField]
        protected int _stepX;

        /// <summary>
        /// The Graphic component attached to this GameObject
        /// </summary>
        private Graphic _graphic;

        /// <summary>
        /// Reference to the player's tranform
        /// </summary>
        private Transform _player;

        #endregion

        public Graphic graphic
        {
            get
            {
                if (_graphic == null)
                {
                    _graphic = GetComponent<Graphic>();
                }
                return _graphic;
            }
        }

        protected override void OnEnable()
        {
            if (graphic != null)
            {
                graphic.SetMaterialDirty();
            }
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            ComputeMask();
        }

        protected override void OnDisable()
        {
            if (graphic != null)
            {
                graphic.SetMaterialDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (graphic != null)
            {
                graphic.SetMaterialDirty();
            }
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            ComputeMask();
        }
#endif

        /// <summary>
        /// Initialize the given RectTransform to fit it's parent size
        /// </summary>
        /// <param name="rect"></param>
        protected void InitializeRectTransform(RectTransform rect)
        {
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Calculate the number of graduation and the step in pixels between each graduation
        /// </summary>
        protected void CalculateGraduations()
        {
            _graduationCount = _visibleCovertureAngle / _graduationPrecision + 3;
            _stepX = (int)(_rectTransform.rect.size.x * _graduationPrecision / (float)_visibleCovertureAngle);
        }

        /// <summary>
        /// Build the Compass
        /// </summary>
        public abstract void BuildCompass(bool keepMarkers);

      

        /// <summary>
        /// Remove all existing markers from compass
        /// </summary>
        public override void RemoveMarkers()
        {
            // Clear the existing markers
            foreach (CompassMarkerUI gr in _markers)
            {
                if (gr.graphic != null)
                {
                    DestroyImmediate(gr.graphic.gameObject);
                }
            }
            _markers.Clear();
        }

        /// <summary>
        /// Remove the marker matching id
        /// </summary>
        /// <param name="id"></param>
        public override void RemoveMarker(string id)
        {
            bool removed = false;
            // Clear the existing markers
            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i]._id.Equals(id))
                {
                    DestroyImmediate(_markers[i].graphic.gameObject);
                    _markers.Remove(_markers[i]);
                    removed = true;
                }
            }

            if (!removed)
            {
                Debug.LogWarning("[Compass] Could not remove marker '" + id + "' because it was not found in the list of current markers.");
            }
        }

        /// <summary>
        /// Return the list of markers
        /// </summary>
        /// <returns></returns>
        public override List<CompassMarkerUI> GetMarkers()
        {
            return _markers;
        }

        /// <summary>
        /// Set the north to a new Vector3 direction
        /// </summary>
        /// <param name="northDirection"></param>
        public override void SetNorthDirection(Vector3 northDirection)
        {
            _northType = NORTH_TYPE.DIRECTION;
            _northVector = northDirection;
        }

        /// <summary>
        /// Set the north to a new arbitrary transform position
        /// </summary>
        /// <param name="northPosition"></param>
        public override void SetNorthPosition(Transform northPosition)
        {
            _northType = NORTH_TYPE.TRANSFORM;
            _northTransform = northPosition;
        }

        /// <summary>
        /// Set the heading of the compass.false The heading is calculated with the 
        /// player's Transfrom and with the selected north type
        /// </summary>
        /// <param name="playerOrientation"></param>
        public override void SetHeading(Transform playerOrientation)
        {
            float heading = 0;
            if (_player == null || _player != playerOrientation)
            {
                _player = playerOrientation;
            }
            if (_northType == NORTH_TYPE.DIRECTION)
            {
                heading = AngleSigned(_northVector, playerOrientation.forward, Vector3.up);
            }
            else
            {
                if (_northTransform != null)
                {
                    heading = AngleSigned(_northTransform.position - playerOrientation.position, playerOrientation.forward, Vector3.up);
                }
                else
                {
                    Debug.LogError("[Compass] Missing north transform");
                }
            }
            _heading = heading;
            UpdateHeading(_heading);
            UpdateGraduations(_heading);
            UpdateMarkers(_heading, _player);
        }

        /// <summary>
        /// Set the heading of the compass, given the heading in parameter
        /// </summary>
        /// <param name="heading"></param>
        public override void SetHeading(float heading)
        {
            _heading = heading;
            UpdateHeading(_heading);
            UpdateGraduations(_heading);
            UpdateMarkers(_heading);
        }

        /// <summary>
        /// Return the heading in degree
        /// </summary>
        /// <returns></returns>
        public override float GetHeading()
        {
            return _heading;
        }

        /// <summary>
        /// Get the direction to the north from the origin's transform positon and rotation
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override Vector3 GetDirectionToNorth(Transform origin)
        {
            if (_northType == NORTH_TYPE.DIRECTION)
            {
                if (origin != null)
                {
                    float headingToNorth = AngleSigned(_northVector, origin.forward, Vector3.up);
                    return new Vector3(0, headingToNorth, 0);
                }
                else
                {
                    Debug.LogWarning("[Compass] Could not determine north direction because origin has not been specified");
                    return Vector3.one;
                }
            }
            else
            {
                if (origin != null)
                {
                    float headingToNorth = AngleSigned(_northTransform.position - origin.position, origin.forward, Vector3.up);
                    return new Vector3(0, headingToNorth, 0);
                }
                else
                {
                    Debug.LogWarning("[Compass] Could not determine north direction because origin has not been specified");
                    return Vector3.one;
                }
            }
        }

        /// <summary>
        /// Get the direction to the north based on the player's position and rotation
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetDirectionToNorth()
        {
            return GetDirectionToNorth(_player);
        }

        /// <summary>
        /// Update the heading indicator value of the compass
        /// </summary>
        /// <param name="heading"></param>
        public virtual void UpdateHeading(float heading)
        {
            heading = AngleModulo(heading);

            if (_headingIndicator != null)
            {
                _headingIndicator.text = ((int)heading).ToString();
            }

            // Update the heading indicator
            if (_headingIndicator != null)
            {
                _headingIndicator.SetLayoutDirty();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(_headingIndicator);
#endif
            }
        }

        /// <summary>
        /// Update the compass with new value of heading
        /// </summary>
        /// <param name="heading"></param>
        public abstract void UpdateGraduations(float heading);

        /// <summary>
        /// Update the markers
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="playerOrientation"></param>
        public abstract void UpdateMarkers(float heading, Transform playerOrientation = null);


        /// <summary>
        /// Determine the signed angle between two vectors, with normal 'n'
        /// as the rotation axis.
        /// </summary>
        protected float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns a valid heading angle in the range [0, 359]
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        protected float AngleModulo(float angle)
        {
            if (angle < 0)
            {
                angle += 360;
            }
            return angle % 360;
        }

        /// <summary>
        /// Compute the compass mask texture for graduations and markers
        /// </summary>
        public abstract void ComputeMask();

    }
}