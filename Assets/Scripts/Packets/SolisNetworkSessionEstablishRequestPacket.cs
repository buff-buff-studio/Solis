using System.IO;
using NetBuff.Packets;

namespace Solis.Packets
{
    /// <summary>
    /// Used internally to establish a network session.
    /// </summary>
    public class SolisNetworkSessionEstablishRequestPacket : NetworkSessionEstablishRequestPacket
    {
        public string Username { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Username);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Username = reader.ReadString();
        }
    }
}