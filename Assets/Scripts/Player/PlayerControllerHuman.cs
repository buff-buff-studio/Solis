using Solis.Data;
using Solis.Interface.Input;
using UnityEngine;

namespace Solis.Player
{
    /// <summary>
    /// Player Controller for the Human Character
    /// </summary>
    [Icon("Assets/Art/Sprites/Editor/PlayerControllerHuman_ico.png")]
    public class PlayerControllerHuman : PlayerControllerBase
    {
        public override CharacterType CharacterType => CharacterType.Human;

        [Header("SPECIAL")]
        private float _specialCooldown;
        private float _specialTimer;
        public GameObject cloudPrefab;
        public Vector3 cloudOffset;

        protected override void _Timer()
        {
            base._Timer();
            if (_specialTimer > 0)
            {
                _specialTimer -= Time.deltaTime;
                if (_specialTimer <= 0) _specialTimer = 0;
            }
        }

        protected override void _Special()
        {
            if (_specialTimer > 0) return;
            if (SolisInput.GetKeyDown("Jump") && (CanJumpCut || _isFalling))
            {
                _specialTimer = _specialCooldown;
                Spawn(cloudPrefab, transform.position + cloudOffset, body.rotation);
            }
        }
    }

    //I only wanted to be part of something

}