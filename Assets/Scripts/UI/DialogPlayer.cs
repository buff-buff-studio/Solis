using System.Collections.Generic;
using _Scripts.UI;
using UnityEngine;

namespace UI
{
    public class DialogPlayer : MonoBehaviour
    {
        public List<DialogData> currentDialog;


        public void PlayDialog()
        {
            DialogPanel.Instance.PlayDialog(currentDialog);
        }
    }
}
