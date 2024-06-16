using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

public class LevelCutscene : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField]
    internal CinemachineVirtualCamera virtualCamera;
    [SerializeField]
    private ParticleSystem particleSystem;
    private CinemachineTrackedDolly _dollyTrack;
    private Transform _lookAt;

    [Space]
    [Header("SETTINGS")]
    [SerializeField]
    [Range(1,60)]
    private float duration = 5f;
    [SerializeField]
    [Range(1,10)]
    private float endDuration = 1f;

    [Space]
    [Header("STATE")]
    [SerializeField] [Range(0,1)] private float position;
    [SerializeField] private float ending;

    public static bool IsPlaying = false;
    public static event Action OnCinematicStarted;
    public static event Action OnCinematicEnded;
    
    private bool _isPaused = false;

#if UNITY_EDITOR
    private bool _isPreview = false;
#endif

    private void Awake()
    {
        IsPlaying = true;
        OnCinematicStarted?.Invoke();
        _dollyTrack = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        _lookAt = virtualCamera.m_LookAt;

        ending = 0;
        position = 0;

#if UNITY_EDITOR
        _isPreview = gameObject.name.Contains(" (Preview)");
        if (_isPreview)
            Selection.activeGameObject = gameObject;
        else
            gameObject.name = gameObject.name.Replace(" (Preview)", "");
#endif
    }

    private void OnEnable()
    {
        PauseManager.OnPause += isPaused =>
        {
            _isPaused = isPaused;
        };
    }

    private void OnDisable()
    {
        PauseManager.OnPause -= isPaused =>
        {
            _isPaused = isPaused;
        };
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
        if(_isPaused) return;
        
        if (position < 1)
        {
            position += Time.fixedDeltaTime / duration;
            _dollyTrack.m_PathPosition = position;

        }
        else if (ending < 1)
        {
            if (ending == 0)
            {
                var ps = Instantiate(particleSystem, _lookAt.position, Quaternion.identity);
                var sh = ps.shape;
                var mn = ps.main;
                mn.duration = endDuration-.5f;
                sh.meshRenderer = _lookAt.GetComponentInChildren<MeshRenderer>();
                ps.Play();
                Destroy(ps.gameObject, endDuration);
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

#if UNITY_EDITOR
        if (!_isPreview) return;
        EditorApplication.ExitPlaymode();
#endif
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
    protected internal void Preview()
    {
        gameObject.name += " (Preview)";
        EditorApplication.EnterPlaymode();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>()!.m_PathPosition = position;
        gameObject.name = gameObject.name.Replace(" (Preview)", "");
    }
#endif

    [System.Serializable]
    public struct PointOfInterest
    {
        public Transform position;
        public float roll;
    }
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
                cutscene.virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = GameObject.FindObjectOfType<CinemachinePathBase>();
            }
            return;
        }
        if(Application.isPlaying)
        {
            if(!LevelCutscene.IsPlaying)
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
        else if (GUILayout.Button("Preview"))
        {
            cutscene.Preview();
        }
    }
}
#endif