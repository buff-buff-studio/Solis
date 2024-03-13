using System;
using System.Collections.Generic;
using System.Linq;
using SolarBuff.Props;
using Unity.VisualScripting;
using UnityEngine;

namespace SolarBuff.Circuit
{
    [RequireComponent(typeof(CableRenderer))]
    [ExecuteInEditMode]
    public class CircuitConnection : MonoBehaviour
    {
        [Serializable]
        public class ControlPoint
        {
            public Vector3 position;
            public Vector3 leftHandle = new Vector3(-1f, 0f, 0f);
            public Vector3 rightHandle = new Vector3(1f, 0f, 0f);
        }

        private CableRenderer _renderer;
    
        public CircuitPlug a;
        public CircuitPlug b;
        public List<ControlPoint> controlPoints = new();
        
        private void OnEnable()
        {
            _renderer = GetComponent<CableRenderer>();
            UpdateVisual(true);
        }
        
        private void Update()
        {
            if(a.Owner != null && a.Owner.transform.hasChanged)
            {
                UpdateVisual(true);
                return;
            }
            
            if(b.Owner != null && b.Owner.transform.hasChanged)
            {
                UpdateVisual(true);
                return;
            }
        }

        #region Path
        public ControlPoint[] GetControlPoints()
        {
            var points = new ControlPoint[controlPoints.Count + 2];
            points[0] = new ControlPoint {position = a.transform.position};
            
            for (var i = 0; i < controlPoints.Count; i++)
                points[i + 1] = controlPoints[i];
            
            points[^1] = new ControlPoint {position = b.transform.position};
            
            //Set 0 right handle as dir from 0 to 1
            points[0].rightHandle = (points[1].position - points[0].position).normalized;
            //Set last left handle as dir from last to last - 1
            points[^1].leftHandle = (points[^2].position - points[^1].position).normalized;
            
            return points;
        }
        #endregion
        
        public void UpdateVisual(bool reloadPoints)
        {
            if (a != null && b != null)
            {
                a.Connection = this;
                b.Connection = this;
                
                if (reloadPoints)
                {
                    transform.position = (a.transform.position + b.transform.position) / 2;
                    var points = GetControlPoints();
                    var data = new BezierCurveData(points);
                    _renderer.SetPositions(data.GeneratePoints().ToArray());
                }

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
                UpdateVisual(false);
                a.Owner.Refresh();
            }
            else
            {
                UpdateVisual(false);
                b.Owner.Refresh();
            }
        }
    }

    public class BezierCurveData
    {
        public CircuitConnection.ControlPoint[] points;

        public BezierCurveData(CircuitConnection.ControlPoint[] points)
        {
            this.points = points;
        }
        
        public IEnumerable<Vector3> GeneratePoints()
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                var p0 = points[i];
                var p1 = points[i + 1];
                
                //Calculate resolution
                var resolution = GetResolutionFor(p0, p1);
                
                for (var j = 0; j < resolution; j++)
                {
                    yield return BezierCurve(p0, p1, j / (float) resolution);
                }
                
                if(i == points.Length - 2)
                    yield return p1.position;
            }
        }

        private static int GetResolutionFor(CircuitConnection.ControlPoint p0, CircuitConnection.ControlPoint p1)
        {
            //if both facing handlers are the same, we can use a lower resolution
            if (p0.rightHandle == p1.leftHandle)
                return 1;
            
            //One for each 0.25f
            return Mathf.CeilToInt(Vector3.Distance(p0.position, p1.position) / 0.1f);
        }
        
        private static Vector3 BezierCurve(CircuitConnection.ControlPoint p0, CircuitConnection.ControlPoint p1, float t)
        {
            return Mathf.Pow(1 - t, 3) * p0.position +
                   3 * Mathf.Pow(1 - t, 2) * t * (p0.position + p0.rightHandle) +
                   3 * (1 - t) * Mathf.Pow(t, 2) * (p1.position + p1.leftHandle) +
                   Mathf.Pow(t, 3) * p1.position;
        }
    }
}