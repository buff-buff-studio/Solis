using System.IO;
using NetBuff.Session;

namespace Solis.Data
{
    /// <summary>
    /// Represents the session data for a Solis game client.
    /// </summary>
    public class SolisSessionData : SessionData
    {
        #region Public Properties
        /// <summary>
        /// The username of the player. Used to restores session when player reconnects.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// The character type of the player.
        /// </summary>
        public CharacterType PlayerCharacterType { get; set; } = CharacterType.Human;

        /// <summary>
        /// The equipped emotes of the player.
        /// </summary>
        public string[] Emotes { get; set; } = {"example", "", "" };
        #endregion

        #region Abstract Methods Implementation
        public override void Serialize(BinaryWriter writer, bool shouldSerializeEverything)
        {
            writer.Write(Username);
            writer.Write((byte)PlayerCharacterType);
            writer.Write(Emotes.Length);
            foreach (var emote in Emotes)
            {
                writer.Write(emote ?? "");
            }
        }

        public override void Deserialize(BinaryReader reader, bool shouldDeserializeEverything)
        {
            Username = reader.ReadString();
            PlayerCharacterType = (CharacterType)reader.ReadByte();
            var emotesLength = reader.ReadInt32();
            Emotes = new string[emotesLength];
            for (var i = 0; i < emotesLength; i++)
            {
                Emotes[i] = reader.ReadString();
            }
        }
        #endregion
    }
}