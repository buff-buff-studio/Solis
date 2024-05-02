using UnityEngine;

namespace Solis.Misc
{
    [CreateAssetMenu(fileName = "Game Scene Registry", menuName = "Solis/Game/Scene Registry")]
    public class GameSceneRegistry : ScriptableObject
    {
        #region Public Fields
        public SceneRef quitScene;
        public SceneRef lobbyScene;
        public SceneRef cutsceneScene;
        
        public SceneRef[] levels;
        #endregion
    }
}