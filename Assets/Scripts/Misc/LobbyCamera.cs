using System;
using System.Collections;
using System.Collections.Generic;
using Solis.Misc;
using UnityEngine;

namespace Solis.Misc
{
    public class LobbyCamera : MonoBehaviour
    {
        private void OnEnable()
        {
            var camera = GetComponent<Camera>();

            if (camera == null || Camera.main == null)
                return;
            
            camera.depth = Camera.main.depth + 1;
        }
    }
}