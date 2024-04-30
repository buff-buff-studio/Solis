using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;

namespace Solis.Packets
{
    /// <summary>
    /// Represents the actions that the player can do in the lobby
    /// </summary>
    public enum LobbyPlayerAction
    {
        None,
        ChangeCharacter,
    }
    
    /// <summary>
    /// Used to send actions from the player in the lobby
    /// </summary>
    public class LobbyPlayerActionPacket : IOwnedPacket
    {
        public LobbyPlayerAction Action { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            writer.Write((byte)Action);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            Action = (LobbyPlayerAction)reader.ReadByte();
        }

        public NetworkId Id { get; set; }
    }
}