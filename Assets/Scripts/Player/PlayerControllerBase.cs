using System;
using System.Threading.Tasks;
using Cinemachine;
using NetBuff.Components;
using NetBuff.Interface;
using Solis.Circuit.Components;
using Solis.Data;
using Solis.Misc;
using Solis.Packets;
using UnityEngine;

namespace Solis.Player
{
    /// <summary>
    /// Base class for player controllers.
    /// </summary>
    public abstract class PlayerControllerBase : NetworkBehaviour
    {
        #region Types
        /// <summary>
        /// Represents the state of the player.
        /// </summary>
        public enum State
        {
            Normal,
            Magnetized
        }

        /// <summary>
        /// Represents the death type of the player.
        /// </summary>
        public enum Death
        {
            Stun,
            Fall
        }
        #endregion

        #region Inspector Fields
        [Header("REFERENCES")]
        public CharacterController controller;
        public Transform body;
        public Transform lookAt;
        public NetworkAnimator animator;
        public ParticleSystem dustParticles;
        public ParticleSystem jumpParticles;
        public ParticleSystem landParticles;
        public PlayerEmoteController emoteController;

        [Header("MOVEMENT")]
        public float maxSpeed = 12f;
        [Tooltip("Meters Per Second")]
        public float acceleration = 0.1f;
        public float deceleration = 0.1f;
        public float rotationSpeed = 25;

        [Header("JUMP")]
        [Tooltip("Meters Per Second")]
        public float jumpMaxHeight = 2f;
        public float jumpAcceleration = 0.1f;
        public float jumpGravityMultiplier = 0.5f;
        public float jumpCutGravityMultiplier = 0.75f;
        [Range(0, 3)]
        public float timeToJump = 0.5f;        
        [Range(0, 1)]
        public float coyoteTime = 0.3f;

        [Header("GRAVITY")]
        [Tooltip("The recommended value is -9.81")]
        public float gravity = -9.81f;
        public float fallMultiplier = 2.5f;
        public float maxFallSpeed = 20f;

        [Header("STATE")]
        public State state;
        public Vector3 velocity;
        public float interactCooldown;
        
        [Header("NETWORK")]
        public int tickRate = 50;

        [Header("MAGNETIZED")]
        public Vector3 magnetReferenceLocalPosition = new Vector3(0, 2, 0);

        public Transform magnetAnchor;
        public Transform handPosition;
        #endregion

        #region Private Fields
        private float _remoteBodyRotation;
        private Vector3 _remoteBodyPosition;
        private float _coyoteTimer;
        private float _jumpTimer;
        private float _startJumpPos;
        private bool _isJumping;
        private bool _isJumpCut;
        private bool _isFalling;
        private float _multiplier;

        private Vector3 _lastSafePosition;

        #if UNITY_EDITOR
        private Vector3 _nextMovePos;

        [Header("DEBUG")]
        public bool debugJump = false;
        #endif
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns whether the player is grounded or not.
        /// </summary>
        public bool IsGrounded => controller.isGrounded;
        
        /// <summary>
        /// Returns the character type of the player.
        /// </summary>
        public abstract CharacterType CharacterType { get; }
        
        #endregion

        #region Private Properties
        private float InputX => Input.GetAxis("Horizontal");
        private float InputZ => Input.GetAxis("Vertical");
        private Vector2 MoveInput => new(InputX, InputZ);
        private bool InputJump => Input.GetButtonDown("Jump");
        private bool InputJumpUp => Input.GetButtonUp("Jump");
        private bool CanJump => !_isJumping && (IsGrounded || _coyoteTimer > 0) && _jumpTimer <= 0;
        private bool CanJumpCut => _isJumping && !_isJumpCut;
        #endregion

        #region Unity Callbacks
        public void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _remoteBodyRotation = body.localEulerAngles.y;
            _remoteBodyPosition = body.localPosition;
            _multiplier = fallMultiplier;
            dustParticles.Stop();
            if (controller == null) TryGetComponent(out controller);
            InvokeRepeating(nameof(_Tick), 0, 1f / tickRate);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(_Tick));
        }

        private void Update()
        {
            if (!HasAuthority || !IsOwnedByClient) return;

            _Timer();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var cursorIsOn = Cursor.visible;
                Cursor.visible = !cursorIsOn;
                Cursor.lockState = cursorIsOn ? CursorLockMode.Locked : CursorLockMode.None;
            }

            switch (state)
            {
                case State.Normal:
                    _Move();
                    _Jump();
                    _Interact();
                    break;
                case State.Magnetized:
                    if (magnetAnchor == null)
                    {
                        state = State.Normal;
                        velocity = Vector3.zero;
                        controller.enabled = true;
                    }
                    else
                    {
                        var pos = magnetAnchor.position - magnetReferenceLocalPosition;
                        controller.enabled = false;
                        var playerPos = transform.position;
                        transform.position = Vector3.MoveTowards(playerPos, pos, Time.deltaTime * 15);
                    }

                    break;
            }


            if (Input.GetKeyDown(KeyCode.Alpha1))
                emoteController.ShowEmote(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                emoteController.ShowEmote(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                emoteController.ShowEmote(2);
        }

        private void FixedUpdate()
        {
            if (!HasAuthority || !IsOwnedByClient)
            {
                body.localEulerAngles = new Vector3(0,
                    Mathf.LerpAngle(body.localEulerAngles.y, _remoteBodyRotation, Time.fixedDeltaTime * 20), 0);
                body.localPosition = Vector3.Lerp(body.localPosition, _remoteBodyPosition, Time.fixedDeltaTime * 20);
                return;
            }

            switch (state)
            {
                case State.Normal:
                    _Gravity();
                    _HandlePlatform();

                    var camAngle = Camera.main!.transform.eulerAngles.y;
                    var moveAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
                    var te = transform.eulerAngles;
                    var velocityXZ = new Vector2(velocity.x, velocity.z);
                    transform.eulerAngles = new Vector3(0,
                        Mathf.LerpAngle(te.y, camAngle, velocityXZ.magnitude * Time.fixedDeltaTime * rotationSpeed), 0);
                    body.localEulerAngles = new Vector3(0,
                        Mathf.LerpAngle(body.localEulerAngles.y, moveAngle,
                            velocityXZ.magnitude * Time.fixedDeltaTime * rotationSpeed * 1f));
                    if (velocityXZ.magnitude > 0.8f) dustParticles.Play();
                    else dustParticles.Stop();

                    var move = Quaternion.Euler(0, te.y, 0) * velocity;
                    if (interactCooldown > 0)
                        move = Vector3.zero;

                    var walking = velocityXZ.magnitude > 0.1f;
                    var nextPos = transform.position + (new Vector3(move.x, 0, move.z) * (Time.fixedDeltaTime * 10f));

                    #if UNITY_EDITOR
                    _nextMovePos = nextPos;
                    #endif

                    if (Physics.CheckSphere(transform.position, 0.5f, LayerMask.GetMask("SafePlatform")))
                    {
                        if (!Physics.Raycast(nextPos, Vector3.down, 1.1f) && !_isJumping && IsGrounded)
                        {
                            walking = false;
                            velocity.x = velocity.z = 0;
                            move = Vector3.zero;
                        }
                    }

                    controller.Move(new Vector3(move.x, velocity.y, move.z) * Time.fixedDeltaTime);
                    if(IsGrounded && Physics.Raycast(nextPos, Vector3.down, out var hit, 0.1f) && hit.collider.gameObject.layer != LayerMask.NameToLayer("Platform"))
                    {
                        _lastSafePosition = transform.position;
                    }

                    animator.SetBool("Jumping", _isJumping);
                    animator.SetBool("Grounded", !_isFalling && IsGrounded);
                    animator.SetFloat("Running",
                        Mathf.Lerp(animator.GetFloat("Running"), walking ? 1 : 0, Time.deltaTime * 7f));

                    if (transform.position.y < -15)
                    {
                        PlayerDeath(Death.Fall);
                    }

                    break;
            }
        }

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 1.1f))
            {
                Gizmos.color = hit.collider.gameObject.layer == LayerMask.NameToLayer("SafePlatform")
                    ? Color.green
                    : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }

            if (Physics.Raycast(_nextMovePos, Vector3.down, 1.1f))
            {
                Gizmos.color = !_isJumping && IsGrounded ? Color.green : Color.yellow;
                Gizmos.DrawRay(_nextMovePos, hit.point - _nextMovePos);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_nextMovePos, Vector3.down);
            }

            Gizmos.color = IsGrounded ? Color.cyan : Color.blue;
            Gizmos.DrawWireCube(_lastSafePosition, Vector3.one);
#endif
        }
        #endregion

        #region Network Callbacks
        public override void OnSpawned(bool isRetroactive)
        {
            if (!HasAuthority)
                return;
            var cam = Camera.main!.GetComponentInChildren<CinemachineFreeLook>();
            cam.Follow = transform;
            cam.LookAt = lookAt;
        }

        public override void OnServerReceivePacket(IOwnedPacket packet, int clientId)
        {
            switch (packet)
            {
                case PlayerBodyLerpPacket bodyLerpPacket:
                    if (clientId == OwnerId)
                        ServerBroadcastPacketExceptFor(bodyLerpPacket, clientId);
                    break;
                case PlayerDeathPacket deathPacket:
                    if (clientId == OwnerId)
                        ServerBroadcastPacketExceptFor(deathPacket, clientId);
                    break;
            }
        }

        public override void OnClientReceivePacket(IOwnedPacket packet)
        {
            switch (packet)
            {
                case PlayerBodyLerpPacket bodyLerpPacket:
                    _remoteBodyRotation = bodyLerpPacket.BodyRotation;
                    _remoteBodyPosition = bodyLerpPacket.BodyPosition;
                    break;
                case PlayerDeathPacket deathPacket:
                    if (deathPacket.Id == Id)
                        PlayerDeath(deathPacket.Type);
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void _Timer()
        {
            var deltaTime = Time.deltaTime;
            _coyoteTimer = IsGrounded ? coyoteTime : _coyoteTimer - deltaTime;
            
            if(IsGrounded) _jumpTimer -= deltaTime;
            if (interactCooldown > 0)
            {
                interactCooldown -= deltaTime;
            }
        }

        private void _Interact()
        {
            if (Input.GetKeyDown(KeyCode.E) && interactCooldown <= 0 && IsGrounded)
            {
                animator.SetTrigger("Punch");
                interactCooldown = 1.25f;

                //Run after
                Task.Run(async () =>
                {
                    await Task.Delay(500);

                    SendPacket(new PlayerInteractPacket
                    {
                        Id = Id
                    }, true);
                });
            }
        }

        private void _Move()
        {
            var moveInput = MoveInput;
            var target = moveInput * maxSpeed;
            var accelerationValue = ((Mathf.Abs(moveInput.magnitude) > 0.01f) ? acceleration : deceleration) *
                                    Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, target.x, accelerationValue);
            velocity.z = Mathf.MoveTowards(velocity.z, target.y, accelerationValue);
        }

        private void _Jump()
        {
            if (InputJump && CanJump)
            {
                _isJumping = true;
                _isJumpCut = false;
                velocity.y = 0.1f;
                _startJumpPos = transform.position.y;
                _multiplier = jumpGravityMultiplier;
                _jumpTimer = timeToJump;
                jumpParticles.Play();
            }

            if (InputJumpUp && CanJumpCut)
            {
                _isJumpCut = true;
                _isJumping = false;
                velocity.y *= 0.5f;
                _multiplier = jumpCutGravityMultiplier;
            }

            if (_isJumping && !_isJumpCut)
            {
                velocity.y += jumpAcceleration * Time.deltaTime;
                var diff = Mathf.Abs(transform.position.y - _startJumpPos);
                if (diff >= jumpMaxHeight)
                    _isJumping = false;

#if UNITY_EDITOR
                if(debugJump) Debug.Log($"start: {_startJumpPos} current: {transform.position.y} diff: {diff}");
#endif
            }
        }

        private void _Gravity()
        {
            if (IsGrounded)
            {
                _multiplier = fallMultiplier;
                if (_isFalling)
                {
                    landParticles.Play();
                    _isFalling = false;
                }

                return;
            }

            if (_isJumping)
                return;

            _isFalling = velocity.y < (gravity * _multiplier) / 2;
            velocity.y += gravity * _multiplier * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        }

        private void _HandlePlatform()
        {
            if (!IsGrounded)
                return;

            var ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 1.1f))
            {
                var platform = hit.collider.GetComponentInParent<CircuitPlatform>();
                if (platform != null)
                {
                    controller.Move(platform.DeltaSinceLastFrame);
                }
            }
        }

        private void _Tick()
        {
            if (!HasAuthority || !IsOwnedByClient)
                return;

            var pos = body.localPosition;
            var packet = new PlayerBodyLerpPacket
            {
                Id = Id,
                BodyRotation = body.localEulerAngles.y,
                BodyPosition = new Vector3(pos.x, pos.y, pos.z)
            };
            SendPacket(packet);
        }
        #endregion

        #region Public Methods

        public void PlayerDeath(Death death)
        {
            Debug.Log("Player ID: " + Id + " died with type: " + death);
            controller.enabled = false;
            switch (death)
            {
                case Death.Stun:
                    Debug.Log("Death Smash");
                    break;
                case Death.Fall:
                    transform.position = _lastSafePosition;
                    velocity = Vector3.zero;
                    landParticles.Play();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(death), death, null);
            }
            controller.enabled = true;
        }

        #endregion
    }
}