using DG.Tweening;
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
        [SerializeField]
        private int radiusToGetObject = 4;

        private Transform objectHolding;
        private BoolNetworkValue hasObject;
        [SerializeField]
        private LayerMask layerMask;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            WithValues(isOnStart,hasObject);
            claw.position = isOnStart.Value ? start.position : end.position;
        }

        protected override void OnRefresh()
        {
            // when is active
            Debug.Log("MOVEE");
            if (!hasObject.Value)
                GetNearObject();
            claw.DOMove(isOnStart.Value ? end.position : start.position, 2f).OnComplete(OnFinish);
                
            isOnStart.Value = !isOnStart.Value;
        }

        private void OnFinish()
        {
            ReleaseObject();
        }

        private void ReleaseObject()
        {
            if (!hasObject.Value)
                objectHolding.transform.SetParent(null);
        }

        private void GetNearObject()
        {
            Collider[] colliders = new Collider[1];
            var size = Physics.OverlapSphereNonAlloc(claw.position, radiusToGetObject, colliders,layerMask);

            foreach (var c in colliders)
            {
                objectHolding = c.transform;
                objectHolding.transform.SetParent(claw);
                hasObject.Value = true;
            }
        }
    }
}