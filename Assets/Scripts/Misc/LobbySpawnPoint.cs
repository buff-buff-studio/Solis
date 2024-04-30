using NetBuff.Misc;
using UnityEngine;

namespace Solis.Misc
{
    /// <summary>
    /// Defines a spawn point in the lobby.
    /// </summary>
    public class LobbySpawnPoint : MonoBehaviour
    {
        #region Inspector Fields
        [ServerOnly]
        [SerializeField, HideInInspector]
        public int occupiedBy = -1;
        #endregion
    }
}