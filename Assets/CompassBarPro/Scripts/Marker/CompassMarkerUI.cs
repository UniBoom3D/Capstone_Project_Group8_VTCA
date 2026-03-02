
using UnityEngine;
using UnityEngine.UI;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// Class of data for a compass marker
    /// </summary>
    public class CompassMarkerUI
    {
        /// <summary>
        /// The id of the Marker
        /// </summary>
        public string _id;

        /// <summary>
        /// The Graphic component of the marker in the compass
        /// </summary>
        public Graphic graphic;

        /// <summary>
        /// True if the marker is at an absolute heading, which means it's heading will remain constant and is independant from the player's position
        /// </summary>
        public bool isAbsolute = false;

        /// <summary>
        /// The heading of the market
        /// </summary>
        public float heading;

        /// <summary>
        /// The transform component in the scene representing the market, leave this to null if the marker is absolute
        /// </summary>
        public Transform transform;
    }
}