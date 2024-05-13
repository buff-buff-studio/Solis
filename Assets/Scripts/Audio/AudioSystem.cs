﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Solis.Audio
{
    /// <summary>
    /// Used to play audio in the game.
    /// </summary>
    public class AudioSystem : MonoBehaviour
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the current instance of the audio system.
        /// </summary>
        public static AudioSystem Instance { get; private set; }
        #endregion
        
        #region Inspector Fields
        [Header("REFERENCES")]
        public AudioPalette[] audioPalettes = Array.Empty<AudioPalette>();
        
        [Header("SETTINGS")]
        public int audioSourcePoolSize = 10;
        #endregion

        #region Private Fields
        [SerializeField, HideInInspector]
        private List<AudioSource> freeAudioSources = new();
        
        [SerializeField, HideInInspector]
        private List<AudioPlayer> audioPlayers = new();
        
        [SerializeField, HideInInspector]
        private List<AudioMusicTransition> musicTransitions = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the source that is playing the current music.
        /// </summary>
        public AudioPlayer MusicPlayer => audioPlayers.FirstOrDefault(player => player.AudioType == AudioType.Music);
        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            for (var i = 0; i < audioSourcePoolSize; i++)
                _CreateAudioSource(i);
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            for (var i = audioPlayers.Count - 1; i >= 0; i--)
            {
                var player = audioPlayers[i];

                if (player.AudioSource == null)
                {
                    audioPlayers.RemoveAt(i);
                    continue;
                }

                if (!player.HasEnded) 
                    continue;
                
                Kill(player);
            }

            var time = Time.deltaTime;
            for (var i = musicTransitions.Count - 1; i >= 0; i--)
            {
                if (musicTransitions[i].Tick(time))
                    musicTransitions.RemoveAt(i);
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Starts playing a music. The transition parameter is the time in seconds that the current music will take to fade out.
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public AudioPlayer PlayMusic(string audioName, float transition = 0)
        {
            var currentMusic = MusicPlayer;
            var player = _CreatePlayer(audioName, AudioType.Music);
            player.Play(true);
            
            if (currentMusic == null)
                return player;
            
            musicTransitions.Add(new AudioMusicTransition(currentMusic, player, transition));
            return player;
        }
        
        /// <summary>
        /// Starts playing a sound effect. The loop parameter is used to determine if the sound will loop or not.
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public AudioPlayer PlayVfx(string audioName, bool loop = false)
        {
            var player = CreateVfx(audioName);
            return player?.Play(loop);
        }
        
        /// <summary>
        /// Creates a sound effect player, without playing it.
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        public AudioPlayer CreateVfx(string audioName)
        {
            return _CreatePlayer(audioName, AudioType.Vfx);
        }
        
        /// <summary>
        /// Kills a player, stopping the audio and returning the audio source to the pool.
        /// </summary>
        /// <param name="player"></param>
        public void Kill(AudioPlayer player)
        {
            player.Stop();
            
            var t = player.AudioSource.transform;
            t.parent = transform;
            t.localPosition = Vector3.zero;
            t.gameObject.SetActive(false);
            
            audioPlayers.Remove(player);
            freeAudioSources.Add(player.AudioSource);
        }
        
        /// <summary>
        /// Kills all players, stopping all audio and returning all audio sources to the pool.
        /// </summary>
        public void KillAll()
        {
            for (var i = audioPlayers.Count - 1; i >= 0; i--)
            {
                Kill(audioPlayers[i]);
            }
        }
        
        /// <summary>
        /// Kills all sound effects players, stopping all sound effects and returning all audio sources to the pool.
        /// </summary>
        public void KillAllVfx()
        {
            for (var i = audioPlayers.Count - 1; i >= 0; i--)
            {
                if (audioPlayers[i].AudioType == AudioType.Vfx)
                    Kill(audioPlayers[i]);
            }
        }
        #endregion
        
        #region Private Methods
        private AudioSource _GetFreeAudioSource()
        {
            if (freeAudioSources.Count == 0)
            {
                _CreateAudioSource(-1);
            }

            var audioSource = freeAudioSources[0];
            freeAudioSources.RemoveAt(0);
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        
        private void _CreateAudioSource(int n)
        {
            var go = new GameObject($"AudioSource {n}");
            go.SetActive(false);
            go.transform.SetParent(transform);
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.transform.localPosition = Vector3.zero;
            freeAudioSources.Add(audioSource);
        }
        
        private AudioPlayer _CreatePlayer(string audioName, AudioType type)
        {
            foreach (var palette in audioPalettes)
            {
                if (palette.TryGetAudio(audioName, out var clip))
                {
                    return _CreatePlayer(clip.clip, type, clip.volume);
                }
            }
            
            return null;
        }
        
        private AudioPlayer _CreatePlayer(AudioClip clip, AudioType type, float volume = 1)
        {
            var source = _GetFreeAudioSource();
            
            var player = new AudioPlayer(type, clip, source, this, volume);
            audioPlayers.Add(player);
            
            return player;
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Starts playing a music. The transition parameter is the time in seconds that the current music will take to fade out.
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="transition"></param>
        public static void PlayMusicStatic(string audioName, float transition = 0)
        {
            Instance.PlayMusic(audioName, transition);
        }
        
        /// <summary>
        /// Starts playing a sound effect. The loop parameter is used to determine if the sound will loop or not.
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="loop"></param>
        public static void PlayVfxStatic(string audioName, bool loop = false)
        {
            Instance.PlayVfx(audioName, loop);
        }
        
        /// <summary>
        /// Creates a sound effect player, without playing it.
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        public static AudioPlayer CreateVfxStatic(string audioName)
        {
            return Instance.CreateVfx(audioName);
        }
        #endregion
    }
}