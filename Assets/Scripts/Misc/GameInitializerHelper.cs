using NetBuff;
using Solis.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Solis.Misc
{
    /// <summary>
    /// This class is used to initialize the game.
    /// It is used to load the core scene and initialize the network manager.
    /// </summary>
    public class GameInitializerHelper : MonoBehaviour
    {
        #region Unity Callbacks
        private void OnEnable()
        {
            if (NetworkManager.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            SolisNetworkManager.sceneToLoad = gameObject.scene.name;
            SceneManager.LoadScene("Core");
        }
        #endregion
    }
}