using DG.Tweening;
using ExamplePlatformer;
using NetBuff.Misc;
using SolarBuff.Circuit.Components.Testing;
using SolarBuff.Player;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitZipLine : CircuitComponent
    {
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
            claw.position = GetPlugValue(CircuitPlug.Type.Input) ?end.position: start.position;
        }

        protected override void OnRefresh()
        {
            var inputBool = GetPlugValue(CircuitPlug.Type.Input);
            if (objectHolding == null && inputBool)
                GetNearObject();
            claw.DOMove(inputBool? end.position : start.position, 2f).OnComplete(OnFinish);
        }

        private void OnFinish()
        {
            ReleaseObject();
        }

        private void ReleaseObject()
        {
            if (objectHolding != null)
            {
                objectHolding.transform.SetParent(null);
            }

            objectHolding = null;
        }

        private void GetNearObject()
        {
            Collider[] colliders = new Collider[8];
            var size = Physics.OverlapSphereNonAlloc(claw.position, radiusToGetObject, colliders,layerMask);
            for (int i = 0; i < size; i++)
            {
                if (colliders[i].transform.TryGetComponent(out MagnetObject coll))
                {
                    if (colliders[i].CompareTag("Player"))
                    {
                        var player = coll.transform.GetComponent<PlayerControllerCore>();
                        if(player.type == PlayerControllerCore.PlayerType.Robot)
                            objectHolding = colliders[i].transform;
                    }
                    else if (objectHolding == null)
                        objectHolding = colliders[i].transform;
                    
                    
                    objectHolding.transform.SetParent(claw);
                    objectHolding.transform.localPosition = Vector3.zero;
                }
            }
        }
    }
}