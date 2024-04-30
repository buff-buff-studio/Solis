using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using Unity.Mathematics;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify clients that a physical cable connection has been established or removed.
    /// </summary>
    public class PhysicalCableConnectionPacket : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public NetworkId Holder { get; set; }
        public half3[] Nodes { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);

            Holder.Serialize(writer);

            writer.Write(Nodes.Length);
            foreach (var node in Nodes)
            {
                writer.Write(node.x.value);
                writer.Write(node.y.value);
                writer.Write(node.z.value);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            Holder = NetworkId.Read(reader);

            var count = reader.ReadInt32();
            Nodes = new half3[count];
            for (var i = 0; i < count; i++)
            {
                Nodes[i] = new half3(new half { value = reader.ReadUInt16() }, new half { value = reader.ReadUInt16() },
                    new half { value = reader.ReadUInt16() });
            }
        }
    }
}