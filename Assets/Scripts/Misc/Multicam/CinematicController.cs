using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using NetBuff.Components;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Solis.Misc.Multicam
{
    [RequireComponent(typeof(Animation))]
    [Icon("Assets/Editor/Multicam/Icons/CinematicControllerIcon.png")]
    public class CinematicController : MonoBehaviour
    {
        [Serializable]
        public class CinematicRoll
        {
            public string name;
            [FormerlySerializedAs("followAnimation")] public AnimationClip clip;
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

        [Header("REFERENCES")]
        [SerializeField]
        internal CinemachineVirtualCamera virtualCamera;
        [SerializeField]
        internal Animation animation;

        public Transform Camera => virtualCamera.transform;

        [Space] [Header("SETTINGS")] [SerializeField]
        private bool playOnAwake = true;

        [Space]
        [Header("CINEMATIC")]
        public int currentRoll;
        [SerializeField]
        private List<CinematicRoll> rolls;
        public CinematicRoll CurrentRoll => rolls[currentRoll];
        public List<string> GetRollsName => rolls.ConvertAll(roll => roll.name);

        [Space] [Header("STATE")]
        [SerializeField] private float rollTime = 1f;
        [SerializeField] private float frameTime = 1f;
        [Space(1)]
        [SerializeField] private float duration = 5f;
        [SerializeField] private float frameDuration = 1f;
        [SerializeField] private float frameTransitionDuration = 1f;
        [SerializeField] private bool isTransitioning = false;

        private Transform _follow, _lookAt;

        public static bool IsPlaying = false;
        public static event Action OnCinematicStarted;
        public static event Action OnCinematicEnded;

        private bool _isPaused = false;

        private void Awake()
        {
            TryGetComponent(out animation);

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

        }

        public void Play(int roll)
        {
            currentRoll = roll;

            var clip = BakeAnimation();

            animation.AddClip(rolls[currentRoll].clip, rolls[currentRoll].name);
            animation.Play(rolls[currentRoll].name);

            _isPaused = false;
        }

        public AnimationClip BakeAnimation()
        {
            if (rolls[currentRoll].clip == null)
            {
                var c = new AnimationClip
                {
                    name = SceneManager.GetActiveScene().name + " - " + rolls[currentRoll].name
                };
                AssetDatabase.CreateAsset(c, $"Assets/Animations/Cinematic/{c.name}.anim");
                rolls[currentRoll].clip = c;
            }
            var clip = rolls[currentRoll].clip;

            var followPos = new []
            {
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve()
            };
            var lookAtPos = new []
            {
                new AnimationCurve(),
                new AnimationCurve(),
                new AnimationCurve()
            };

            var s = 0f;
            for (var i = 0; i < rolls[currentRoll].framing.Count; i++)
            {
                var frame = rolls[currentRoll].framing[i];
                var sStart = frame.transitionDuration == 0 ? s : s + frame.transitionDuration;
                var sEnd = sStart + frame.duration - 0.017f;

                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.follow.position.x, sEnd, frame.follow.position.x).keys)
                    followPos[0].AddKey(keyframe);
                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.follow.position.y, sEnd, frame.follow.position.y).keys)
                    followPos[1].AddKey(keyframe);
                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.follow.position.z, sEnd, frame.follow.position.z).keys)
                    followPos[2].AddKey(keyframe);

                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.lookAt.position.x, sEnd, frame.lookAt.position.x).keys)
                    lookAtPos[0].AddKey(keyframe);
                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.lookAt.position.y, sEnd, frame.lookAt.position.y).keys)
                    lookAtPos[1].AddKey(keyframe);
                foreach (var keyframe in AnimationCurve.Linear(sStart, frame.lookAt.position.z, sEnd, frame.lookAt.position.z).keys)
                    lookAtPos[2].AddKey(keyframe);

                s += frame.duration + frame.transitionDuration;
            }

            clip.ClearCurves();
            clip.SetCurve("Follow", typeof(Transform), "localPosition.x", followPos[0]);
            clip.SetCurve("Follow", typeof(Transform), "localPosition.y", followPos[1]);
            clip.SetCurve("Follow", typeof(Transform), "localPosition.z", followPos[2]);

            clip.SetCurve("LookAt", typeof(Transform), "localPosition.x", lookAtPos[0]);
            clip.SetCurve("LookAt", typeof(Transform), "localPosition.y", lookAtPos[1]);
            clip.SetCurve("LookAt", typeof(Transform), "localPosition.z", lookAtPos[2]);
            clip.legacy = true;
            return clip;
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
            //IsPlaying = true;
            //rollTime = 0;
            //frameTime = 0;
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

        public void UpdateFrame()
        {
            if (rolls == null) rolls = new List<CinematicRoll>();
            if (rolls.Count == 0) rolls.Add(new CinematicRoll {name = "Roll 1"});
            if (rolls[currentRoll].framing == null)
            {
                AddFrame();
                return;
            }
            var updatedFrame = new MulticamCamera.MulticamTarget(
                    SceneView.lastActiveSceneView.camera,
                    out var follow, out var lookAt);

            follow.name = $"{rolls[currentRoll].framing.Count} - Follow";
            lookAt.name = $"{rolls[currentRoll].framing.Count} - LookAt";

            rolls[currentRoll].framing[rolls[currentRoll].currentFrame] = updatedFrame;

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

            SetCameraToCurrentFrame();
        }

        public void SetCameraToCurrentFrame()
        {
            virtualCamera.m_Follow = rolls[currentRoll].GetFollow();
            virtualCamera.m_LookAt = rolls[currentRoll].GetLookAt();
        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CinematicController))]
    public class LevelCutsceneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cutscene = (CinematicController)target;

            base.OnInspectorGUI();
            GUILayout.Space(20);

            if (Application.isPlaying)
            {
                if (!CinematicController.IsPlaying)
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
            else
            {
                GUILayout.Label("Editing Roll: " + cutscene.CurrentRoll.name, EditorStyles.boldLabel);
                GUILayout.Label("Frames: " + cutscene.CurrentRoll.framing.Count, EditorStyles.miniLabel);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Frame", GUILayout.Width(120), GUILayout.Height(40)))
                    cutscene.AddFrame();
                if (GUILayout.Button($"Update Frame {cutscene.CurrentRoll.currentFrame}", GUILayout.Width(120), GUILayout.Height(40)))
                    cutscene.UpdateFrame();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Bake Roll Animation", GUILayout.Width(120), GUILayout.Height(40)))
                    cutscene.BakeAnimation();
                if (GUILayout.Button("Bake All Animations", GUILayout.Width(120), GUILayout.Height(40)))
                    cutscene.BakeAnimation();
                GUILayout.EndHorizontal();
            }
        }
    }
#endif
}
