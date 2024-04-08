using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ExamplePlatformer;
using SolarBuff.Circuit;
using SolarBuff.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CircuitMovingPlataform : CircuitComponent
{
      public CircuitPlug input;
      private bool automaticallyMove;
      [FormerlySerializedAs("claw")] [SerializeField] private Transform plataform;
      [SerializeField] private float speed;
      [SerializeField] private Vector3 deslocationAmount;
      [SerializeField]
      private int radiusToGetPlayer = 4;
      private Vector3 deslocationInitial;
      private Vector3 deslocationFinal;
      [SerializeField]
      private LayerMask layerMask;
      private List<PlayerControllerCore> playersOnTop;
      protected override void OnEnable()
      {
          base.OnEnable();
          playersOnTop = new List<PlayerControllerCore>();
          deslocationInitial = transform.position;
          deslocationFinal = deslocationInitial + deslocationAmount;
          plataform.position = input.ReadValue<bool>() ? deslocationFinal : deslocationInitial;
      }
    
      protected override void OnRefresh() 
      {
          var inputBool = input.ReadValue<bool>();
          deslocationInitial = transform.position;
          deslocationFinal = deslocationInitial + deslocationAmount;
          GetPlayers();
          var duration = Vector3.Distance(deslocationFinal, deslocationInitial) / speed;
          plataform.DOMove(inputBool? deslocationFinal : deslocationInitial, duration).OnComplete(OnFinish);
      }
      private void OnFinish()
      {
          ReleaseObject();
      }

      private void ReleaseObject()
      {
          
          foreach (var p in playersOnTop)
          {
              p.plataform = null;
              p.transform.SetParent(null);
          }
          playersOnTop.Clear();
      }

      private void GetPlayers()
      {
          Collider[] colliders = new Collider[8];
//          playersOnTop.Clear();
          var size = Physics.OverlapSphereNonAlloc(plataform.position, radiusToGetPlayer, colliders,layerMask);
          for (int i = 0; i < size; i++)
          {
              if (colliders[i].transform.TryGetComponent(out PlayerControllerCore coll))
              {
                  coll.transform.SetParent(plataform);
                  coll.plataform = plataform;
                  playersOnTop.Add(coll);
              }
          }
      }
      
}
