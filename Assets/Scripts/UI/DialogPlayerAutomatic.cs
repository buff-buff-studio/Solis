using Solis.Data;
using Solis.Player;
using UnityEngine;

namespace UI
{
    public class DialogPlayerAutomatic : DialogPlayerBase
    {
        public CharacterTypeFilter characterTypeFilter = CharacterTypeFilter.Both;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!TryGetComponent(out PlayerControllerBase p)) return;

            if(characterTypeFilter.Filter(p.CharacterType))
                PlayDialog();
        }
    }
}
