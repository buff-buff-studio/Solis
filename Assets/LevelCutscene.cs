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

    private void Awake()
    {
        _lookAt = virtualCamera.m_LookAt;
        _dollyTrack = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();

        ending = 0;
        position = 0;
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
                sh.mesh = _lookAt.GetComponentInChildren<MeshFilter>().sharedMesh;
                Destroy(ps.gameObject, ending);
            }
            ending += Time.fixedDeltaTime / endDuration;
        }
        else
        {
            virtualCamera.enabled = false;
            this.enabled = false;
        }
    }
}
