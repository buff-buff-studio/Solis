using System;
using System.IO;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using UnityEngine;

namespace SolarBuff
{
    public class LatencyTest : NetworkBehaviour
    {
        public Vector3 pointA = new(5, 0, 5);
        public Vector3 pointB = new(-5, 0, 5);
        
        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
            if (!HasAuthority)
                  return;
            transform.position = ((TestPacket)packet).Position;
        }
        
        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            if (packet is TestPacket testPacket)
            {
                if(clientId == OwnerId)
                    ServerBroadcastPacketExceptFor(testPacket, clientId);
            }
        }
        
        public void FixedUpdate()
        {
            if (!HasAuthority)
                return;
            
            transform.position = Vector3.Lerp(pointA, pointB, Mathf.PingPong(Time.time, 1));
            SendPacket(new TestPacket
            {
                Position = transform.position,
                Id = Id
            });
        }
    }

    public class TestPacket : IOwnedPacket
    {
        public Vector3 Position { get; set; }
        public NetworkId Id { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);
            Id.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Id = NetworkId.Read(reader);
        }
    }
}