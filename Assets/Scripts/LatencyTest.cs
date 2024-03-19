using System;
using NetBuff.Components;
using UnityEngine;

namespace SolarBuff
{
    public class LatencyTest : NetworkBehaviour
    {
        public Vector3 pointA = new(5, 0, 5);
        public Vector3 pointB = new(-5, 0, 5);
        
        public void Update()
        {
            if (!HasAuthority)
                return;

            transform.position = Vector3.Lerp(pointA, pointB, Mathf.PingPong(Time.time, 1));
        }
    }
}