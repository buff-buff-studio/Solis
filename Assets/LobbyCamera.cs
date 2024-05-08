using System;
using System.Collections;
using System.Collections.Generic;
using Solis.Misc;
using UnityEngine;

public class LobbyCamera : MonoBehaviour
{
    private GameObject _cam;
    private void OnEnable()
    {
        _cam = Camera.main!.gameObject;
        _cam.SetActive(false);
    }

    private void OnDisable()
    {
        _cam.SetActive(true);
    }
}
