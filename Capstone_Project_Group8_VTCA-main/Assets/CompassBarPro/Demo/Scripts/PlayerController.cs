using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// This script manages the player movements
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public float _mouseSensibility = 2.0f;
        public float _moveSpeed = 3.0f;
        private Rigidbody _rigid;

        void Start()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            // Rotate player with mouse movements
            float mouseX = _mouseSensibility * Input.GetAxis("Mouse X") * Time.deltaTime;
            transform.Rotate(0, mouseX, 0);

            // Move player with keyboard input   
            float movementForward = Input.GetAxis("Vertical") * Time.deltaTime;
            float movementSideway = Input.GetAxis("Horizontal") * Time.deltaTime;
            _rigid.linearVelocity = _moveSpeed * Vector3.Normalize(transform.forward * movementForward + transform.right * movementSideway);
        }
    }
}