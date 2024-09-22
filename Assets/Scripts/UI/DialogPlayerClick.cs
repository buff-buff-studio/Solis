using NetBuff.Misc;
using Solis.Packets;
using UI;
using UnityEditor;
using UnityEngine;

public class DialogPlayerClick : DialogPlayerBase
{ 
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
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(DialogPlayerClick)),CanEditMultipleObjects]
    public class DialogPlayerEditor : Editor
    {
        private DialogPlayerClick targetClass;

        private void OnEnable()
        {
            targetClass = target as DialogPlayerClick;
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
