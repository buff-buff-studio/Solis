using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Packets
{
    public class SnapSyncPacket : IOwnedPacket
    {
        public Vector3 Position {get; set; }
        public Vector3 Rotation {get; set; }

        [InspectorMode(InspectorMode.Object)]
        public NetworkId Id { get; set; }

        public SnapSyncPacket(NetworkId networkId, Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
            Id = networkId;
        }

        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);

            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);

            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}