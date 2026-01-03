using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace LunarCatsStudio.Compass
{
    public class ObjectClick : MonoBehaviour
    {
        public bool _onlyOnce = true;
        public UnityEvent _onClick;

        private bool _triggered = false;

        void OnMouseDown()
        {
            if (_onlyOnce && _triggered)
            {
                return;
            }
            _triggered = true;
            _onClick.Invoke();
        }
    }
}
