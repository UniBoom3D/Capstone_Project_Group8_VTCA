using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LunarCatsStudio.Compass
{
    public class SliderToHeading : MonoBehaviour
    {
        public Slider _slider;

        public ICompassBarPro _compass;

        public void Start()
        {
            UpdateValue();
        }

        public void UpdateValue()
        {
            _compass.SetHeading(_slider.value);
        }
    }
}
