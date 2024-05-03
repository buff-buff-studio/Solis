using Solis.Misc;
using UnityEngine;

namespace Solis.Data
{
    [CreateAssetMenu(fileName = "Level Info", menuName = "Solis/Game/Level Info", order = 0)]
    public class LevelInfo : ScriptableObject
    {
        public string unlocalizedName;
        public SceneRef scene;
        public Sprite preview;
    }
}