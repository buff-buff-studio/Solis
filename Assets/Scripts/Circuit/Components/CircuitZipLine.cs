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
        [SerializeField] private Transform claw;
        [SerializeField]
        private int radiusToGetObject = 4;
        private Transform objectHolding;
        [SerializeField]
        private LayerMask layerMask;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            claw.position = input.ReadValue<bool>() ? start.position : end.position;
        }

        protected override void OnRefresh()
        {
            // when is active
            Debug.Log("MOVEE");
            if (objectHolding == null)
                GetNearObject();
            claw.DOMove(input.ReadValue<bool>() ? end.position : start.position, 2f).OnComplete(OnFinish);
        }

        private void OnFinish()
        {
            ReleaseObject();
        }

        private void ReleaseObject()
        {
            if (objectHolding != null)
                objectHolding.transform.SetParent(null);
        }

        private void GetNearObject()
        {
            Collider[] colliders = new Collider[8];
            var size = Physics.OverlapSphereNonAlloc(claw.position, radiusToGetObject, colliders,layerMask);
            for (int i = 0; i < size; i++)
            {
                objectHolding = colliders[i].transform;
                objectHolding.transform.SetParent(claw);
            }
        }
    }
}