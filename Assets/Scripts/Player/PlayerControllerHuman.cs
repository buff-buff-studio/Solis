using Solis.Data;

namespace Solis.Player
{
    /// <summary>
    /// Player Controller for the Human Character
    /// </summary>
    public class PlayerControllerHuman : PlayerControllerBase
    {
        public override CharacterType CharacterType => CharacterType.Human;
    }
}