using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.Compass
{

    /// <summary>
    /// Attach this script to the player GameObject so that it updates the Compass's heading
    /// </summary>
    public class PlayerHeading : MonoBehaviour
    {
        [Tooltip("Reference to the Compass GameObject")]
        public ICompassBarPro _compass;

        // Update is called once per frame
        void Update()
        {
            // Update Compass with player's heading
            //_compass.SetHeading(transform.rotation.eulerAngles.y);
            _compass.SetHeading(transform);
        }
    }
}
