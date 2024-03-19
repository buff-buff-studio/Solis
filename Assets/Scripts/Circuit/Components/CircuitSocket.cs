using System;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    [Serializable]
    public class CircuitSocket : CircuitComponent
    {
        public CircuitPlug exit;
        public CircuitPlug socket;

        public MeshFilter socketFilter;
        public Mesh[] socketMeshes;

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return exit == plug ? socket.ReadValue<T>() : exit.ReadValue<T>();
        }

        public override bool IsHighVoltage(CircuitPlug plug)
        {
            return plug == exit ? socket.IsHighVoltage() : exit.IsHighVoltage();
        }

        protected override void OnRefresh()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            var hasConnection = socket.Connection != null || socket.GetComponentInChildren<CircuitPhysicalCable>() != null;
            socketFilter.mesh = socketMeshes[hasConnection ? 1 : 0];
            
            base.OnRefresh();
        }
    }
}