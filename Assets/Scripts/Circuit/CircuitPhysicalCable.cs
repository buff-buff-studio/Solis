using System;
using System.Collections.Generic;
using ExamplePlatformer;
using NetBuff.Components;
using UnityEngine;

namespace SolarBuff.Circuit
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircuitPhysicalCable : NetworkBehaviour
    {
        [Serializable]
        public struct Node
        {
            public GameObject gameObject;
            public Rigidbody rigidbody;
            public Joint joint;
        }

        [Header("SETTINGS")] 
        public int minNodeCount = 3;
        public int maxNodeCount = 15;
        public float nodeDistance = 0.25f;

        [Header("REFERENCES")]
        public GameObject nodePrefab;
      
        [Header("STATE")]
        public List<Node> nodes = new();
        public Rigidbody helder;
        
        public Node Head => nodes[^1];
        public Node Tail => nodes[0];

        public Rigidbody Helder
        {
            get => helder;
            set
            {
                helder = value;
                if (helder != null)
                {
                    helderJoint = helder.gameObject.AddComponent<HingeJoint>();
                    helderJoint.anchor = Vector3.zero;
                    helderJoint.connectedBody = Head.rigidbody;
                }
                else
                {
                    if(helderJoint != null)
                        Destroy(helderJoint);
                    helderJoint = null;
                }
            }
        }

        [HideInInspector, SerializeField]
        private HingeJoint helderJoint;
        
        private LineRenderer _renderer;
        
        public void OnEnable()
        {
            GetPacketListener<PlayerPunchActionPacket>().OnServerReceive += OnPlayerPunch;
            InvokeRepeating(nameof(TickCable), 0, 0.05f);
            _renderer = GetComponent<LineRenderer>();
        }

        private void OnPlayerPunch(PlayerPunchActionPacket packet, int client)
        {
            var puncher = GetNetworkObject(packet.Id).gameObject.GetComponentInChildren<Rigidbody>();

            Helder = puncher == Helder ? null : puncher;
        }

        public void TickCable()
        {
            if (nodes.Count == 0)
            {
                var pos = transform.position;
                for (var i = 0; i < minNodeCount; i++)
                {
                    _CreateNode(pos, i);
                    pos += Vector3.forward * nodeDistance;
                }
                
                Helder = helder;
            }

            if (nodes.Count > minNodeCount || Helder != null)
            {
                if(helderJoint != null)
                {
                    var target = helderJoint.transform.position;
                    
                    for (var i = 1; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        var prevNode = nodes[i - 1];

                        var distance = Vector3.Distance(node.gameObject.transform.position,
                            prevNode.gameObject.transform.position);
                        
                        if (distance >= nodeDistance * 2)
                        {
                            if (nodes.Count < maxNodeCount)
                            {
                                var pos = Head.gameObject.transform.position;
                                var p = pos + (target - pos).normalized * nodeDistance;
                                _CreateNode(p, nodes.Count);
                            }
                            
                            break;
                        }
                    }
                    
                    while(nodes.Count < maxNodeCount && Vector3.Distance(Head.gameObject.transform.position, target) >= nodeDistance)
                    {
                        var pos = Head.gameObject.transform.position;
                        var p = pos + (target - pos).normalized * nodeDistance;
                        _CreateNode(p, nodes.Count);
                    }
                }
                
                //Delete nodes if not needed
                for (var i = 1; i < nodes.Count - 1; i++)
                {
                    var next = nodes[i + 1];
                    var prev = nodes[i - 1];

                    var distance = Vector3.Distance(next.gameObject.transform.position,
                        prev.gameObject.transform.position);
                        
                    if (distance < nodeDistance * 2)
                    {
                        next.joint.connectedBody = prev.rigidbody;
                        Destroy(nodes[i].gameObject);
                        nodes.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            _renderer.positionCount = nodes.Count;
            _renderer.SetPositions(nodes.ConvertAll(n => n.gameObject.transform.position).ToArray());
        }

        private void _CreateNode(Vector3 position, int index)
        {
            var go = Instantiate(nodePrefab, position, Quaternion.identity, transform);
            var node = new Node
            {
                gameObject = go,
                rigidbody = go.GetComponent<Rigidbody>(),
                joint = go.GetComponent<SpringJoint>()
            };

            var hasPrev = index < nodes.Count;
            var prevOnPos = hasPrev ? nodes[index] : default;
            
            nodes.Insert(index, node);
            
            if (nodes.Count > 1)
            {
                var prevNode = nodes[index - 1];
                var cj = (node.joint as SpringJoint)!;
                cj.anchor = Vector3.zero;

                if (hasPrev)
                {
                    prevOnPos.joint.connectedBody = node.rigidbody;
                    //prevOnPos.rigidbody.isKinematic = false;
                }
                
                cj.autoConfigureConnectedAnchor = false;
                cj.connectedAnchor = new Vector3(0, 0, 0);
                cj.connectedBody = prevNode.rigidbody;
                cj.minDistance = 0;
                cj.maxDistance = nodeDistance;
                cj.spring = 1000f;
                cj.enableCollision = false;
                cj.tolerance = 0;
            }
            else
            {
                node.rigidbody.isKinematic = true;
            }

            if (index != nodes.Count - 1) return;
            if (helderJoint == null) return;
            helderJoint.connectedBody = node.rigidbody;
        }
    }
}