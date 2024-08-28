using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using NetBuff.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Solis.Misc.Cam
{
    public class LevelCutscene : MonoBehaviour
    {
        [Serializable]
        public class CinematicRoll
        {
            public string name;
            public int currentFrame;
            public List<MulticamCamera.MulticamTarget> framing;

            public Transform GetFollow()
            {
                return framing[currentFrame].follow;
            }
            public Transform GetLookAt()
            {
                return framing[currentFrame].lookAt;
            }
            public MulticamCamera.MulticamTarget GetFrame()
            {
                return framing[currentFrame];
            }
        }

        [Header("REFERENCES")] [SerializeField]
        internal CinemachineVirtualCamera virtualCamera;

        [Space] [Header("SETTINGS")] [SerializeField]
        private bool playOnAwake = true;

        [Space]
        [Header("CINEMATIC")]
        [SerializeField]
        private int currentRoll;
        [SerializeField]
        private List<CinematicRoll> rolls;

        [Space] [Header("STATE")]
        [SerializeField] private float rollTime = 1f;
        [SerializeField] private float frameTime = 1f;
        [Space(1)]
        [SerializeField] private float duration = 5f;
        [SerializeField] private float frameDuration = 1f;
        [SerializeField] private float frameTransitionDuration = 1f;
        [SerializeField]private bool isTransitioning = false;

        private Transform _follow, _lookAt;

        public static bool IsPlaying = false;
        public static event Action OnCinematicStarted;
        public static event Action OnCinematicEnded;

        private bool _isPaused = false;

        private void Awake()
        {
            IsPlaying = true;
            OnCinematicStarted?.Invoke();

            _follow = new GameObject("Follow").transform;
            _lookAt = new GameObject("LookAt").transform;
            _follow.SetParent(this.transform);
            _lookAt.SetParent(this.transform);
            virtualCamera.m_Follow = _follow;
            virtualCamera.m_LookAt = _lookAt;

            MulticamCamera.Instance.SetCinematic(virtualCamera, null, playOnAwake);
        }

        private void OnEnable()
        {
            PauseManager.OnPause += isPaused => { _isPaused = isPaused; };
        }

        private void OnDisable()
        {
            PauseManager.OnPause -= isPaused => { _isPaused = isPaused; };
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cutscene"))
            {
                virtualCamera.enabled = false;
                this.enabled = false;
                OnCinematicEnded?.Invoke();
                IsPlaying = false;
            }
        }

        private void FixedUpdate()
        {
            if (_isPaused) return;

            if (duration == 0)
            {
                Stop();
                return;
            }

            if (!isTransitioning)
            {
                if (frameTime >= frameDuration)
                {
                    frameTime = 0;
                    if (rolls[currentRoll].currentFrame + 1 >= rolls[currentRoll].framing.Count)
                    {
                        rolls[currentRoll].currentFrame = 0;
                        Stop();
                        return;
                    }
                    rolls[currentRoll].currentFrame++;
                    frameDuration = rolls[currentRoll].GetFrame().duration;
                    if (rolls[currentRoll].GetFrame().transition != MulticamCamera.MulticamTarget.CameraTransition.Instant)
                    {
                        isTransitioning = true;
                        frameTransitionDuration = rolls[currentRoll].GetFrame().transitionDuration;
                    }
                }
                else
                {
                //Talvez converter em animator?
                    _follow.position = rolls[currentRoll].GetFollow().position;
                    _follow.rotation = rolls[currentRoll].GetFollow().rotation;
                    _lookAt.position = rolls[currentRoll].GetLookAt().position;
                    _lookAt.rotation = rolls[currentRoll].GetLookAt().rotation;
                }
            }
            else
            {
                if (frameTime >= frameTransitionDuration)
                {
                    frameTime = 0;
                    isTransitioning = false;
                }
                else
                {
                    var transition = frameTime / frameTransitionDuration;
                    if ((rolls[currentRoll].GetFrame().transition &
                         MulticamCamera.MulticamTarget.CameraTransition.SmoothFollow) != 0)
                    {
                        _follow.position =
                            Vector3.Lerp(
                                rolls[currentRoll].framing[rolls[currentRoll].currentFrame - 1].follow.position,
                                rolls[currentRoll].GetFollow().position, transition);
                        _follow.rotation =
                            Quaternion.Lerp(
                                rolls[currentRoll].framing[rolls[currentRoll].currentFrame - 1].follow.rotation,
                                rolls[currentRoll].GetFollow().rotation, transition);
                    }
                    if ((rolls[currentRoll].GetFrame().transition &
                         MulticamCamera.MulticamTarget.CameraTransition.SmoothLookAt) != 0)
                    {
                        _lookAt.position =
                            Vector3.Lerp(
                                rolls[currentRoll].framing[rolls[currentRoll].currentFrame - 1].lookAt.position,
                                rolls[currentRoll].GetLookAt().position, transition);
                        _lookAt.rotation =
                            Quaternion.Lerp(
                                rolls[currentRoll].framing[rolls[currentRoll].currentFrame - 1].lookAt.rotation,
                                rolls[currentRoll].GetLookAt().rotation, transition);
                    }
                }
            }
            frameTime += Time.fixedDeltaTime;
            rollTime += Time.fixedDeltaTime;
        }

        public void Play(int roll)
        {
            currentRoll = roll;
            var seconds = 0;
            rolls[currentRoll].framing.ForEach(frame =>
            {
                seconds += (int)frame.duration;
                seconds += (int)frame.transitionDuration;
            });

            rollTime = 0;
            frameTime = 0;

            duration = seconds;
            rolls[currentRoll].currentFrame = 0;
            frameDuration = rolls[currentRoll].framing[0].duration;
            frameTransitionDuration = 0;

            _isPaused = false;
        }

        public void Stop()
        {
            virtualCamera.enabled = false;
            this.enabled = false;
            OnCinematicEnded?.Invoke();
            IsPlaying = false;
        }

        protected internal void Reset()
        {
            IsPlaying = true;
            rollTime = 0;
            frameTime = 0;
            virtualCamera.enabled = true;
            this.enabled = true;
            Play(currentRoll);
        }

#if UNITY_EDITOR
        public void AddFrame()
        {
            if (rolls == null) rolls = new List<CinematicRoll>();
            if (rolls.Count == 0) rolls.Add(new CinematicRoll {name = "Roll 1"});
            if (rolls[currentRoll].framing == null) rolls[currentRoll].framing = new List<MulticamCamera.MulticamTarget>();
            rolls[currentRoll].framing.Add(
                new MulticamCamera.MulticamTarget(
                    SceneView.lastActiveSceneView.camera,
                    out var follow, out var lookAt));

            follow.name = $"{rolls[currentRoll].framing.Count} - Follow";
            lookAt.name = $"{rolls[currentRoll].framing.Count} - LookAt";

            OrganizeRolls();
            var parent = transform.Find("Rolls");
            var roll = parent.Find(rolls[currentRoll].name);
            follow.SetParent(roll);
            lookAt.SetParent(roll);
        }

        public void OrganizeRolls()
        {
            if (rolls == null) return;

            var parent = transform.Find("Rolls");
            if (parent == null)
            {
                parent = new GameObject("Rolls").transform;
                parent.SetParent(transform);
            }
            parent.SetAsFirstSibling();
            var i = 0;
            rolls.ForEach(roll =>
            {
                var rollObj = parent.Find(roll.name);
                if (rollObj == null)
                {
                    rollObj = new GameObject(roll.name).transform;
                    rollObj.SetParent(parent);
                }
                rollObj.SetSiblingIndex(i);
                i++;
            });
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            currentRoll = Mathf.Clamp(currentRoll, 0, rolls.Count - 1);
            rolls?.ForEach(roll =>
            {
                roll.currentFrame = Mathf.Clamp(roll.currentFrame, 0, roll.framing.Count - 1);
            });

            virtualCamera.m_Follow = rolls[currentRoll].GetFollow();
            virtualCamera.m_LookAt = rolls[currentRoll].GetLookAt();
        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(LevelCutscene))]
    public class LevelCutsceneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cutscene = (LevelCutscene)target;

            base.OnInspectorGUI();
            GUILayout.Space(20);

            if (Application.isPlaying)
            {
                if (!LevelCutscene.IsPlaying)
                {
                    if (GUILayout.Button("Play"))
                    {
                        cutscene.Reset();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop"))
                    {
                        cutscene.Stop();
                    }
                }
            }
            else if (GUILayout.Button("Add Position"))
            {
                cutscene.AddFrame();
            }
        }
    }
#endif
}
