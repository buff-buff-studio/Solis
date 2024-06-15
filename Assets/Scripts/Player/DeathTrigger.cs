using NetBuff.Components;
using Solis.Packets;
using Solis.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Solis.Player
{
    [Icon("Assets/Art/Sprites/Editor/DeathTrigger_ico.png")]
    [RequireComponent(typeof(BoxCollider))]
    public class DeathTrigger : NetworkBehaviour
    {
        [SerializeField] private PlayerControllerBase.Death _type;

#if UNITY_EDITOR
        private BoxCollider _boxCollider;

        private void OnDrawGizmos()
        {
            Gizmos.color = _type == PlayerControllerBase.Death.Fall
                ? new Color(1, 0, 0, .25f)
                : new Color(1, .25f, 0, .25f);
            Gizmos.DrawCube(transform.position, transform.localScale);
            Gizmos.DrawWireCube(transform.position, transform.localScale);

            BoxColliderVolume();
        }

        private void BoxColliderVolume()
        {
            if (_boxCollider == null) _boxCollider = GetComponent<BoxCollider>();

            if (_boxCollider.center != Vector3.zero)
                transform.position += Vector3.Scale(_boxCollider.center, transform.localScale);
            if (_boxCollider.size != Vector3.one)
                transform.localScale = Vector3.Scale(transform.localScale, _boxCollider.size);

            _boxCollider.center = Vector3.zero;
            _boxCollider.size = Vector3.one;
        }

        private void OnValidate()
        {
            TryGetComponent(out _boxCollider);
            _boxCollider.isTrigger = true;
            gameObject.tag = "DeathTrigger";
        }

#endif

        private void OnTriggerEnter(Collider col)
        {
            if (!HasAuthority)
                return;
            
            if (col.CompareTag("Player") && col.TryGetComponent(out PlayerControllerBase p))
            {
                SendPacket(new PlayerDeathPacket()
                {
                    Type = _type,
                    Id = p.Id
                });
            }
        }
    }
}