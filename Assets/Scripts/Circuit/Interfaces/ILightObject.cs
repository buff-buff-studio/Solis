using UnityEngine;

namespace Solis.Circuit.Interfaces
{
    public interface ILightObject
    {
        /// <summary>
        /// Returns the game object of this object.
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject();
    }
}