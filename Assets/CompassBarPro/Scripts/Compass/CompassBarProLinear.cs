using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// This class handles the Linear Compass behavior
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CompassBarProLinear : CompassBarPro, IMaterialModifier
    {
        #region Params
        /*
        * Public Parameters
        */
        [Tooltip("The mask texture for the compass background")]
        public Texture _maskTexture;
        [Tooltip("Prefab for graduation")]
        public GameObject _graduationPrefab;
        [Tooltip("Factor size for text indicator")]
        [Range(0.5f, 3.5f)] public float _indicatorSclaeFactor = 2.0f;

        [Tooltip("Parent GameObject for all graduations")]
        public Transform _graduationParent;

        [Range(0, 359)] public int _defaultMarkerPosY = 20;

        #endregion

        #region private_methodes

        /// <summary>
        /// The mask material used by graduations and markers to handle mask and gradiant transparency
        /// </summary>
        private Material _maskMaterial;

        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void Start()
        {
            base.Start();
            BuildCompass(true);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

        private void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                ComputeMask();
            }
#else
            {
                ComputeMask();                
            }
#endif
        }

        /// <summary>
        /// Build the Compass
        /// </summary>
        public override void BuildCompass(bool keepMarkers)
        {
            // Debug.Log("[Compass] Building compass ");

            if (!keepMarkers)
            {
                // Remove actual markers
                RemoveMarkers();
            }

            // Calculate the number of graduation and the step in pixels between each graduation
            CalculateGraduations();

            if (_hasGraduations)
            {
                if (_graduationPrefab != null)
                {
                    // Building Graduations
                    for (int i = 0; i < _graduationCount; i++)
                    {
                        if (_graduationsValue.Count <= i)
                        {
                            // Debug.Log("[Compass] Creating new graduation " + i);
                            GameObject go = GameObject.Instantiate(_graduationPrefab, _graduationParent.transform);
                            go.name = "graduation_" + i;

                            Text value = go.GetComponent<Text>();
                            Text line = go.transform.GetChild(0).GetComponent<Text>();
                            value.material = GetMaskedMaterial(true);
                            line.material = GetMaskedMaterial(true);

                            _graduationsValue.Add(value);
                            _graduationsLine.Add(line);
                        }
                        else if (_graduationsValue[i] == null)
                        {
                            Transform graduation = _graduationParent.transform.Find("graduation_" + i);
                            if (graduation == null)
                            {
                                GameObject go = GameObject.Instantiate(_graduationPrefab, _graduationParent.transform);
                                go.name = "graduation_" + i;
                                _graduationsValue[i] = go.GetComponent<Text>();
                                _graduationsLine[i] = go.transform.GetChild(0).GetComponentInChildren<Text>();
                                _graduationsValue[i].material = GetMaskedMaterial(true);
                                _graduationsLine[i].material = GetMaskedMaterial(true);
                            }
                            else
                            {
                                _graduationsValue[i] = graduation.GetComponent<Text>();
                                _graduationsLine[i] = graduation.transform.GetChild(0).GetComponentInChildren<Text>();
                            }

                        }
                        else
                        {
                            _graduationsValue[i].gameObject.SetActive(true);
                            _graduationsValue[i].material = GetMaskedMaterial(true);
                            _graduationsLine[i].material = GetMaskedMaterial(true);
                        }

                        _graduationsValue[i].material.SetTexture("_MaskTex", _maskTexture);
                        _graduationsLine[i].material.SetTexture("_MaskTex", _maskTexture);
                    }

                    UpdateGraduations(_heading);
                }
                else
                {
                    Debug.LogError("[Compass] Missing graduation prefab");
                }
                // Deactivate all existing but no needed graduation
                for (int i = _graduationCount; i < _graduationsValue.Count; i++)
                {
                    if (_graduationsValue[i] != null)
                    {
                        _graduationsValue[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _graduationsValue.Count; i++)
                {
                    if (_graduationsValue[i] != null)
                    {
                        DestroyImmediate(_graduationsValue[i].gameObject);
                    }
                }
                _graduationsValue.Clear();
                _graduationsLine.Clear();
            }
        }

        /// <summary>
        /// Set the text of the graduation, given the it's angle 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="line"></param>
        /// <param name="angle"></param>
        protected void SetGraduationText(Text value, Text line, int angle)
        {

            Text template = _graduationPrefab.GetComponent<Text>();

            if (angle == 0 && _cardinalIndicatorDetail != CARDINAL_INDICATOR_DETAIL.NONE)
            {
                value.text = "N";
                value.fontSize = Mathf.RoundToInt((float)template.fontSize * _indicatorSclaeFactor);
                value.fontStyle = FontStyle.Bold;
                line.gameObject.SetActive(false);
            }
            else if (angle == 45 && _cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.EIGHT_INDICATOR)
            {
                value.text = "NE";
                value.fontSize = template.fontSize;
                value.fontStyle = FontStyle.Normal;
                line.gameObject.SetActive(true);
            }
            else if (angle == 90 && _cardinalIndicatorDetail != CARDINAL_INDICATOR_DETAIL.NONE)
            {
                value.text = "E";
                value.fontSize = Mathf.RoundToInt((float)template.fontSize * _indicatorSclaeFactor);
                value.fontStyle = FontStyle.Bold;
                line.gameObject.SetActive(false);
            }
            else if (angle == 135 && _cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.EIGHT_INDICATOR)
            {
                value.text = "SE";
                value.fontSize = template.fontSize;
                value.fontStyle = FontStyle.Normal;
                line.gameObject.SetActive(true);
            }
            else if (angle == 180 && _cardinalIndicatorDetail != CARDINAL_INDICATOR_DETAIL.NONE)
            {
                value.text = "S";
                value.fontSize = Mathf.RoundToInt((float)template.fontSize * _indicatorSclaeFactor);
                value.fontStyle = FontStyle.Bold;
                line.gameObject.SetActive(false);
            }
            else if (angle == 225 && _cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.EIGHT_INDICATOR)
            {
                value.text = "SW";
                value.fontSize = template.fontSize;
                value.fontStyle = FontStyle.Normal;
                line.gameObject.SetActive(true);
            }
            else if (angle == 270 && _cardinalIndicatorDetail != CARDINAL_INDICATOR_DETAIL.NONE)
            {
                value.text = "W";
                value.fontSize = Mathf.RoundToInt((float)template.fontSize * _indicatorSclaeFactor);
                value.fontStyle = FontStyle.Bold;
                line.gameObject.SetActive(false);
            }
            else if (angle == 315 && _cardinalIndicatorDetail == CARDINAL_INDICATOR_DETAIL.EIGHT_INDICATOR)
            {
                value.text = "NW";
                value.fontSize = template.fontSize;
                value.fontStyle = FontStyle.Normal;
                line.gameObject.SetActive(true);
            }
            else
            {
                value.text = angle.ToString();
                value.fontSize = template.fontSize;
                value.fontStyle = FontStyle.Normal;
                line.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="marker"></param>
        public override void AddMarker(string id, Transform marker)
        {
            AddMarker(id, marker, _defaultMarkerPrefab, 0, _defaultMarkerPosY);
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab)
        {
            AddMarker(id, marker, prefab, 0, _defaultMarkerPosY);
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab, int posY)
        {
            AddMarker(id, marker, prefab, 0, _defaultMarkerPosY);
        }

        public override void AddMarker(string id, float heading, int posY)
        {
            AddMarker(id, null, _defaultMarkerPrefab, heading, posY);
        }

        public override void AddMarker(string id, float heading)
        {
            AddMarker(id, null, _defaultMarkerPrefab, heading, _defaultMarkerPosY);
        }

        public override void AddMarker(string id, GameObject prefab, float heading)
        {
            AddMarker(id, null, prefab, heading, _defaultMarkerPosY);
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
                go.GetComponent<Graphic>().material = GetMaskedMaterial(true);
                go.transform.localPosition = new Vector3((heading - _heading) * _stepX / _graduationPrecision, posY, 0);

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
        /// Update the compass with new value of heading
        /// </summary>
        /// <param name="heading"></param>
        public override void UpdateGraduations(float heading)
        {
            heading = AngleModulo(heading);

            // Update Compass's graduations values
            for (int i = 0; i < _graduationsValue.Count; i++)
            {
                if (_graduationsValue[i] != null)
                {
                    _graduationsValue[i].transform.localPosition = new Vector3((i - _graduationCount / 2) * _stepX - (heading % _graduationPrecision) * _stepX / _graduationPrecision, 0, 0);
                    int angle = ((i - _graduationCount / 2) * _graduationPrecision + _graduationPrecision * (int)(heading / _graduationPrecision)) % 360;
                    if (angle < 0)
                    {
                        angle += 360;
                    }
                    SetGraduationText(_graduationsValue[i], _graduationsLine[i], angle);
                }
            }
        }

        /// <summary>
        /// Update the markers
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="playerOrientation"></param>
        public override void UpdateMarkers(float heading, Transform playerOrientation = null)
        {
            foreach (CompassMarkerUI marker in _markers)
            {
                if (marker != null)
                {
                    if (!marker.isAbsolute && marker.transform != null)
                    {
                        if (playerOrientation != null)
                        {
                            marker.heading = heading - AngleSigned(marker.transform.position - playerOrientation.position, playerOrientation.forward, Vector3.up);
                        }
                    }

                    if (marker.heading - heading < -180)
                    {
                        marker.graphic.transform.localPosition = new Vector3(((marker.heading - heading + 360) % 360) * _stepX / _graduationPrecision, _defaultMarkerPosY, 0);
                    }
                    else if (marker.heading - heading > 180)
                    {
                        marker.graphic.transform.localPosition = new Vector3(((marker.heading - heading - 360) % 360) * _stepX / _graduationPrecision, _defaultMarkerPosY, 0);
                    }
                    else
                    {
                        marker.graphic.transform.localPosition = new Vector3((marker.heading - heading) * _stepX / _graduationPrecision, _defaultMarkerPosY, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Compute the compass mask texture for graduations and markers
        /// </summary>
        public override void ComputeMask()
        {
            if (graphic != null)
            {
                Vector2 parentSize = _rectTransform.rect.size;
                float offsetX;
                float offsetY;

                // Image components
                for (int i = 0; i < _graduationsValue.Count; i++)
                {
                    if (_graduationsValue[i] != null)
                    {
                        offsetX = _graduationsValue[i].rectTransform.localPosition.x - _graduationsValue[i].rectTransform.pivot.x * _graduationsValue[i].rectTransform.sizeDelta.x + _rectTransform.pivot.x * parentSize.x;
                        offsetY = _graduationsValue[i].rectTransform.localPosition.y - _graduationsValue[i].rectTransform.pivot.y * _graduationsValue[i].rectTransform.sizeDelta.y + _rectTransform.pivot.y * parentSize.y;
                        UpdateMaskMaterial(_graduationsValue[i].material, offsetX, offsetY, parentSize.x, parentSize.y, _graduationsValue[i].rectTransform.sizeDelta.x, _graduationsValue[i].rectTransform.sizeDelta.y);
                        UpdateMaskMaterial(_graduationsLine[i].material, offsetX, offsetY, parentSize.x, parentSize.y, _graduationsValue[i].rectTransform.sizeDelta.x, _graduationsValue[i].rectTransform.sizeDelta.y);
                    }
                }

                // Markers
                foreach (CompassMarkerUI marker in _markers)
                {
                    if (marker != null)
                    {
                        offsetX = marker.graphic.rectTransform.localPosition.x - marker.graphic.rectTransform.pivot.x * marker.graphic.rectTransform.sizeDelta.x + _rectTransform.pivot.x * parentSize.x;
                        offsetY = marker.graphic.rectTransform.localPosition.y - marker.graphic.rectTransform.pivot.y * marker.graphic.rectTransform.sizeDelta.y + _rectTransform.pivot.y * parentSize.y;
                        UpdateMaskMaterial(marker.graphic.material, offsetX, offsetY, parentSize.x, parentSize.y, marker.graphic.rectTransform.sizeDelta.x, marker.graphic.rectTransform.sizeDelta.y);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new masked material
        /// </summary>
        /// <param name="isText"></param>
        /// <returns></returns>
        private Material GetMaskedMaterial(bool isText)
        {
            Material mat = new Material(Shader.Find("Custom/CompassSoftMask"));
            mat.SetFloat("_StencilComp", 3);
            mat.SetFloat("_Stencil", 1);
            mat.SetFloat("_StencilOp", 0);
            mat.SetFloat("_StencilWriteMask", 0);
            mat.SetFloat("_StencilReadMask", 1);
            mat.SetFloat("_ColorMask", 15);
            mat.SetTexture("_MaskTex", _maskTexture);
            if (isText)
            {
                mat.SetFloat("_istext", 1);
            }
            return mat;
        }

        /// <summary>
        /// Set the masked material shader's values from the position of the associated element
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="parentSizeX"></param>
        /// <param name="parentSizeY"></param>
        /// <param name="sizeDeltaX"></param>
        /// <param name="sizeDeltaY"></param>
        private void UpdateMaskMaterial(Material mat, float offsetX, float offsetY, float parentSizeX, float parentSizeY, float sizeDeltaX, float sizeDeltaY)
        {
            mat.SetFloat("_x", offsetX / parentSizeX);
            mat.SetFloat("_y", offsetY / parentSizeY);
            mat.SetFloat("_sizex", parentSizeX / sizeDeltaX);
            mat.SetFloat("_sizey", parentSizeY / sizeDeltaY);
        }

        /// <summary>
        /// Return the mask material of the compass's root component
        /// </summary>
        /// <returns></returns>
        private Material GetMaskMaterial()
        {
            if (_maskMaterial == null)
            {
                _maskMaterial = new Material(Shader.Find("UI/Default"));
                _maskMaterial.SetFloat("_StencilComp", 8);
                _maskMaterial.SetFloat("_Stencil", 1);
                _maskMaterial.SetFloat("_StencilOp", 2);
                _maskMaterial.SetFloat("_StencilWriteMask", 255);
                _maskMaterial.SetFloat("_StencilReadMask", 255);
                _maskMaterial.SetFloat("_ColorMask", 0);
                _maskMaterial.SetTexture("_MaskTex", _maskTexture);
            }
            return _maskMaterial;
        }

        /// <summary>
        /// Implementation of the GetModifiedMaterial methode from IMaterialModifier interface
        /// </summary>
        /// <param name="baseMaterial"></param>
        /// <returns></returns>
        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            if (isActiveAndEnabled)
            {
                return GetMaskMaterial();
            }
            else
            {
                return baseMaterial;
            }
        }
    }
}