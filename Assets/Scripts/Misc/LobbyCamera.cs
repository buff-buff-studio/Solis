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
            GetComponent<Camera>().depth = Camera.main!.depth + 1;
        }
    }
}