using System;
using Cinemachine;
using UnityEngine;
using NetBuff.Components;
using Solis.Player;

namespace Solis.Misc.Multicam
{
    public class MulticamCamera : NetworkBehaviour
    {
        public enum CameraState
        {
            Gameplay,
            Cinematic,
            Dialogue
        }

        public static MulticamCamera Instance { get; private set; }

        #region Inspector Fields
        [Header("REFERENCES")]
        public Camera mainCamera;
        public CameraState state;
        public Transform target;

        [Header("GAMEPLAY")]
        public CinemachineFreeLook gameplayCamera;
        public CinematicFrame playerTarget;

        [Header("CINEMATIC")]
        public CinemachineVirtualCamera cinematicCamera;

        [Header("DIALOGUE")]
        public CinemachineVirtualCamera dialogueCamera;

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
            //dialogueCamera.gameObject.SetActive(newState == CameraState.Dialogue);
            state = newState;
            switch (state)
            {
                case CameraState.Gameplay:
                    //target = playerTarget.lookAt;
                    break;
                case CameraState.Cinematic:
                    break;
                case CameraState.Dialogue:
                    Debug.LogError("Dialogue Camera not implemented yet! Sorry :P");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public Transform SetPlayerTarget(Transform follow, Transform lookAt, CameraTransition transition = CameraTransition.Instant, bool changeState = false)
        {
            playerTarget.behaviour.transition = transition;

            gameplayCamera.Follow = follow;
            gameplayCamera.LookAt = lookAt;

            if(changeState)
            {
                ChangeCameraState(CameraState.Gameplay);
                state = CameraState.Gameplay;
            }
            return mainCamera.transform;
        }

        public void SetCinematic(CinemachineVirtualCamera cinematic, bool changeState = false)
        {
            cinematicCamera = cinematic;

            if(changeState)
            {
                CinematicController.Instance.Play(0);
            }
        }

        #endregion
    }
}