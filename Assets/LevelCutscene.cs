using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class LevelCutscene : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    [SerializeField]
    private ParticleSystem particleSystem;
    private CinemachineTrackedDolly _dollyTrack;
    private Transform _lookAt;

    [SerializeField]
    [Range(1,60)]
    private float duration = 5f;
    [SerializeField]
    [Range(1,10)]
    private float endDuration = 1f;

    [SerializeField] private float position, ending;

    public static bool IsPlaying = false;
    public static event Action OnCinematicEnded;

    private void Awake()
    {
        IsPlaying = true;
        _lookAt = virtualCamera.m_LookAt;
        _dollyTrack = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();

        ending = 0;
        position = 0;
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
            virtualCamera.enabled = false;
            this.enabled = false;
            OnCinematicEnded?.Invoke();
            IsPlaying = false;
        }
    }
}
