using UnityEngine;

namespace Solis.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Player", order = 0)]
    public class PlayerData : ScriptableObject
    {
        [Header("MOVEMENT")]
        public float maxSpeed = 12f;
        [Tooltip("Meters Per Second")]
        public float acceleration = 0.1f;
        public float deceleration = 0.1f;
        public float accelInJumpMultiplier = 1.5f;
        public float rotationSpeed = 25;

        [Header("JUMP")]
        [Tooltip("Meters Per Second")]
        public float jumpMaxHeight = 2f;
        public float jumpAcceleration = 0.1f;
        public float jumpGravityMultiplier = 0.5f;
        public float jumpCutMinHeight = 1.5f;
        public float jumpCutGravityMultiplier = 0.75f;
        [Range(0, 1)]
        public float coyoteTime = 0.3f;
        [Range(0, 3)]
        public float timeToJump = 0.5f;

        [Header("GRAVITY")]
        [Tooltip("The recommended value is -9.81")]
        public float gravity = -9.81f;
        public float fallMultiplier = 2.5f;
        public float maxFallSpeed = 20f;
        [Range(1, 0)] [Tooltip("The lower the value, the greater the deceleration")]
        public float maxHeightDecel = 0.85f;
        [Range(1, 0)] [Tooltip("The lower the value, the greater the deceleration")]
        public float hitHeadDecel = 0.15f;

        [Header("COOLDOWNS")]
        public float interactCooldown;
        public float respawnCooldown = 3f;

        public void Update()
        {
            jumpCutMinHeight = Mathf.Clamp(jumpCutMinHeight, 0, jumpMaxHeight);
            Debug.Log("PlayerData Updated");
        }
    }
}