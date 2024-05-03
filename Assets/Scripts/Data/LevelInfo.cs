using Solis.Misc;
using UnityEngine;

namespace Solis.Data
{
    /// <summary>
    /// Holds information about a level.
    /// </summary>
    [CreateAssetMenu(fileName = "Level Info", menuName = "Solis/Game/Level Info", order = 0)]
    public class LevelInfo : ScriptableObject
    {
        #region Inspector Fields
        public string unlocalizedName;
        public SceneRef scene;
        public Sprite preview;
        #endregion
    }
}