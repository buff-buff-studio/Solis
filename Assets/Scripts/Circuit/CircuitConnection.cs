using System;
using System.Collections.Generic;
using System.Linq;
using SolarBuff.Props;
using UnityEngine;

namespace SolarBuff.Circuit
{
    [RequireComponent(typeof(CableRenderer))]
    [ExecuteInEditMode]
    public class CircuitConnection : MonoBehaviour
    {
        [Serializable]
        public struct ControlPoint
        {
            public Vector3 position;
        }

        private CableRenderer _renderer;
    
        public CircuitPlug a;
        public CircuitPlug b;
        public List<ControlPoint> controlPoints = new();
        
        private void OnEnable()
        {
            _renderer = GetComponent<CableRenderer>();
            UpdateVisual();
        }

        private void Update()
        {
            if(a.Owner != null && a.Owner.transform.hasChanged)
            {
                UpdateVisual();
                return;
            }
            
            if(b.Owner != null && b.Owner.transform.hasChanged)
            {
                UpdateVisual();
                return;
            }
        }

        #region Path
        public IEnumerable<Vector3> GetControlPoints()
        {
            yield return a.transform.position;
            foreach (var controlPoint in controlPoints)
                yield return controlPoint.position;
            yield return b.transform.position;
        }
        #endregion
        
        public void UpdateVisual()
        {
            if (a != null && b != null)
            {
                a.Connection = this;
                b.Connection = this;
                transform.position = (a.transform.position + b.transform.position) / 2;
                _renderer.SetPositions(GetControlPoints().ToArray());
                
                if(Application.isPlaying)
                    _renderer.material.color = Color.Lerp(Color.black, Color.red, a.ReadValue<float>());
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
        
        
        private void OnDisable()
        {
            if (a != null)
            {
                a.Connection = null;
                if(a.Owner != null)
                    a.Owner.Refresh();
            }
            
            if (b != null)
            {
                b.Connection = null;
                if(b.Owner != null)
                    b.Owner.Refresh();
            }
        }
        
        public void Refresh()
        {
            if (a.type == CircuitPlug.Type.Input)
            {
                UpdateVisual();
                a.Owner.Refresh();
            }
            else
            {
                UpdateVisual();
                b.Owner.Refresh();
            }
        }


        public Vector3 GetClosestPoint(Vector3 point)
        {
            var points = GetControlPoints();
            var current = points.First();
            var currentDistance = Vector3.Distance(current, point);

            for (int i = 1; i < points.Count(); i++)
            {
                var a = points.ElementAt(i - 1);
                var b = points.ElementAt(i);
                var closest = ClosestPointOnLineSegment(a, b, point);
                if (Vector3.Distance(closest, point) < currentDistance)
                {
                    current = closest;
                    currentDistance = Vector3.Distance(closest, point);
                }
            }

            return current;
        }

        private Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            var ap = p - a;
            var ab = b - a;
            var magnitude = ab.sqrMagnitude;
            var abap = Vector3.Dot(ap, ab);
            var t = abap / magnitude;
            if (t < 0)
                return a;
            if (t > 1)
                return b;
            return a + ab * t;
        }
    }
}