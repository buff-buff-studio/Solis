using System.Collections;
using UnityEngine;

namespace Solis.Audio.Players
{
    public class SolisMusicPlayer : MonoBehaviour
    {
        public string musicName;
        
        public float playerAfterStart;
        public float playerAfterEnd;
        
        public void OnEnable()
        {
            StartCoroutine(_PlayAfter(0));
        }
        
        private IEnumerator _PlayAfter(float time)
        {
            Debug.Log("Playing music after " + musicName + " " + time + " seconds");
            yield return new WaitForSeconds(time);
            
            var musicPlayer = AudioSystem.Instance.MusicPlayer;

            if (musicPlayer == null)
            {
                musicPlayer = AudioSystem.PlayMusicStatic(musicName, 2f);
            }
            else
            {
                Audio clip = null;
                
                foreach (var palette in AudioSystem.Instance.audioPalettes)
                {
                    if (palette.TryGetAudio(musicName, out var c))
                    {
                        clip = c;
                    }
                }
                
                if (clip == null || musicPlayer.AudioClip != clip.clip)
                    musicPlayer = AudioSystem.PlayMusicStatic(musicName, 2f);
            }


            musicPlayer.AudioSource.loop = false;
            
            musicPlayer.OnEnd = () =>
            {
                StartCoroutine(_PlayAfter(Random.Range(playerAfterStart, playerAfterEnd)));
            };
        }
    }
}