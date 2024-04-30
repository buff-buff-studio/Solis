using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;
using UnityEngine;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify clients the current body rotation and position of a player.
    /// </summary>
    public class PlayerBodyLerpPacket : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public float BodyRotation { get; set; }
        public Vector3 BodyPosition { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            writer.Write(BodyRotation);
            writer.Write(BodyPosition.x);
            writer.Write(BodyPosition.y);
            writer.Write(BodyPosition.z);
        }

        void IPacket.Deserialize(BinaryReader reader)
        {
            Deserialize(reader);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            BodyRotation = reader.ReadSingle();
            BodyPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}