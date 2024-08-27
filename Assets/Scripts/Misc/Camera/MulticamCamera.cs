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
        public class MulticamTarget
        {
            [Flags]
            public enum CameraTransition
            {
                Instant = 0,
                SmoothFollow = 1,
                SmoothLookAt = 2,
            }

            public enum CameraMovement
            {
                Static,
                ZoomOut,
                ZoomIn
            }

            [Header("Target")]
            public Transform follow;
            public Transform lookAt;

            [Header("Settings")]
            [Tooltip("In seconds.")] [Range(0, 10f)]
            public float duration = 1f;
            public CameraTransition transition;
            [Tooltip("Only for Smooth transitions.")] [Range(0, 5f)]
            public float transitionDuration;
            public CameraMovement movement;

            public MulticamTarget(Transform follow, Transform lookAt, CameraTransition transition, CameraMovement movement)
            {
                this.follow = follow;
                this.lookAt = lookAt;
                this.transition = transition;
                this.movement = movement;
            }
            public MulticamTarget(Camera camera, out Transform follow, out Transform lookAt)
            {
                follow = new GameObject("Follow").transform;
                lookAt = new GameObject("LookAt").transform;

                follow.position = camera.transform.position;
                follow.rotation = camera.transform.rotation;

                Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, 1000);
                lookAt.position = hit.transform.position;
                lookAt.rotation = hit.transform.rotation;

                this.follow = follow;
                this.lookAt = lookAt;
                transition = CameraTransition.Instant;
                movement = CameraMovement.Static;
            }
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

        public void ChangeCameraState(CameraState newState, CinemachineBlendDefinition.Style blend = CinemachineBlendDefinition.Style.Cut, float blendTime = 0)
        {
            mainCamera.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition(blend, blendTime);

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
        public Transform SetPlayerTarget(Transform follow, Transform lookAt, MulticamTarget.CameraTransition transition = MulticamTarget.CameraTransition.Instant, bool changeState = false)
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