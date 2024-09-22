using UnityEngine;

namespace UI
{
    public class DialogPlayerAutomatic : DialogPlayerBase
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                PlayDialog();
        }
    }
}
