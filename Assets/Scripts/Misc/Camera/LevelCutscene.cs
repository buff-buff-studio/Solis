using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Solis.Misc.Cam
{
    public class LevelCutscene : MonoBehaviour
    {
        [Header("REFERENCES")] [SerializeField]
        internal CinemachineVirtualCamera virtualCamera;

        [Space] [Header("SETTINGS")] [SerializeField]
        private bool playOnAwake = true;

        [SerializeField] [Range(0, 60)] private float duration = 5f;
        [SerializeField] [Range(0, 10)] private float endDuration = 1f;

        [Space]
        [Header("CINEMATIC")]
        [SerializeField]
        private int currentRoll;
        [SerializeField]
        private SerializedDictionary<int, List<MulticamCamera.MulticamTarget>> rolls;

        [Space] [Header("STATE")] [SerializeField] [Range(0, 1)]
        private float position;

        [SerializeField] private float ending;

        public static bool IsPlaying = false;
        public static event Action OnCinematicStarted;
        public static event Action OnCinematicEnded;

        private bool _isPaused = false;

        private void Awake()
        {
            IsPlaying = true;
            OnCinematicStarted?.Invoke();

            ending = 0;
            position = 0;

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

            if (position < 1)
            {
                position += Time.fixedDeltaTime / duration;
            }
            else if (ending < 1)
            {
                if (ending == 0)
                {
                    //Do something when the cutscene ends before switching back to gameplay
                }

                ending += Time.fixedDeltaTime / endDuration;
            }
            else
            {
                Stop();
            }
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
            ending = 0;
            position = 0;
            virtualCamera.enabled = true;
            this.enabled = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            var cam = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            if (cam != null)
                cam.m_PathPosition = position;
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
            if (cutscene.virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>() == null)
            {
                EditorGUILayout.HelpBox("Virtual Camera Body type must be Tracked Dolly", MessageType.Error);
                if (GUILayout.Button("Fix it"))
                {
                    var td = cutscene.virtualCamera.AddCinemachineComponent<CinemachineTrackedDolly>();
                    td.m_PositionUnits = CinemachinePathBase.PositionUnits.Normalized;
                }

                return;
            }

            base.OnInspectorGUI();
            GUILayout.Space(20);
            if (cutscene.virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path == null)
            {
                EditorGUILayout.HelpBox("Tracked Dolly Path is missing", MessageType.Warning);
                if (GUILayout.Button("Locate Path"))
                {
                    cutscene.virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path =
                        GameObject.FindObjectOfType<CinemachinePathBase>();
                }

                return;
            }

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

            }
        }
    }
#endif
}