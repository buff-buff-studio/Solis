using System;
using UnityEditor;
using UnityEngine;

namespace Solis.Misc
{
    /// <summary>
    /// Class that rotates the windmill blades
    /// </summary>
    public class WindmillRotator : MonoBehaviour
    {
        [SerializeField]
        private bool playOnAwake = true;
        [SerializeField]
        private Vector3 rotationAxis = Vector3.right;
        [SerializeField]
        private float speed = 10f;
        [SerializeField]
        private float acceleration = 0.1f;

        [SerializeField]
        private AudioSource audioSource;

        public float Power => _currentSpeed / speed;

        private float SpeedMultiplier => (_isRotating ? acceleration : -acceleration) * Time.fixedDeltaTime;
        private float _currentSpeed = 0;
        private bool _isRotating = true;

        private void Awake()
        {
            if(playOnAwake) Play(true);
            else Pause(true);
        }
        private void FixedUpdate()
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed + SpeedMultiplier, 0, speed);
            transform.Rotate(rotationAxis, _currentSpeed * Time.deltaTime);
            UpdateAudio();
        }

        /// <summary>
        /// Updates the audio source based on the current speed of the windmill.
        /// </summary>
        private void UpdateAudio()
        {
            // Check if the audio source is assigned
            if (audioSource)
            {
                // If the current speed is greater than 60, play the audio source if it is not already playing,
                // and adjust the pitch based on the current speed.
                if (_currentSpeed > 60)
                {
                    if (!audioSource.isPlaying) audioSource.Play();
                    audioSource.pitch = _currentSpeed / 130;
                }
                // If the current speed is 60 or less and the audio source is playing, stop the audio source.
                else if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        public void Play(bool forceSpeed = false)
        {
            _isRotating = true;
            _currentSpeed = forceSpeed ? speed : _currentSpeed;
        }

        public void Pause(bool forceSpeed = false)
        {
            _isRotating = false;
            _currentSpeed = forceSpeed ? 0 : _currentSpeed;
        }

        public void ChangeState()
        {
            if(_isRotating) Pause();
            else Play();
        }

        public void ChangeState(bool state, bool forceSpeed = false)
        {
            if(state) Play(forceSpeed);
            else Pause(forceSpeed);
        }
    }
}