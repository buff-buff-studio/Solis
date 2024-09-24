using System;
using Cinemachine;
using UnityEngine;
using NetBuff.Components;
using Solis.Data;
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

        [Header("CINEMATIC")]
        public CinemachineVirtualCamera cinematicCamera;

        [Header("DIALOGUE")]
        public CinemachineVirtualCamera dialogueCamera;

        private CinemachineBrain _cinemachineBrain;

        #endregion

        private Transform ram, nina, diluvio;

        #region Unity Callbacks
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        }
        #endregion

        #region Public Methods

        public void ChangeCameraState(CameraState newState, CinemachineBlendDefinition.Style blend = CinemachineBlendDefinition.Style.Cut, float blendTime = 0)
        {
            _cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(blend, blendTime);

            gameplayCamera.gameObject.SetActive(newState == CameraState.Gameplay);
            dialogueCamera.gameObject.SetActive(newState == CameraState.Dialogue);

            if(cinematicCamera != null) cinematicCamera.gameObject.SetActive(newState == CameraState.Cinematic);
            else if(newState == CameraState.Cinematic)
            {
                Debug.LogError("Cinematic camera is not set");
                ChangeCameraState(CameraState.Gameplay);
                return;
            }

            state = newState;
        }
        public Transform SetPlayerTarget(Transform follow, Transform lookAt)
        {
            gameplayCamera.Follow = follow;
            gameplayCamera.LookAt = lookAt;

            if(!cinematicCamera)
            {
                ChangeCameraState(CameraState.Gameplay);
                Debug.Log("Gameplay camera is on");
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
            else
            {
                ChangeCameraState(CameraState.Gameplay);
            }
        }

        public void SetDialogueFocus(CharacterTypeEmote type)
        {
            switch (type)
            {
                case CharacterTypeEmote.Nina:
                    if(!nina)
                    {
                        var player = FindFirstObjectByType<PlayerControllerHuman>();
                        if (player) nina = player.dialogueLookAt;
                        else
                        {
                            Debug.LogError("Nina is not found to focus on dialogue");
                            dialogueCamera.LookAt = gameplayCamera.LookAt;
                            dialogueCamera.Follow = gameplayCamera.Follow;
                            break;
                        }
                    }
                    dialogueCamera.LookAt = nina;
                    dialogueCamera.Follow = nina;
                    break;
                case CharacterTypeEmote.RAM:
                    if(!ram)
                    {
                        var player = FindFirstObjectByType<PlayerControllerRobot>();
                        if (player) ram = player.dialogueLookAt;
                        else
                        {
                            Debug.LogError("RAM is not found to focus on dialogue");
                            dialogueCamera.LookAt = gameplayCamera.LookAt;
                            dialogueCamera.Follow = gameplayCamera.Follow;
                            break;
                        }
                    }
                    dialogueCamera.LookAt = ram;
                    dialogueCamera.Follow = ram;
                    break;
                case CharacterTypeEmote.Diluvio:
                    if(!diluvio)
                    {
                        var player = FindFirstObjectByType<PlayerControllerRobot>();
                        if (player) diluvio = player.diluvioPosition;
                        else
                        {
                            Debug.LogError("Diluvio is not found to focus on dialogue");
                            dialogueCamera.LookAt = gameplayCamera.LookAt;
                            dialogueCamera.Follow = gameplayCamera.Follow;
                            break;
                        }
                    }
                    dialogueCamera.LookAt = diluvio;
                    dialogueCamera.Follow = diluvio;
                    break;
                default:
                    Debug.LogError("This focus on dialogue is not implemented yet, the camera will follow the player instead.");
                    dialogueCamera.LookAt = gameplayCamera.LookAt;
                    dialogueCamera.Follow = gameplayCamera.Follow;
                    break;
            }

            ChangeCameraState(CameraState.Dialogue, CinemachineBlendDefinition.Style.EaseInOut, 1);
        }

        #endregion
    }
}