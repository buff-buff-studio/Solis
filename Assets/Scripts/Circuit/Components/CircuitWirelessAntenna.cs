using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// Used to transmit signals between two circuits without a direct connection, while still showing the link visually.
    /// </summary>
    [ExecuteInEditMode]
    public class CircuitWirelessAntenna : CircuitComponent
    {
        #region Private Static Fields
        private static readonly Color[] _Colors = {
            new(0.690f, 0.180f, 0.149f),
            new(0.227f, 0.702f, 0.855f),
            new(1f, 0.847f, 0.239f),
            new(0.502f, 0.780f, 0.121f),
            new(0.776f, 0.309f, 0.741f),
            new(0.537f, 0.196f, 0.717f),
            new(0.510f, 0.329f, 0.196f),
            new(0.976f, 1f, 1f),
            new(0.976f, 0.502f, 0.114f),
            new(0.086f, 0.612f, 0.616f),
            new(0.365f, 0.486f, 0.086f),
            new(0.114f, 0.110f, 0.129f),
            new(0.278f, 0.310f, 0.322f),
            new(.235f, 0.267f, 0.663f),
            new(0.953f, 0.545f, 0.667f),
            new(0.611f, 0.616f, 0.592f),
        };
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public CircuitPlug top;
        public CircuitPlug bottom;
        public Renderer colorRenderer;
        
        [Header("SETTINGS")]
        public int channel = -1;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            _UpdateChannel();
        }

        protected override void OnDisable()
        {
            if (!Application.isPlaying)
            {
                channel = -1;
                return;
            }

            base.OnDisable();
        }

        private void OnValidate()
        {
            _UpdateChannel();
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            if (plug == top)
                return bottom.ReadOutput();

            if (plug == bottom)
                return top.ReadOutput();

            return default;
        }

        protected override void OnRefresh()
        {
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return top;
            yield return bottom;
        }
        #endregion

        #region Private Methods
        private void _UpdateChannel()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                    return;
                
                if (channel == -1)
                {
                    //TRY TO GET A SIBBLING ANTENNA
                    var siblingAntenna = transform.parent.GetComponentsInChildren<CircuitWirelessAntenna>()
                        .FirstOrDefault(a => a != this);
                    
                    if (siblingAntenna != null && siblingAntenna.channel != -1)
                    {
                        channel = siblingAntenna.channel;
                        return;
                    }

                    var allAntennas = FindObjectsByType<CircuitWirelessAntenna>(FindObjectsSortMode.None);
                    var unusedChannels = Enumerable.Range(0, _Colors.Length).Except(allAntennas.Select(a => a.channel))
                        .ToArray();
                    
                    if (unusedChannels.Length > 0)
                        channel = unusedChannels[Random.Range(0, unusedChannels.Length)];
                    else
                    {
                        Debug.LogError("No more channels available for WirelessAntenna");
                        enabled = false;
                        return;
                    }
                }

                return;
            }
            #endif
            
            if (colorRenderer != null)
                colorRenderer.material.color = _Colors[channel % _Colors.Length];
        }
        #endregion
    }
}