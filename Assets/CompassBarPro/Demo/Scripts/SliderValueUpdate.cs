using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LunarCatsStudio.Compass
{
    [RequireComponent(typeof(Text))]
    public class SliderValueUpdate : MonoBehaviour
    {
        public Slider _slider;

        public CompassMarker _compassMarker;

        private Text _text;

        public void Start()
        {
            _text = GetComponent<Text>();
        }

        public void UpdateValue()
        {
            _text.text = _slider.value.ToString();
            _compassMarker._heading = _slider.value;
        }
    }
}
