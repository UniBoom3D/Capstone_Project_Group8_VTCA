using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace LunarCatsStudio.Compass
{
    public abstract class ICompassBarPro : UIBehaviour
    {
        /// <summary>
        /// Remove all existing markers from compass
        /// </summary>
        public abstract void RemoveMarkers();

        /// <summary>
        /// Remove the marker matching id
        /// </summary>
        /// <param name="id"></param>
        public abstract void RemoveMarker(string id);

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="marker"></param>
        public abstract void AddMarker(string id, Transform marker);

        public abstract void AddMarker(string id, Transform marker, GameObject prefab);

        public abstract void AddMarker(string id, Transform marker, GameObject prefab, int posY);

        public abstract void AddMarker(string id, float heading, int posY);

        public abstract void AddMarker(string id, float heading);

        public abstract void AddMarker(string id, GameObject prefab, float heading);

        /// <summary>
        /// Add a marker
        /// </summary>
        /// <param name="id"></param>
        /// <param name="markerTransform"></param>
        /// <param name="prefab"></param>
        /// <param name="heading"></param>
        /// <param name="posY"></param>
        /// <summary>
        public abstract void AddMarker(string id, Transform markerTransform, GameObject prefab, float heading, int posY);


        /// <summary>
        /// Return the list of markers
        /// </summary>
        /// <returns></returns>
        public abstract List<CompassMarkerUI> GetMarkers();

        /// <summary>
        /// Set the north to a new Vector3 direction
        /// </summary>
        /// <param name="northDirection"></param>
        public abstract void SetNorthDirection(Vector3 northDirection);

        /// <summary>
        /// Set the north to a new arbitrary transform position
        /// </summary>
        /// <param name="northPosition"></param>
        public abstract void SetNorthPosition(Transform northPosition);

        /// <summary>
        /// Set the heading of the compass.false The heading is calculated with the 
        /// player's Transfrom and with the selected north type
        /// </summary>
        /// <param name="playerOrientation"></param>
        public abstract void SetHeading(Transform playerOrientation);

        /// <summary>
        /// Set the heading of the compass, given the heading in parameter
        /// </summary>
        /// <param name="heading"></param>
        public abstract void SetHeading(float heading);

        /// <summary>
        /// Return the heading in degree
        /// </summary>
        /// <returns></returns>
        public abstract float GetHeading();

        /// <summary>
        /// Get the direction to the north from the origin's transform positon and rotation
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public abstract Vector3 GetDirectionToNorth(Transform origin);

        /// <summary>
        /// Get the direction to the north based on the player's position and rotation
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetDirectionToNorth();

    }

}