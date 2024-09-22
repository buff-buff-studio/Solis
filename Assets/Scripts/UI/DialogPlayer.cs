
using _Scripts.UI;
using NetBuff.Components;
using NetBuff.Misc;
using Solis.Packets;
using UnityEditor;
using UnityEngine;

namespace UI
{
    public class DialogPlayer : NetworkBehaviour
    {
        public DialogData currentDialog;
        private static bool InputDialog => Input.GetButtonDown("Interact");
        private static bool IsDialogPlaying => DialogPanel.Instance.index.Value != -1;
        public float radius = 2;
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
            if (dist > radius)
                return false;

            if (IsDialogPlaying) return false;
            
            PlayDialog();
            return true;
        }

        private void PlayDialog()
        {
            DialogPanel.Instance.PlayDialog(this);
        }
    }
    
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(DialogPlayer))]
    public class DialogPlayerEditor : Editor
    {
        private DialogPlayer targetClass;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnEnable()
        {
            targetClass = target as DialogPlayer;
        }

        void OnSceneGUI()
        {
            var transform = targetClass.transform;
            targetClass.radius = Handles.RadiusHandle(
                transform.rotation, 
                transform.position, 
                targetClass.radius);
        }
    }
#endif
}
