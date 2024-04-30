using UnityEngine;

namespace Solis.Data.Emotes
{
    /// <summary>
    /// Represents a balloon emote, that can be used in-gameplay, displaying a balloon with an icon
    /// </summary>
    [CreateAssetMenu(fileName = "Balloon Emote", menuName = "Solis/Emote/Balloon")]
    public class EmoteBalloon : Emote
    {
        #region Inspector Fields
        [Header("REFERENCES")]
        public Sprite humanSprite;
        public Sprite robotSprite;
        #endregion
    }
}