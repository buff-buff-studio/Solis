using System;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    [Serializable]
    public class CircuitSocket : CircuitComponent
    {
        public CircuitPlug exit;
        public CircuitPlug socket;

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return exit == plug ? socket.ReadValue<T>() : exit.ReadValue<T>();
        }
    }
}