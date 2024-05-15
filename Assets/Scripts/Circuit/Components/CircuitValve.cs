using System.Collections;
using System.Collections.Generic;
using Solis.Circuit;
using UnityEngine;

public class CircuitValve : CircuitComponent
{
     public CircuitPlug input;
     [SerializeField]
     private GameObject objectToDisable;

     public override CircuitData ReadOutput(CircuitPlug plug)
     {
         return new CircuitData();
     }

     public override IEnumerable<CircuitPlug> GetPlugs()
     {
         yield return input;
     }

     protected override void OnRefresh()
     {
         objectToDisable.SetActive(input.ReadOutput().power > 0);
     }
}
