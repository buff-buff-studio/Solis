using System;
using System.Collections.Generic;
using _Scripts.UI;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Packets;
using Solis.Player;
using UnityEngine;

namespace UI
{
    public class DialogPlayer : NetworkBehaviour
    {
        public List<DialogData> currentDialog;
        private static bool InputDialog => Input.GetButtonDown("Interact");
        private static BoolNetworkValue IsDialogPlaying => DialogPanel.Instance.textWriterSingle.isWriting;
        [SerializeField]private float _radius = 2;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }

        protected void OnEnable()
        {
            PacketListener.GetPacketListener<PlayerInteractPacket>().AddServerListener(OnClickDialog);
        }
        protected void OnDisable()
        {
            PacketListener.GetPacketListener<PlayerInteractPacket>().RemoveServerListener(OnClickDialog);
        }

        private bool OnClickDialog(PlayerInteractPacket arg1, int arg2)
        {
            var player = GetNetworkObject(arg1.Id);
            var dist = Vector3.Distance(player.transform.position, transform.position);

            if (dist > _radius)
                return false;

            var controller = player.GetComponent<PlayerControllerBase>();
            if (controller == null)
                return false;

            /*if (playerTypeFilter.Filter(controller.CharacterType))
            {
                isOn.Value = !isOn.Value;
                onToggleComponent?.Invoke();
                return true;
            }*/
            return true;

        }

        private void Update()
        {
            if (InputDialog && !IsDialogPlaying.Value)
                PlayDialog();
        }

        public void PlayDialog()
        {
            DialogPanel.Instance.PlayDialog(this);
        }
    }
}
