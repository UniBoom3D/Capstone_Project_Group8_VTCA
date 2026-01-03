using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LunarCatsStudio.Compass
{
    public class ZoomIndicator : MonoBehaviour
    {

        public Slider _slider;

        public CompassBarProCircular _compass;

        // Start is called before the first frame update
        void Start()
        {
            UpdateZoom();
        }

        public void UpdateZoom()
        {
            if (_compass != null)
            {
                _slider.value = (_compass.GetZoom() - CompassBarProCircular.MIN_ZOOM) / (CompassBarProCircular.MAX_ZOOM - CompassBarProCircular.MIN_ZOOM);
            }
        }
    }
}