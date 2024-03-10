using System;
using NetBuff.Components;
using NetBuff.Misc;
using SolarBuff.Data;
using TMPro;
using UnityEngine;

namespace SolarBuff
{
    public class SaveTest : NetworkBehaviour
    {
        public IntNetworkValue counter;
        public TMP_Text label;
        
        public void OnEnable()
        {
            var profile = SaveManager.GetCurrentProfile();
            counter = new IntNetworkValue(profile.Body.Get("counter", 0), NetworkValue.ModifierType.Everybody);
            
            WithValues(counter);
            label.text = counter.Value.ToString();
            counter.OnValueChanged += (o, i) => label.text = i.ToString();
        }
        
        public void Increase()
        {
            counter.Value++;
#pragma warning disable CS4014
            var profile = SaveManager.GetCurrentProfile();
            profile.Body["counter"] = counter.Value;
            SaveManager.Save();
#pragma warning restore CS4014
        }
        
        public void Decrease()
        {
            counter.Value--;
#pragma warning disable CS4014
            var profile = SaveManager.GetCurrentProfile();
            profile.Body["counter"] = counter.Value;
            SaveManager.Save();
#pragma warning restore CS4014
        }
    }
}