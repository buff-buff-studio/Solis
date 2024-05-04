using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Solis.Audio
{
    /// <summary>
    /// Used to store the audio entries for the game.
    /// </summary>
    [CreateAssetMenu(menuName = "Solis/Audio/Audio Palette", fileName = "AudioPalette")]
    public class AudioPalette : ScriptableObject
    {
        #region Inspector Fields
        [SerializeField]
        private SerializedDictionary<string, Audio> audios = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the audio with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Audio GetAudio(string key)
        {
            return audios[key];
        }
        
        /// <summary>
        /// Returns true if the key exists in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return audios.ContainsKey(key);
        }
        
        /// <summary>
        /// Tries to get the audio with the given key.
        /// Returns true if the key exists in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="audio"></param>
        /// <returns></returns>
        public bool TryGetAudio(string key, out Audio audio)
        {
            return audios.TryGetValue(key, out audio);
        }
        #endregion
    }
}