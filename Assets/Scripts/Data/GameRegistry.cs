using System;
using Solis.Misc;
using UnityEngine;

namespace Solis.Data
{
    /// <summary>
    /// Used to store game specific data like scenes, levels etc.
    /// </summary>
    [CreateAssetMenu(fileName = "Game Registry", menuName = "Solis/Game/Registry")]
    public class GameRegistry : ScriptableObject
    {
        #region Inspector Fields
        public SceneRef sceneQuit;
        public SceneRef sceneLobby;
        public SceneRef sceneCutscene;
        
        public LevelInfo[] levels = Array.Empty<LevelInfo>();
        #endregion
    }
}