using Solis.Circuit.Interfaces;
using Solis.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Solis.Player
{
    /// <summary>
    /// Player Controller for the Robot Character
    /// </summary>
    public class PlayerControllerRobot : PlayerControllerBase, IMagneticObject, IHeavyObject
    {
        public override CharacterType CharacterType => CharacterType.Robot;
        #region IMagneticObject Implementation
        public void Magnetize(GameObject magnet, Transform anchor)
        {
            magnetAnchor = anchor;
            state = State.Magnetized;
        }

        public void Demagnetize(GameObject magnet, Transform anchor)
        {
            magnetAnchor = null;
        }

        public Transform GetCurrentAnchor()
        {
            return magnetAnchor;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
        #endregion
    }
}