
using _Scripts.UI;
using NetBuff.Components;
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
