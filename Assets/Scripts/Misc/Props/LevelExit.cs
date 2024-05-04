using NetBuff;
using Solis.Core;
using Solis.Player;
using UnityEngine;

namespace Solis.Misc.Props
{
    /// <summary>
    /// When the player collides with this object, the level will be exited
    /// </summary>
    public class LevelExit : MonoBehaviour
    {
        #region Unity Callbacks
        private void OnTriggerEnter(Collider other)
        {
            var controller = other.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return;
            
            if(NetworkManager.Instance == null)
                return;

            if (!NetworkManager.Instance.IsServerRunning)
                return;

            var game = GameManager.Instance;
            
            if (game == null)
                return;

            game.SaveData.currentLevel++;
            game.LoadLevel();
        }
        #endregion
    }
}