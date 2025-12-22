using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.Compass
{
    public class NorthChanger : MonoBehaviour
    {
        public Transform _arrow;
        public ICompassBarPro _compass;
        private int _count = 0;

        public void ChangeNorth()
        {
            if (_count == 0)
            {
                _arrow.localRotation = Quaternion.Euler(0, 180, 0);
            }
            _count++;
            if (_count < 4)
            {
                _arrow.localRotation *= Quaternion.Euler(0, 90, 0);
                if (_compass != null)
                {
                    _compass.SetNorthDirection(_arrow.forward);
                }
            }
            else
            {
                _arrow.localRotation = Quaternion.Euler(90, 180, 0);
                _count = 0;
                if (_compass != null)
                {
                    _compass.SetNorthPosition(transform);
                }
            }
        }
    }
}
