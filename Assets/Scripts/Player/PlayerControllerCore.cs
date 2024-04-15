using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ExamplePlatformer;
using NetBuff;
using NetBuff.Components;
using NetBuff.Interface;
using NetBuff.Misc;
using SolarBuff.Circuit.Components.Testing;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace SolarBuff.Player
{
    public class PlayerControllerCore : MagnetObject
    {
        public enum PlayerType : int
        {
            Human,
            Robot,
            Both
        }
        
        public static readonly List<PlayerControllerCore> Players = new();

        [Header("References")]
        public CharacterController controller;
        public Transform body;
        public OrbitCamera cam;
        public NetworkAnimator animator;
        public TMP_Text headplate;
        public EmoteSystem emoteSystem;
        public PlayerType type;
        public ParticleSystem dustParticles, jumpParticles, landParticles;

        [Header("Network")]
        public int tickRate = 50;
        public StringNetworkValue nickname = new StringNetworkValue("");
        public IntNetworkValue pType = new IntNetworkValue(-1);

        public Color bodyRob;
        public Color skinRob;
        public Color bodyHum;
        public Color skinHum;

        private float remoteBodyRotation;
        private Vector3 remoteBodyPosition;

        [Header("Movement")]
        [Tooltip("Metros por Segundo")]
        public float maxSpeed = 12f;
        public float acceleration = 0.1f;
        public float deceleration = 0.1f;
        public float rotationSpeed = 25;

        [Header("Jump")]
        [Tooltip("Metros por Segundo")]
        public float jumpMaxHeight = 2f;
        public float jumpAcceleration = 0.1f;
        public float jumpGravityMultiplier = 0.5f;
        public float jumpCutGravityMultiplier = 0.75f;
        [Range(0,1)]
        public float coyoteTime = 0.3f;

        //Private Jump Variables
        private float _coyoteTimer;
        private float _startJumpPos;
        private bool _isJumping;
        private bool _isJumpCut;
        private bool _isFalling;

        [Header("Gravity")]
        [Tooltip("O valor recomendado é -9.81")]
        public float gravity = -9.81f;
        public float fallMultiplier = 2.5f;
        public float maxFallSpeed = 20f;
        private float _multiplier;

        [Header("Interactions")]
        public float punchCooldown;
        public Renderer[] bodyRenderers;
        private bool IsGrounded => controller.isGrounded;

        private Vector3 velocity;
        private Vector2 velocityXZ => new Vector2(velocity.x, velocity.z);

        //Inputs
        private float xInput => Input.GetAxis("Horizontal");
        private float zInput => Input.GetAxis("Vertical");
        private Vector2 moveInput => new Vector2(xInput, zInput);
        private bool jumpInput => Input.GetButtonDown("Jump");
        private bool jumpInputUp => Input.GetButtonUp("Jump");

        //Checks
        private bool CanJump => !_isJumping && (IsGrounded || _coyoteTimer > 0);
        private bool CanJumpCut => _isJumping && !_isJumpCut;

        //DEBUG
        private Vector3 DEBUG_nextMovePos;


        public Transform plataform = null;
        #region Network Events

        /// <summary>
        /// Tick the behaviour
        /// </summary>
        public void Tick()
        {
            if (!HasAuthority || !IsOwnedByClient)
                return;

            var packet = new PacketBodyLerp
            {
                Id = Id,
                BodyRotation = body.localEulerAngles.y,
                BodyPositionX = body.localPosition.x,
                BodyPositionY = body.localPosition.y,
                BodyPositionZ = body.localPosition.z
            };
            SendPacket(packet);
        }

        /// <summary>
        /// Called when a packet is received by the server
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="clientId"></param>
        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            switch (packet)
            {
                case PacketBodyLerp data:
                    if(clientId == OwnerId)
                        ServerBroadcastPacketExceptFor(data, clientId);
                    break;
            }
        }

        /// <summary>
        /// Called when a packet is received by the client
        /// </summary>
        /// <param name="packet"></param>
        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
            switch (packet)
            {
                case PacketBodyLerp bodyRot:
                    remoteBodyRotation = bodyRot.BodyRotation;
                    remoteBodyPosition = bodyRot.BodyPosition;
                    break;
            }
        }

        /// <summary>
        /// Called when the object is spawned
        /// </summary>
        /// <param name="isRetroactive"></param>
        public override void OnSpawned(bool isRetroactive)
        {
            if (!HasAuthority || !IsOwnedByClient)
                return;

            if (cam == null)
            {
                var idx = GetLocalClientIndex(OwnerId);

                if (LevelManager.Instance != null)
                {
                    cam = LevelManager.Instance.orbitCameras[idx];
                }
                else
                {
                    cam = FindAnyObjectByType<OrbitCamera>();
                }

                cam.target = gameObject;

                if (idx == 1)
                {
                    //Setup split screen
                    var cameraA = LevelManager.Instance.orbitCameras[0].GetComponent<Camera>();
                    var cameraB = cam.GetComponent<Camera>();

                    cameraA.rect = new Rect(0, 0, 0.5f, 1);
                    cameraB.rect = new Rect(0.5f, 0, 0.5f, 1);
                }
            }
            pType.Value = (int)(Players.FindIndex(p => p == this) == 0 ? PlayerType.Human : PlayerType.Robot);
            Debug.Log(type.ToString());
            nickname.Value = CreateRandomEnglishName();
            cam = cam ?? FindObjectOfType<OrbitCamera>();
            transform.position = GameManager.Instance.GetPlayerSpawnPoint( (PlayerType) pType.Value).position;
        }

        #endregion

        #region Unity Events

        public void OnEnable()
        {
            Players.Add(this);
            
            pType.OnValueChanged += OnPlayerTypeChange;
            remoteBodyRotation = body.localEulerAngles.y;
            remoteBodyPosition = body.localPosition;
            _multiplier = fallMultiplier;
            dustParticles.Stop();
            if (controller == null) TryGetComponent(out controller);
            InvokeRepeating(nameof(Tick), 0, 1f / tickRate);
            WithValues(nickname, pType);

            nickname.OnValueChanged += (oldValue, newValue) =>
            {
                headplate.text = newValue;
            };
        }

        private void OnPlayerTypeChange(int oldvalue, int newvalue)
        {
            if(!HasAuthority) return;
            
            type = (PlayerType)newvalue;
            Debug.LogWarning(type.ToString());
            var bodyCol = newvalue == 0 ? bodyHum : bodyRob;
            var skinCol = newvalue == 0 ? skinHum : skinRob;
            bodyRenderers[3].materials[2].color = newvalue == 0 ? Color.black : Color.yellow;
            
            foreach (var r in bodyRenderers)
            {
                r.materials[0].color = bodyCol;
                if(r.materials.Length > 1) r.materials[1].color = skinCol;
            }
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(Tick));
        }

        private void Update()
        {
            if (!HasAuthority || !IsOwnedByClient) return;

            Timer();

            Move();
            Jump();
            Punch();

            if (Input.GetKeyDown(KeyCode.Keypad1))
                emoteSystem.ShowEmote(0, type);
            else if (Input.GetKeyDown(KeyCode.Keypad2))
                emoteSystem.ShowEmote(1, type);
            else if (Input.GetKeyDown(KeyCode.Keypad3))
                emoteSystem.ShowEmote(2, type);
        }

        private void FixedUpdate()
        {
            if (!HasAuthority || !IsOwnedByClient)
            {
                body.localEulerAngles = new Vector3(0, Mathf.LerpAngle(body.localEulerAngles.y, remoteBodyRotation, Time.deltaTime * 20), 0);
                body.localPosition = Vector3.Lerp(body.localPosition, remoteBodyPosition, Time.deltaTime * 20);
                return;
            }

            Gravity();

            if (cam == null) cam = FindObjectOfType<OrbitCamera>();
            var camAngle = cam.transform.eulerAngles.y;
            var moveAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            var te = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(te.y, camAngle, velocityXZ.magnitude * Time.deltaTime * rotationSpeed), 0);
            body.localEulerAngles = new Vector3(0, Mathf.LerpAngle(body.localEulerAngles.y, moveAngle, velocityXZ.magnitude * Time.deltaTime * rotationSpeed * 1f));
            if(velocityXZ.magnitude > 0.8f) dustParticles.Play();
            else dustParticles.Stop();

            var move = Quaternion.Euler(0, te.y, 0) * velocity;
            if(punchCooldown > 0) move = Vector3.zero;

            //Verifica se assim que o jogador se mover tenha chão, se não tiver, ele zera a velocidade x e z
            var walking = velocityXZ.magnitude > 0.1f;
            var nextPos = transform.position + new Vector3(move.x, 0, move.z) * Time.fixedDeltaTime;
            DEBUG_nextMovePos = nextPos;
            if (Physics.CheckSphere(transform.position, 0.5f, LayerMask.GetMask("Default")))
            {

                if (!Physics.Raycast(nextPos, Vector3.down, 1.1f) && !_isJumping && IsGrounded)
                {
                    walking = false;
                    velocity.x = velocity.z = 0;
                    move = Vector3.zero;
                }
            }

            controller.Move(new Vector3(move.x, velocity.y, move.z) * Time.fixedDeltaTime);

            animator.SetBool("Jumping", _isJumping);
            animator.SetFloat("Running", Mathf.Lerp(animator.GetFloat("Running"), walking ? 1 : 0, Time.deltaTime * 5f));

            if(transform.position.y < -10)
            {
                transform.position = new Vector3(0, 1, 0);
                velocity = Vector3.zero;
            }
        }

        private void Punch()
        {
            if (Input.GetKeyDown(KeyCode.Q) && punchCooldown <= 0 && IsGrounded)
            {
                animator.SetTrigger("Punch");
                punchCooldown = 1.25f;

                //Run after
                Task.Run(async () =>
                {
                    await Task.Delay(500);

                    SendPacket(new PlayerPunchActionPacket
                    {
                        Id = Id
                    }, true);
                });
            }
        }

        #endregion

        public void OnDrawGizmos()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out var hit, 1.1f))
            {
                Gizmos.color = hit.collider.gameObject.layer == LayerMask.NameToLayer("Default") ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }

            if (Physics.Raycast(DEBUG_nextMovePos, Vector3.down, 1.1f))
            {
                Gizmos.color = !_isJumping && IsGrounded ? Color.green : Color.yellow;
                Gizmos.DrawRay(DEBUG_nextMovePos, hit.point - DEBUG_nextMovePos);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(DEBUG_nextMovePos, Vector3.down);
            }

        }

        private void Timer()
        {
            _coyoteTimer = IsGrounded ? coyoteTime : _coyoteTimer - Time.deltaTime;
            if (punchCooldown > 0)
            {
                punchCooldown -= Time.deltaTime;
            }
        }
        Vector3 plataformDeslocation = Vector3.zero;
        private void Move()
        {
            var target = moveInput * maxSpeed;
            var accelerationValue = ((Mathf.Abs(moveInput.magnitude) > 0.01f) ? acceleration : deceleration) * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, target.x, accelerationValue);
            velocity.z = Mathf.MoveTowards(velocity.z, target.y, accelerationValue);
            
            if (plataform != null)
            {
                Vector3 deltaPosition = Vector3.zero - plataformDeslocation;
                transform.position += deltaPosition;
                plataformDeslocation = plataform.position - plataform.position;
            }
            if(IsGrounded && plataform != null) velocity.y = 0;

        }

        private void Jump()
        {
            if (jumpInput && CanJump)
            {
                _isJumping = true;
                _isJumpCut = false;
                velocity.y = 0.1f;
                _startJumpPos = transform.position.y;
                _multiplier = jumpGravityMultiplier;
                Debug.Log("Jump");
                jumpParticles.Play();
            }
            if (jumpInputUp && CanJumpCut)
            {
                _isJumpCut = true;
                _isJumping = false;
                velocity.y *= 0.5f;
                _multiplier = jumpCutGravityMultiplier;
                Debug.Log("Jump Cut");
            }

            if (_isJumping && !_isJumpCut)
            {
                var diff = Mathf.Abs((transform.position.y + (jumpAcceleration * Time.fixedDeltaTime)) - _startJumpPos);
                velocity.y += jumpAcceleration * Time.fixedDeltaTime;
                if (diff >= jumpMaxHeight)
                {
                    _isJumping = false;
                    Debug.Log("Jump End by Max Height");
                }
            }
        }

        private void Gravity()
        {
            if (IsGrounded)
            {
                _multiplier = fallMultiplier;
                if (_isFalling)
                {
                    landParticles.Play();
                    Debug.Log("Land");
                    _isFalling = false;
                }
                return;
            }else if(_isJumping) return;
            _isFalling = velocity.y < (gravity * _multiplier)/2;
            velocity.y += gravity * _multiplier * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        }

        #region Extra

        private string CreateRandomEnglishName()
        {
            string[] vowels = {"a", "e", "i", "o", "u"};
            string[] others = {"jh", "w", "n", "g", "gn", "b", "t", "th", "r", "l", "s", "sh", "k", "m", "d", "f", "v", "z", "p", "j", "ch"};

            var current = "";
            var b = Random.Range(0, 2) == 0;

            for (var i = 0; i < Random.Range(3, 10); i++)
            {
                if (b)
                    current += vowels[Random.Range(0, vowels.Length)];
                else
                    current += others[Random.Range(0, others.Length)];

                b = !b;
            }

            return current[..1].ToUpper() + current[1..];
        }

        #endregion
    }

    public class PacketBodyLerp : IOwnedPacket
    {
        public NetworkId Id { get; set; }
        public float BodyRotation { get; set; }
        public float BodyPositionX { get; set; }
        public float BodyPositionY { get; set; }
        public float BodyPositionZ { get; set; }
        internal Vector3 BodyPosition => new Vector3(BodyPositionX, BodyPositionY, BodyPositionZ);

        public void Serialize(BinaryWriter writer)
        {
            Id.Serialize(writer);
            writer.Write(BodyRotation);
            writer.Write(BodyPositionX);
            writer.Write(BodyPositionY);
            writer.Write(BodyPositionZ);
        }

        public void Deserialize(BinaryReader reader)
        {
            Id = NetworkId.Read(reader);
            BodyRotation = reader.ReadSingle();
            BodyPositionX = reader.ReadSingle();
            BodyPositionY = reader.ReadSingle();
            BodyPositionZ = reader.ReadSingle();
        }
    }

    public class PlayerPunchActionPacket : IPacket
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