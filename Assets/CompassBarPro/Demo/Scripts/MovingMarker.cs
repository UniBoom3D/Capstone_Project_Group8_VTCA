using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.Compass
{
    public class MovingMarker : MonoBehaviour
    {
        public float _speed = 1;
        private Vector3 _position;
        private Vector3 _targetPosition1;
        private Vector3 _targetPosition2;

        private float _t = 0.5f;


        // Start is called before the first frame update
        void Start()
        {
            _position = transform.localPosition;
            _targetPosition1 = _position + new Vector3(-25, 0, 0);
            _targetPosition2 = _position + new Vector3(25, 0, 0);
        }

        // Update is called once per frame
        void Update()
        {
            _t += _speed * Time.deltaTime;
            transform.localPosition = Vector3.Lerp(_targetPosition1, _targetPosition2, _t);

            if (_t > 1)
            {
                _t = 1;
                _speed = -_speed;
            }
            else if (_t < 0)
            {
                _t = 0;
                _speed = -_speed;
            }
        }
    }
}
