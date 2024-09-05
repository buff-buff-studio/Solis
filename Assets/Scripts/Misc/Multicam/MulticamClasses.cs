using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Solis.Misc.Multicam
{
    #region Enums

    [Flags]
    public enum CameraTransition : int
    {
        Instant = 0,
        Smooth = 1,
        FadeIn = 2,
        FadeOut = 4
    }

    public enum CameraMovement : int
    {
        Static = 0,
        ZoomOut = 1,
        ZoomIn = 2,
        Transition = 4
    }

    #endregion

    [Serializable]
    public class CinematicFrame
    {
        [Header("Target")]
        public Vector3 follow;
        public Vector3 lookAt;

        [Space]
        public CameraBehaviour behaviour;
        public UnityEvent onFrameShow;
        protected internal bool invoked;
        public CameraTransition transition => behaviour.transition;
        public CameraMovement movement => behaviour.movement;
        public float duration => behaviour.duration;
        public float transitionDuration => behaviour.transitionDuration;

        public CinematicFrame(Transform follow, Transform lookAt, CameraTransition transition, CameraMovement movement)
        {
            this.follow = follow.position;
            this.lookAt = lookAt.position;
            this.behaviour = new CameraBehaviour
            {
                duration = 1,
                transition = transition,
                transitionDuration = 0,
                movement = movement
            };
        }
        public CinematicFrame(Camera camera)
        {
            Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, 1000, ~LayerMask.GetMask("InvisibleWall", "Ignore Raycast"));

            this.follow = camera.transform.position;
            this.lookAt = hit.transform.position;
            this.behaviour = new CameraBehaviour
            {
                duration = 1,
                transition = CameraTransition.Instant,
                transitionDuration = 0,
                movement = CameraMovement.Static
            };
        }
    }

    [Serializable]
    public struct CameraBehaviour
    {
        [Tooltip("FadeIn/Out are not implemented yet. Sorry :P")]
        public CameraMovement movement;
        [Tooltip("In seconds.")] [Range(0, 10f)]
        public float duration;

        [Tooltip("ZoomIn/Out are not implemented yet. Sorry :P")]
        public CameraTransition transition;
        [Tooltip("Instant transition if 0.")] [Range(0, 5f)]
        public float transitionDuration;
    }

    [Serializable]
    public class CinematicRoll
    {
        public string name;
        public AnimationClip clip;

        public CinemachineBlendDefinition.Style blend;
        [Tooltip("If the blend is Cut, this value is ignored.")] [Range(0, 5f)]
        public float blendTime;

        [Space]
        [Header("Frames")]
        public int currentFrame;
        public List<CinematicFrame> framing;
        public CinematicFrame CurrentFrame => framing[currentFrame];
        public Vector3 GetFollow()
        {
            return framing[currentFrame].follow;
        }
        public Vector3 GetLookAt()
        {
            return framing[currentFrame].lookAt;
        }
        public CinematicFrame GetFrame()
        {
            return framing[currentFrame];
        }
    }
}