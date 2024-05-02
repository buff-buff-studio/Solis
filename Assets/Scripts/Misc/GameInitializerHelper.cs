using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using NetBuff;
using Solis.Core;
using Solis.Data;
#endif

namespace Solis.Misc
{
    /// <summary>
    /// This class is used to initialize the game.
    /// It is used to load the core scene and initialize the network manager.
    /// </summary>
    public class GameInitializerHelper : MonoBehaviour
    {
        #if UNITY_EDITOR
        #region Inspector Fields
        public CharacterType defaultType = CharacterType.Human;
        #endregion
        
        #region Unity Callbacks
        private void OnEnable()
        {
            if (NetworkManager.Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            SolisNetworkManager.sceneToLoad = gameObject.scene.name;
            SolisNetworkManager.defaultType = defaultType;
            SceneManager.LoadScene("Core");
        }
        #endregion
        #endif
    }
}