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
        private static bool IsDialogPlaying => DialogPanel.Instance.index.Value != -1;
        [SerializeField]private float _radius = 2;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }

        private void Update()
        {
            if (InputDialog && !IsDialogPlaying)
                PlayDialog();
        }

        public void PlayDialog()
        {
            DialogPanel.Instance.PlayDialog(this);
        }
    }
}
