using System;
using Cinemachine;
using UnityEngine;
using NetBuff.Components;
using Solis.Player;

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

        [Serializable]
        public struct MulticamTarget
        {
            public enum TargetTransition
            {
                Instant,
                Smooth
            }
            [Header("Target")]
            public Transform follow;
            public Transform lookAt;
            [Header("Settings")]
            public TargetTransition transition;
        }

        public static MulticamCamera Instance { get; private set; }

        #region Inspector Fields
        [Header("REFERENCES")]
        public Camera mainCamera;
        public CameraState state;
        public Transform target;

        [Header("GAMEPLAY")]
        public CinemachineFreeLook gameplayCamera;
        public MulticamTarget playerTarget;

        [Header("CINEMATIC")]
        public CinemachineVirtualCamera cinematicCamera;
        public MulticamTarget[] cinematicTargets;

        [Header("DIALOGUE")]
        public CinemachineVirtualCamera dialogueCamera;

        #endregion

        #region Private Fields

        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }
        #endregion

        #region Public Methods

        public void ChangeCameraState(CameraState newState)
        {
            gameplayCamera.gameObject.SetActive(newState == CameraState.Gameplay);
            cinematicCamera.gameObject.SetActive(newState == CameraState.Cinematic);
            dialogueCamera.gameObject.SetActive(newState == CameraState.Dialogue);
            state = newState;

            switch (state)
            {
                case CameraState.Gameplay:
                    target = playerTarget.lookAt;
                    break;
                case CameraState.Cinematic:
                    break;
                case CameraState.Dialogue:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public Transform SetPlayerTarget(Transform follow, Transform lookAt, MulticamTarget.TargetTransition transition = MulticamTarget.TargetTransition.Instant, bool changeState = false)
        {
            playerTarget.follow = follow;
            playerTarget.lookAt = lookAt;
            playerTarget.transition = transition;

            gameplayCamera.Follow = follow;
            gameplayCamera.LookAt = lookAt;

            if(changeState)
            {
                //ChangeCameraState(CameraState.Gameplay);
                state = CameraState.Gameplay;
            }
            return mainCamera.transform;
        }

        public void SetCinematic(CinemachineVirtualCamera cinematic, MulticamTarget[] targets, bool changeState = false)
        {
            cinematicCamera = cinematic;
            cinematicTargets = targets;

            if(changeState)
            {
                //ChangeCameraState(CameraState.Cinematic);
                state = CameraState.Cinematic;
            }
        }

        #endregion
    }
}