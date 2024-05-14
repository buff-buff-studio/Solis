using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using Solis.Data;
using Solis.Player;

namespace Solis.Packets
{
    /// <summary>
    /// Packet sent when a player dies.
    /// </summary>
    public class PlayerDeathPacket : IOwnedPacket
    {
        public PlayerControllerBase.Death Type { get; set; }
        public NetworkId Id { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            Id.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Type = (PlayerControllerBase.Death) reader.ReadByte();
            Id = NetworkId.Read(reader);
        }
    }
}