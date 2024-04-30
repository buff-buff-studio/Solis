using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify clients that an object has been magnetized or demagnetized.
    /// </summary>
    public class PacketMagnetizedStateChange : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public NetworkId Object { get; set; }
        public bool Magnetized { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            Object.Serialize(writer);
            writer.Write(Magnetized);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            Object = NetworkId.Read(reader);
            Magnetized = reader.ReadBoolean();
        }
    }
}