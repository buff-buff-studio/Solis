using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solis.VFX
{
    [ExecuteInEditMode]
    public class GrassEffector : MonoBehaviour
    {
        private static readonly List<GrassEffector> _GrassObjects = new();

        public float radius = 2f;
        private static readonly int _InteractionPositions = Shader.PropertyToID("_effectorData");

        private void OnEnable()
        {
            _GrassObjects.Add(this);
        }

        private void OnDisable()
        {
            _GrassObjects.Remove(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        private void Update()
        {
            if (_GrassObjects[0] != this)
                return;

            var array = Shader.GetGlobalFloatArray(_InteractionPositions);
            if (array == null)
                array = new float[41];

            array[0] = _GrassObjects.Count;
            for (var i = 0; i < _GrassObjects.Count; i++)
            {
                var go = _GrassObjects[i];
                var pos = go.transform.position;
                var idx = i * 4 + 1;
                array[idx] = pos.x;
                array[idx + 1] = pos.y;
                array[idx + 2] = pos.z;
                array[idx + 3] = go.radius;
            }

            Shader.SetGlobalFloatArray(_InteractionPositions, array);
        }
    }
}