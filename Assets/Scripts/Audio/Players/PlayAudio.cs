using UnityEngine;
using UnityEngine.Serialization;

namespace Solis.Audio.Players
{
    public class PlayAudio : MonoBehaviour
    {
        [FormerlySerializedAs("_audioName")] 
        public string audioName;

        public void Play(string audio)
        {
            Debug.Log("PlayAudio");
            AudioSystem.Instance.PlayVfx(audio);
        }

        public void PlayAudioName()
        {
            AudioSystem.PlayVfxStatic(audioName);
        }
    }
}