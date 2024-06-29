using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Solis.Circuit.Connections
{
    /// <summary>
    /// Standard cable connection between two plugs.
    /// Uses Bezier curves to draw the cable.
    /// </summary>
    [ExecuteInEditMode]
    public class CircuitStandardCableConnection : CircuitWirelessConnection
    {
        #region Types
        /// <summary>
        /// Represents a control point for the Bezier curve.
        /// </summary>
        [Serializable]
        public class ControlPoint
        {
            public Vector3 position;
            public Vector3 leftHandle;
            public Vector3 rightHandle;
        }

        /// <summary>
        /// Represents the data of a Bezier curve.
        /// </summary>
        private class BezierCurveData
        {
            #region Private Fields
            private readonly ControlPoint[] _points;
            #endregion

            #region Public Constructors
            public BezierCurveData(ControlPoint[] points)
            {
                _points = points;
            }
            #endregion

            #region Public Static Methods
            /// <summary>
            /// Generates a point on the Bezier curve.
            /// </summary>
            /// <param name="p0"></param>
            /// <param name="p1"></param>
            /// <param name="t"></param>
            /// <returns></returns>
            public static Vector3 BezierCurve(ControlPoint p0, ControlPoint p1, float t)
            {
                return Mathf.Pow(1 - t, 3) * p0.position +
                       3 * Mathf.Pow(1 - t, 2) * t * (p0.position + p0.rightHandle) +
                       3 * (1 - t) * Mathf.Pow(t, 2) * (p1.position + p1.leftHandle) +
                       Mathf.Pow(t, 3) * p1.position;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Returns the generated points of the Bezier curve.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<Vector3> GeneratePoints()
            {
                for (var i = 0; i < _points.Length - 1; i++)
                {
                    var p0 = _points[i];
                    var p1 = _points[i + 1];
                    
                    var resolution = _GetResolutionFor(p0, p1);

                    for (var j = 0; j < resolution; j++)
                    {
                        yield return BezierCurve(p0, p1, j / (float)resolution);
                    }

                    if (i == _points.Length - 2)
                        yield return p1.position;
                }
            }
            #endregion

            #region Private Methods
            private static int _GetResolutionFor(ControlPoint p0, ControlPoint p1)
            {
                return p0.rightHandle == p1.leftHandle
                    ? 1
                    : Mathf.CeilToInt(Vector3.Distance(p0.position, p1.position) / 0.1f);
            }
            #endregion
        }
        #endregion
        
        #region Inspector Fields
        [Header("VISUAL")]
        public Material material;
        public float width = 0.3f;

        [Header("REFERENCES")]
        public GameObject prefabShockVFX;
        
        [Header("STATE")]
        public List<ControlPoint> controlPoints = new();
        #endregion
        
        #if UNITY_EDITOR
        #region Public Fields
        [SerializeField, HideInInspector]
        public List<int> selectedControlPointIndices = new();
        #endregion
        #endif

        #region Private Fields
        [SerializeField, HideInInspector]
        private ControlPoint firstPoint = new();
        [SerializeField, HideInInspector]
        private ControlPoint lastPoint = new();
        [SerializeField, HideInInspector]
        private ParticleSystem shockVFX;
        private LineRenderer _renderer;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            if (_renderer == null)
                _renderer = gameObject.GetComponent<LineRenderer>();

            if (_renderer == null)
                _renderer = gameObject.AddComponent<LineRenderer>();

            base.OnEnable();

            if (Application.isPlaying)
                InvokeRepeating(nameof(_ShockEffects), 0f, 0.25f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Application.isPlaying)
                CancelInvoke(nameof(_ShockEffects));

            if (_renderer == null)
                return;

            _renderer.positionCount = 0;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshVisuals();
        }

        protected override void OnDrawGizmos()
        {
        }

        protected override void OnDrawGizmosSelected()
        {
        }

        private void Update()
        {
            if (!IsValid)
                return;

            if (PlugA.transform.hasChanged || PlugB.transform.hasChanged)
            {
                RefreshVisuals();
                transform.position = (PlugA.transform.position + PlugB.transform.position) / 2;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refreshes the visuals of the cable.
        /// </summary>
        public void RefreshVisuals()
        {
            if (_renderer == null || !enabled)
                return;

            _renderer.endColor = _renderer.startColor = color;
            _renderer.startWidth = _renderer.endWidth = width;
            _renderer.useWorldSpace = false;
            _renderer.material = material;

            if (!IsValid)
            {
                _renderer.positionCount = 0;
                return;
            }

            var points = GetControlPoints();
            var data = new BezierCurveData(points);

            var generatedPoints = data.GeneratePoints().ToArray();
            _renderer.positionCount = generatedPoints.Length;
            _renderer.SetPositions(generatedPoints);
        }
        
        /// <summary>
        /// Returns the control points of the cable.
        /// </summary>
        /// <returns></returns>
        public ControlPoint[] GetControlPoints()
        {
            firstPoint.position = transform.InverseTransformPoint(PlugA.transform.position);
            lastPoint.position = transform.InverseTransformPoint(PlugB.transform.position);

            var points = new ControlPoint[controlPoints.Count + 2];
            points[0] = firstPoint;
            points[^1] = lastPoint;

            for (var i = 0; i < controlPoints.Count; i++)
                points[i + 1] = controlPoints[i];

            if (firstPoint.rightHandle == Vector3.zero)
                firstPoint.rightHandle = (points[1].position - points[0].position).normalized;

            if (lastPoint.leftHandle == Vector3.zero)
                lastPoint.leftHandle = (points[^2].position - points[^1].position).normalized;

            return points;
        }
        #endregion

        #region Private Methods
        private void _ShockEffects()
        {
            if (!IsValid)
            {
                if (shockVFX != null)
                    shockVFX.Stop();
                return;
            }

            var power = PlugA.ReadOutput().power;

            if (power < 0.5f)
            {
                if (shockVFX != null)
                    shockVFX.Stop();
                return;
            }

            if (shockVFX == null)
                shockVFX = Instantiate(prefabShockVFX, transform).GetComponent<ParticleSystem>();

            
            var points = GetControlPoints();
            var ri = UnityEngine.Random.Range(0, points.Length - 1);
            var pos = BezierCurveData.BezierCurve(points[ri], points[ri + 1], UnityEngine.Random.Range(0f, 1f));

            shockVFX.transform.position = transform.TransformPoint(pos);
            shockVFX.Play();
        }
        #endregion
    }
}