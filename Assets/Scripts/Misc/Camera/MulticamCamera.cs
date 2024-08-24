using Cinemachine;
using UnityEngine;
using NetBuff.Components;

namespace Solis.Misc.Cam
{
    public class MulticamCamera : NetworkBehaviour
    {
        public enum CameraState
        {
            Gameplay,
            Cinematic,
            Dialogue
        }

        #region Inspector Fields
        [Header("REFERENCES")]
        public UnityEngine.Camera mainCamera;
        public CameraState state;
        public Transform target;

        [Header("GAMEPLAY")]
        public CinemachineVirtualCamera gameplayCamera;
        public Transform gameplayFollow;
        public Transform gameplayLookAt;

        [Header("CINEMATIC")]
        public CinemachineVirtualCamera cinematicCamera;

        [Header("DIALOGUE")]
        public CinemachineVirtualCamera dialogueCamera;

        #endregion

        #region Private Fields

        #endregion

        #region Unity Callbacks
        private void Start()
        {

        }

        private void Update()
        {

        }
        #endregion
    }
}