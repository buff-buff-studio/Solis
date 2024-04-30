using System.IO;
using NetBuff.Interface;
using NetBuff.Misc;

namespace Solis.Packets
{
    /// <summary>
    /// Used to notify the server that a player has interacted with an object.
    /// </summary>
    public class PlayerInteractPacket : IPacket
    {
        public NetworkId Id { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
        }
    }
}