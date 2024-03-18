using System;
using System.Collections.Generic;
using System.Linq;
using SolarBuff.Props;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SolarBuff.Circuit
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class CircuitStaticCable : MonoBehaviour, ICircuitConnection
    {
        private static bool _isQuitting;
        
        [Serializable]
        public class ControlPoint
        {
            public Vector3 position;
            public Vector3 leftHandle = new Vector3(-1f, 0f, 0f);
            public Vector3 rightHandle = new Vector3(1f, 0f, 0f);
        }

        private LineRenderer _renderer;
        
        public List<ControlPoint> controlPoints = new();
        
        public CircuitPlug plugA;
        public CircuitPlug plugB;
        
        public CircuitPlug PlugA
        {
            get => plugA;
        }
        
        public CircuitPlug PlugB
        {
            get => plugB;
        }

       
        private void OnEnable()
        {
            _renderer = GetComponent<LineRenderer>();
     
#if UNITY_EDITOR
            _renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Cable.mat");
            _renderer.widthCurve = new AnimationCurve(new Keyframe(0, 0.25f), new Keyframe(1, 0.25f));
#endif

            
            if(_RefreshInternal())
                RefreshVisual(true);
        }
        
        protected virtual void OnDisable()
        {
            if(_isQuitting)
                return;
            
            if(plugA != null)
            {
                plugA.Connection = null;
                if(plugA.Owner != null)
                    plugA.Owner.Refresh();
            }
            
            if(plugB != null)
            {
                plugB.Connection = null;
                if(plugB.Owner != null)
                    plugB.Owner.Refresh();
            }
        }
        
        protected virtual void OnApplicationQuit () 
        {
            _isQuitting = true;
        }
        
        private void Update()
        {
            if(plugA.Owner != null && plugA.Owner.transform.hasChanged)
            {
                RefreshVisual(true);
                plugA.Owner.transform.hasChanged = false;
                return;
            }
            
            if(plugB.Owner != null && plugB.Owner.transform.hasChanged)
            {
                RefreshVisual(true);
                plugB.Owner.transform.hasChanged = false;
            }
        }

        #region Path
        public ControlPoint[] GetControlPoints()
        {
            var points = new ControlPoint[controlPoints.Count + 2];
            points[0] = new ControlPoint {position = plugA.transform.position};
            
            for (var i = 0; i < controlPoints.Count; i++)
                points[i + 1] = controlPoints[i];
            
            points[^1] = new ControlPoint {position = plugB.transform.position};
            
            //Set 0 right handle as dir from 0 to 1
            points[0].rightHandle = (points[1].position - points[0].position).normalized;
            //Set last left handle as dir from last to last - 1
            points[^1].leftHandle = (points[^2].position - points[^1].position).normalized;
            
            return points;
        }
        #endregion
        
        public void RefreshVisual(bool reloadPoints)
        {
            if (plugA != null && plugB != null)
            {
                if (reloadPoints)
                {
                    transform.position = (plugA.transform.position + plugB.transform.position) / 2;
                    var points = GetControlPoints();
                    var data = new BezierCurveData(points);
                    
                    _renderer.positionCount = data.GeneratePoints().Count();
                    _renderer.SetPositions(data.GeneratePoints().ToArray());
                }

                if (Application.isPlaying)
                    _renderer.material.color = Color.Lerp(Color.black, Color.red, plugA.ReadValue<float>());
            }
        }
        
        public bool Refresh()
        {
            if (_RefreshInternal())
            {
                if (plugA.type == CircuitPlug.Type.Input)
                {
                    plugA.Owner.Refresh();
                    RefreshVisual(false);
                }
                else
                {
                    plugB.Owner.Refresh();
                    RefreshVisual(false);
                }

                return true;
            }
            return false;
        }

        private bool _RefreshInternal()
        {
            if (plugA != null && plugB != null)
            {
                if (plugA.Connection != null && plugA.Connection != this)
                {
                    DestroyImmediate(gameObject);
                    return false;
                }

                if (plugB.Connection != null && plugB.Connection != this)
                {
                    DestroyImmediate(gameObject);
                    return false;
                }

                plugA.Connection = this;
                plugB.Connection = this;

                return true;
            }
            
            DestroyImmediate(gameObject);
            return false;
        }
    }

    public class BezierCurveData
    {
        public CircuitStaticCable.ControlPoint[] points;

        public BezierCurveData(CircuitStaticCable.ControlPoint[] points)
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

        private static int GetResolutionFor(CircuitStaticCable.ControlPoint p0, CircuitStaticCable.ControlPoint p1)
        {
            //if both facing handlers are the same, we can use a lower resolution
            if (p0.rightHandle == p1.leftHandle)
                return 1;
            
            //One for each 0.25f
            return Mathf.CeilToInt(Vector3.Distance(p0.position, p1.position) / 0.1f);
        }
        
        private static Vector3 BezierCurve(CircuitStaticCable.ControlPoint p0, CircuitStaticCable.ControlPoint p1, float t)
        {
            return Mathf.Pow(1 - t, 3) * p0.position +
                   3 * Mathf.Pow(1 - t, 2) * t * (p0.position + p0.rightHandle) +
                   3 * (1 - t) * Mathf.Pow(t, 2) * (p1.position + p1.leftHandle) +
                   Mathf.Pow(t, 3) * p1.position;
        }
    }
}