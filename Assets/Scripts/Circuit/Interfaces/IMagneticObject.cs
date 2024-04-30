using UnityEngine;

namespace Solis.Circuit.Interfaces
{
    /// <summary>
    /// Interface for magnetic objects. Used to identify objects that are magnetic and should be treated as such.
    /// </summary>
    public interface IMagneticObject
    {
        /// <summary>
        /// Magnetize the object to the anchor.
        /// </summary>
        /// <param name="magnet"></param>
        /// <param name="anchor"></param>
        public void Magnetize(GameObject magnet, Transform anchor);
        
        /// <summary>
        /// Magnetize the object to the anchor.
        /// </summary>
        /// <param name="magnet"></param>
        /// <param name="anchor"></param>
        public void Demagnetize(GameObject magnet, Transform anchor);
        
        /// <summary>
        /// Returns the current anchor of the object.
        /// </summary>
        /// <returns></returns>
        public Transform GetCurrentAnchor();
        
        /// <summary>
        /// Returns the game object of this object.
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject();
    }
}