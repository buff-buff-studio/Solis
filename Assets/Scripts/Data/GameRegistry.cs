using System;
using Solis.Misc;
using UnityEngine;

namespace Solis.Data
{
    [CreateAssetMenu(fileName = "Game Registry", menuName = "Solis/Game/Registry")]
    public class GameRegistry : ScriptableObject
    {
        #region Public Fields
        public SceneRef sceneQuit;
        public SceneRef sceneLobby;
        public SceneRef sceneCutscene;
        
        public LevelInfo[] levels = Array.Empty<LevelInfo>();
        #endregion
    }
}