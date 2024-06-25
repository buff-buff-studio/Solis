using System.Collections.Generic;
using NetBuff.Misc;
using Solis.Data;
using Solis.Packets;
using Solis.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Solis.Circuit.Components
{
    /// <summary>
    /// A scanner that can be interacted to scan the player type.
    /// </summary>
    public class CircuitPalmScanner : CircuitComponent
    {
        #region Inspector Fields
        [Header("SETTINGS")]
        public float radius = 3f;
        public CharacterTypeFilter playerTypeFilter = CharacterTypeFilter.Both;
        public bool canBeTurnedOff;
        public Color colorOff = Color.white;
        public Color colorOn = Color.green;
        
        [Header("REFERENCES")]
        public BoolNetworkValue isOn = new(false);
        public CircuitPlug output;
        public Image palm;
        public ParticleSystem fx;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            WithValues(isOn);
            
            isOn.OnValueChanged += _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(_OnPlayerInteract);
            
            palm.color = isOn.Value ? colorOn : colorOff;
        }
        
        protected void Update()
        {
            palm.color = Color.Lerp(palm.color, isOn.Value ? colorOn : colorOff, Time.deltaTime * 10);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            isOn.OnValueChanged -= _OnValueChanged;
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(_OnPlayerInteract);
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
                if(canBeTurnedOff)
                    isOn.Value = !isOn.Value;
                else
                    isOn.Value = true;
                onToggleComponent?.Invoke();
                return true;
            }
            
            return false;
        }
        
        private void _OnValueChanged(bool old, bool @new)
        {
            if(@new) fx.Play();
            Refresh();
        }
        #endregion
    }
    
}