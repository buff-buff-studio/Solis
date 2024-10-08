using System;
using System.Collections;
using UnityEngine;

namespace Solis.Audio.Players
{
    public class SolisAmbiencePlayer : MonoBehaviour
    {
        public string audioName;
        private AudioPlayer _audioPlayer = null;
        
        public void OnEnable()
        {
            _Play();
        }

        private void _Play()
        {
            if (_audioPlayer != null)
                return;
            
            _audioPlayer = AudioSystem.CreateVfxStatic(audioName);
            _audioPlayer.Play(true);
        }
        
        public void OnDisable()
        {
            if (_audioPlayer == null)
                return;
            
            StartCoroutine(_Stop(_audioPlayer));
            _audioPlayer = null;
        }
        
        private IEnumerator _Stop(AudioPlayer player)
        {
            var timer = 0f;
            var volume = player.Volume;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                player.SetVolume(Mathf.Lerp(volume, 0f, timer));
                yield return null;
            }
            
            player.Stop();
        }
    }
}