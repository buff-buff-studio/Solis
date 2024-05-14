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
        public override CharacterType CharacterType => CharacterType.Human;
    }
}