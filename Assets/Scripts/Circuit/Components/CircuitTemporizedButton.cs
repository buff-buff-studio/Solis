<<<<<<< HEAD
﻿using System.Threading.Tasks;
using ExamplePlatformer;
using NetBuff.Misc;
using SolarBuff.Player;
using UnityEngine;

namespace SolarBuff.Circuit.Components
{
    public class CircuitTemporizedButton : CircuitComponent
    {
        public BoolNetworkValue isOn = new(false);
        public float radius = 2f;
        public int timeOn = 4;
        public Transform handle;
        private float buttonScale = 0.01f;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            WithValues(isOn);
            handle.localScale = new Vector3(handle.localScale.x, isOn.Value ? buttonScale : 0.3f,handle.localScale.z);
            isOn.OnValueChanged += (_, _) => Refresh();
            
            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive += OnPlayerPunch;
        }
        
        private async void OnPlayerPunch(PlayerPunchActionPacket obj, int client)
        {
            var o = GetNetworkObject(obj.Id);
            var dist = Vector3.Distance(o.transform.position, transform.position);

            if (dist > radius)
                return;
            
            if(isOn.Value) return;

            isOn.Value = true;
            await TurnOffAsync();
        }

        private async Task TurnOffAsync()
        {
            await SomeAsyncOperation();
            async Task SomeAsyncOperation()
            {
                await Task.Delay(timeOn * 1000); 
            }
            
            isOn.Value = false;
        }
        
        private void Update()
        {
            handle.localScale = new Vector3(handle.localScale.x, Mathf.Lerp(handle.localScale.y,isOn.Value ? buttonScale : 0.3f, Time.deltaTime * 10),handle.localScale.z); 
        }
        
        public override T ReadOutput<T>(CircuitPlug plug)
        {
            return SafeOutput<T>(isOn.Value);
        }
=======
﻿using System.Collections.Generic;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A button that can be pressed by a player and will stay on for a certain amount of time.
    /// </summary>
    public class CircuitTemporizedButton : CircuitComponent
    {
        #region Inspector Fields
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;
        public float timeOn = 4;
        
        public Vector3 knobOn = new(0, 0.15f, 0f);
        public Vector3 knobOff = new(0, 0.25f, 0f);
        
        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);
        public CircuitPlug output;
        public Transform knob;
        #endregion

        #region Private Fields
        [SerializeField, HideInInspector]
        private float timeOnCounter;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            WithValues(isOn);
            
            base.OnEnable();
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(_OnPlayerInteract);
            
            isOn.OnValueChanged += _OnValueChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(_OnPlayerInteract);
        }
        
        private void FixedUpdate()
        {
            if (!HasAuthority)
                return;

            if (!isOn.Value) 
                return;
            
            if (timeOnCounter > 0)
            {
                timeOnCounter -= Time.fixedDeltaTime;
            }
            else
            {
                isOn.Value = false;
            }
        }

        private void Update()
        {
            knob.localPosition = Vector3.Lerp(knob.localPosition, isOn.Value ? knobOn : knobOff, Time.deltaTime * 10);
        }
        #endregion

        #region Abstract Methods Implementation
        public override CircuitData ReadOutput(CircuitPlug plug)
        {
            return new CircuitData(isOn.Value);
        }

        protected override void OnRefresh()
        {
            
        }

        public override IEnumerable<CircuitPlug> GetPlugs()
        {
            yield return output;
        }
        #endregion
        
        #region Private Methods
        private bool _OnPlayerInteract(PlayerInteractPacket arg1, int arg2)
        {
            var player = GetNetworkObject(arg1.Id);
            var dist = Vector3.Distance(player.transform.position, transform.position);
            
            if (dist > radius)
                return false;
            
            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;
            
            if (playerTypeFilter.Filter(controller.CharacterType))
            {
                isOn.Value = true;
                timeOnCounter = timeOn;
                
                return true;
            }
            
            return false;
        }
        
        private void _OnValueChanged(bool old, bool @new)
        {
            Refresh();
        }
        #endregion
>>>>>>> renaissance
    }
}