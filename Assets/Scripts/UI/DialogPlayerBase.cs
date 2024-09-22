using _Scripts.UI;
using NetBuff.Components;

namespace UI
{
    public class DialogPlayerBase : NetworkBehaviour
    {
        public DialogData currentDialog;
        public static bool IsDialogPlaying => DialogPanel.Instance.index.Value != -1;
        public void PlayDialog()
        {
            DialogPanel.Instance.PlayDialog(this);
        }
    }
}
