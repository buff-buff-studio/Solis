using System.IO;
using NetBuff.Interface;

namespace Solis.Packets
{
    public class PlayCutscenePacket : IPacket
    {
        public int CutsceneIndex { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(CutsceneIndex);
        }

        public void Deserialize(BinaryReader reader)
        {
            CutsceneIndex = reader.ReadInt32();
        }
    }
}