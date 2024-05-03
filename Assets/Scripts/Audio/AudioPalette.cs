using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Used to store the audio entries for the game.
    /// </summary>
    [CreateAssetMenu(menuName = "Solis/Audio/Audio Palette", fileName = "AudioPalette")]
    public class AudioPalette : ScriptableObject
    {
        #region Inspector Fields
        [SerializeField]
        private SerializedDictionary<string, AudioClip> clips = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the audio clip with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AudioClip GetClip(string key)
        {
            return clips[key];
        }
        
        /// <summary>
        /// Returns true if the key exists in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return clips.ContainsKey(key);
        }
        
        /// <summary>
        /// Tries to get the audio clip with the given key.
        /// Returns true if the key exists in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        public bool TryGetClip(string key, out AudioClip clip)
        {
            return clips.TryGetValue(key, out clip);
        }
        #endregion
    }
}