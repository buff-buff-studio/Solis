using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;

namespace Solis.Packets
{
    public class LightObjectGrabPacket : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public string HandId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            writer.Write(HandId);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            HandId = reader.ReadString();
        }
    }
}