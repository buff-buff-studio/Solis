using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Data;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify clients that a player has performed an emote.
    /// </summary>
    public class PlayerEmotePacket : IOwnedPacket
    {
        public string EmoteName { get; set; }
        public NetworkId Id { get; set; }
        public CharacterType CharacterType { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(EmoteName);
            Id.Serialize(writer);
            writer.Write((byte) CharacterType);
        }

        public void Deserialize(BinaryReader reader)
        {
            EmoteName = reader.ReadString();
            Id = NetworkId.Read(reader);
            CharacterType = (CharacterType) reader.ReadByte();
        }
    }
}