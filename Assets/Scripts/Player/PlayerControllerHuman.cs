using Solis.Data;
using UnityEngine;

namespace Solis.Player
{
    /// <summary>
    /// Player Controller for the Human Character
    /// </summary>
    [Icon("Assets/Art/Sprites/Editor/PlayerControllerHuman_ico.png")]
    public class PlayerControllerHuman : PlayerControllerBase
    {
        private float _specialCooldown;
        private float _specialReadyTime;
        
        public override CharacterType CharacterType => CharacterType.Human;

        protected override void _Timer()
        {
            base._Timer();
            if (_specialCooldown > 0)
            {
                _specialCooldown -= Time.deltaTime;
                if (_specialCooldown <= 0)
                {
                    _specialCooldown = 0;
                    _specialReadyTime = Time.time;
                }
            }
        }

        protected override void _Special()
        {
            if (SolisInput.GetKeyDown("Jump"))
            {
                Debug.Log("a");
            }
        }
    }
}