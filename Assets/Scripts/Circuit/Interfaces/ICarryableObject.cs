using UnityEngine;

namespace Solis.Circuit.Interfaces
{
    public interface ICarryableObject
    {
        /// <summary>
        /// Returns the game object of this object.
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject();
    }
}