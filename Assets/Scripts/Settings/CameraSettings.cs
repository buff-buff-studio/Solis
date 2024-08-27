using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Solis.Settings;
using Solis.Data;
using Solis.Misc.Cam;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSettings : MonoBehaviour
{
    [SerializeField]
    private CinemachineFreeLook freeLookCamera;
    [SerializeField]
    private Volume volumePFX;
    [SerializeField]
    private SettingsData settingsData;

    private const float CamSenseX = 450;
    private const float CamSenseY = 3.5f;
    
    private bool isPaused;
    private float _senseX, _senseY;

    private void Awake()
    {
        ApplyCameraSettings();
        SettingsManager.OnSettingsChanged += ApplyCameraSettings;
    }
    
    private void OnEnable()
    {
        SettingsManager.OnSettingsChanged += ApplyCameraSettings;
        
        PauseManager.OnPause += OnPause;
        LevelCutscene.OnCinematicStarted += OnLevelCutsceneOnOnCinematicStarted;
        LevelCutscene.OnCinematicEnded += OnLevelCutsceneOnOnCinematicEnded;
    }

    private void OnDestroy()
    {
        SettingsManager.OnSettingsChanged -= ApplyCameraSettings;
        
        PauseManager.OnPause -= OnPause;
        LevelCutscene.OnCinematicStarted -= OnLevelCutsceneOnOnCinematicStarted;
        LevelCutscene.OnCinematicEnded -= OnLevelCutsceneOnOnCinematicEnded;
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsChanged -= ApplyCameraSettings;
        
        PauseManager.OnPause -= OnPause;
        LevelCutscene.OnCinematicStarted -= OnLevelCutsceneOnOnCinematicStarted;
        LevelCutscene.OnCinematicEnded -= OnLevelCutsceneOnOnCinematicEnded;
    }
    
    private void ApplyCameraSettings()
    {
        _senseX = settingsData.sliderItems["cameraSensitivity"] * CamSenseX;
        _senseY = settingsData.sliderItems["cameraSensitivity"] * CamSenseY;
        if (isPaused)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = 0;
            freeLookCamera.m_YAxis.m_MaxSpeed = 0;
        }else
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = _senseX;
            freeLookCamera.m_YAxis.m_MaxSpeed = _senseY;
        }
        freeLookCamera.m_XAxis.m_InvertInput = settingsData.toggleItems["invertXAxis"];
        freeLookCamera.m_YAxis.m_InvertInput = settingsData.toggleItems["invertYAxis"];
        volumePFX.profile.TryGet(out MotionBlur motionBlur);
        motionBlur.active = settingsData.toggleItems["motionBlur"];
    }
    
    private void OnLevelCutsceneOnOnCinematicStarted() => freeLookCamera.enabled = false;
    private void OnLevelCutsceneOnOnCinematicEnded()
    {
        freeLookCamera.enabled = true;
    }
    
    private void OnPause(bool isPaused)
    {
        this.isPaused = isPaused;
        if (isPaused)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = 0;
            freeLookCamera.m_YAxis.m_MaxSpeed = 0;
        }
        else
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = _senseX;
            freeLookCamera.m_YAxis.m_MaxSpeed = _senseY;
        }
    }
}