using System;
using SolarBuff.Props;
using UnityEngine;

namespace SolarBuff.Circuit
{
    [RequireComponent(typeof(CableRenderer))]
    [ExecuteInEditMode]
    public class CircuitConnection : MonoBehaviour
    {
        private CableRenderer _renderer;
    
        public CircuitPlug a;
        public CircuitPlug b;
        
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
        public Vector3[] GetControlPoints()
        {
            return new[] {a.transform.position, b.transform.position};
        }
        #endregion
        
        public void UpdateVisual()
        {
            if (a != null && b != null)
            {
                a.Connection = this;
                b.Connection = this;
                transform.position = (a.transform.position + b.transform.position) / 2;
                _renderer.SetPositions(GetControlPoints());
                
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

    }
}