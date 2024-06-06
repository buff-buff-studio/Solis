using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkyboxSettings : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0,5)]
    private float skyboxRotationSpeed = 0.1f;
    [SerializeField]
    private float rotation;
    [SerializeField]
    private uint skyboxIndex = 0;

    [Space]
    [Header("Materials")]
    [SerializeField]
    private Material mainSkyboxMaterial;
    [SerializeField]
    private Material[] skyboxMaterial;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        skyboxIndex = (uint)Random.Range(0, skyboxMaterial.Length);
        UpdateSkybox();
    }

    private void FixedUpdate()
    {
        rotation += skyboxRotationSpeed * Time.fixedDeltaTime;
        if (rotation >= 360) rotation = 0;
        mainSkyboxMaterial.SetFloat("_Rotation", rotation);
    }

    private void UpdateSkybox()
    {
        var m = skyboxMaterial[skyboxIndex];
        mainSkyboxMaterial.SetColor("_Tint", m.GetColor("_Tint"));
        mainSkyboxMaterial.SetFloat("_Exposure", m.GetFloat("_Exposure"));
        mainSkyboxMaterial.SetTexture("_Tex", m.GetTexture("_Tex"));
        RenderSettings.skybox = mainSkyboxMaterial;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (skyboxIndex >= skyboxMaterial.Length)
            skyboxIndex = (uint)skyboxMaterial.Length - 1;
        UpdateSkybox();
    }
#endif
}
