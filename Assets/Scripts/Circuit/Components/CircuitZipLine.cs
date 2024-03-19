using ExamplePlatformer;
using NetBuff.Misc;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitZipLine : CircuitComponent
    {
        public CircuitPlug input;
        [SerializeField]private Transform start;
        [SerializeField]private Transform end;
        public BoolNetworkValue isOnStart = new(true);
        [SerializeField] private Transform claw;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            WithValues(isOnStart);
            claw.position = isOnStart.Value ? start.position : end.position;
        }

        protected override void OnRefresh()
        {
            
            // when is active
            Debug.Log("MOVEE");
            isOnStart.Value = !isOnStart.Value;
        }
    }
}