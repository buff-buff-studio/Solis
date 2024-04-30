using UnityEngine;

namespace Solis.Data.Emotes
{
    /// <summary>
    /// Used as base class for all emotes
    /// </summary>
    public abstract class Emote : ScriptableObject
    {
        #region Inspector Fields
        [Header("SETTINGS")]
        public string unlocalizedName;
        public bool canUseInGameplay;
        #endregion
    }
}