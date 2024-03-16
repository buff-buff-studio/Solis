using System.Collections.Generic;
using ExamplePlatformer;
using NetBuff.Misc;
using Unity.VisualScripting;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitXor : CircuitComponent
    {
        public CircuitPlug inputA;
        public CircuitPlug inputB;

        public CircuitPlug output;
        public CircuitPlug outputInverted;

        public override T ReadOutput<T>(CircuitPlug plug)
        {
            var b = inputA.ReadValue<bool>() ^ inputB.ReadValue<bool>();

            if(plug == output)
                return SafeOutput<T>(b);
            return SafeOutput<T>(!b);
        }
    }
}