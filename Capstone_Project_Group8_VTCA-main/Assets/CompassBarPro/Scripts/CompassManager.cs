using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunarCatsStudio.Compass
{
    /// <summary>
    /// 
    /// </summary>
    public class CompassManager : ICompassBarPro
    {
        public ICompassBarPro[] _compass;

        /// <summary>
        /// Remove all existing markers from compass
        /// </summary>
        public override void RemoveMarkers()
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.RemoveMarkers();
            }
        }

        /// <summary>
        /// Remove the marker matching id
        /// </summary>
        /// <param name="id"></param>
        public override void RemoveMarker(string id)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.RemoveMarker(id);
            }
        }

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="marker"></param>
        public override void AddMarker(string id, Transform marker)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, marker);
            }
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, marker, prefab);
            }
        }

        public override void AddMarker(string id, Transform marker, GameObject prefab, int posY)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, marker, prefab, posY);
            }
        }

        public override void AddMarker(string id, float heading, int posY)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, heading, posY);
            }
        }

        public override void AddMarker(string id, float heading)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, heading);
            }
        }

        public override void AddMarker(string id, GameObject prefab, float heading)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, prefab, heading);
            }
        }

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="id"></param>
        /// <param name="markerTransform"></param>
        /// <param name="prefab"></param>
        /// <param name="heading"></param>
        /// <param name="posY"></param>
        /// <summary>
        public override void AddMarker(string id, Transform markerTransform, GameObject prefab, float heading, int posY)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.AddMarker(id, markerTransform, prefab, heading, posY);
            }
        }

        /// <summary>
        /// Return the list of markers
        /// </summary>
        /// <returns></returns>
        public override List<CompassMarkerUI> GetMarkers()
        {
            return _compass[0].GetMarkers();
        }

        /// <summary>
        /// Set the north to a new Vector3 direction
        /// </summary>
        /// <param name="northDirection"></param>
        public override void SetNorthDirection(Vector3 northDirection)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.SetNorthDirection(northDirection);
            }
        }

        /// <summary>
        /// Set the north to a new arbitrary transform position
        /// </summary>
        /// <param name="northPosition"></param>
        public override void SetNorthPosition(Transform northPosition)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.SetNorthPosition(northPosition);
            }
        }

        /// <summary>
        /// Set the heading of the compass.false The heading is calculated with the 
        /// player's Transfrom and with the selected north type
        /// </summary>
        /// <param name="playerOrientation"></param>
        public override void SetHeading(Transform playerOrientation)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.SetHeading(playerOrientation);
            }
        }

        /// <summary>
        /// Set the heading of the compass, given the heading in parameter
        /// </summary>
        /// <param name="heading"></param>
        public override void SetHeading(float heading)
        {
            foreach (ICompassBarPro compass in _compass)
            {
                compass.SetHeading(heading);
            }
        }

        /// <summary>
        /// Return the heading in degree
        /// </summary>
        /// <returns></returns>
        public override float GetHeading()
        {
            return _compass[0].GetHeading();
        }

        /// <summary>
        /// Get the direction to the north from the origin's transform positon and rotation
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override Vector3 GetDirectionToNorth(Transform origin)
        {
            return _compass[0].GetDirectionToNorth();
        }

        /// <summary>
        /// Get the direction to the north based on the player's position and rotation
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetDirectionToNorth()
        {
            return _compass[0].GetDirectionToNorth();
        }
    }
}