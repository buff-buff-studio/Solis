using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify the state of a cutscene.
    /// </summary>
    public class CutsceneStatePacket : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public int FinishedCount { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            writer.Write(FinishedCount);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            FinishedCount = reader.ReadInt32();
        }
    }
}