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
    private CinemachineVirtualCamera virtualCamera;
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
    [SerializeField] private float position;
    [SerializeField] private float ending;

    public static bool IsPlaying = false;
    public static event Action OnCinematicEnded;

#if UNITY_EDITOR
    private bool _isPreview => gameObject.name.Contains("(Preview)");
#endif

    private void Awake()
    {
        IsPlaying = true;
        _dollyTrack = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        _lookAt = virtualCamera.m_LookAt;

        ending = 0;
        position = 0;

#if UNITY_EDITOR
        if (!_isPreview) return;
        Selection.activeGameObject = gameObject;
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            virtualCamera.enabled = false;
            this.enabled = false;
            OnCinematicEnded?.Invoke();
            IsPlaying = false;
        }
    }

    private void FixedUpdate()
    {
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
        gameObject.name = gameObject.name.Replace(" (Preview)", "");
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
        base.OnInspectorGUI();
        GUILayout.Space(20);
        var cutscene = (LevelCutscene)target;
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