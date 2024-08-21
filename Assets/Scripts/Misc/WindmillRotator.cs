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