using System;
using UnityEditor;
using UnityEngine;

namespace Solis.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Solis/Player", order = 0)]
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
        [Range(0, 3)]
        public float timeToJump = 0.5f;

        [Header("JUMP CUT")]
        public float jumpCutMinHeight = 1.5f;
        public float jumpCutGravityMultiplier = 0.75f;

        [Header("JUMP BUFFER")]
        [Range(0, 1)] [Tooltip("WIP")]
        public float coyoteTime = 0.3f;

        [Header("JUMP GRAVITY")]
        public float jumpGravityMultiplier = 0.5f;
        [Range(1, 0)] [Tooltip("The lower the value, the greater the deceleration")]
        public float maxHeightDecel = 0.85f;
        [Range(1, 0)] [Tooltip("The lower the value, the greater the deceleration")]
        public float hitHeadDecel = 0.15f;

        [Header("GRAVITY")]
        [Tooltip("The recommended value is -9.81")]
        public float gravity = -9.81f;
        public float fallMultiplier = 2.5f;
        public float maxFallSpeed = 20f;

        [Header("COOLDOWNS")]
        public float interactCooldown;
        public float respawnCooldown = 3f;

        [Header("MISC")]
        public float nextMoveMultiplier = 15f;

#if UNITY_EDITOR
        protected internal bool debugJumpSim = false;
#endif

        public void Update()
        {
            jumpCutMinHeight = Mathf.Clamp(jumpCutMinHeight, 0, jumpMaxHeight);
            Debug.Log("PlayerData Updated");
            //coyoteTime = 0;
        }
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PlayerData)), CanEditMultipleObjects]
    public class PlayerDataEditor : UnityEditor.Editor
    {
        private PlayerData playerData;
        private AnimationCurve jumpCurve;
        private Vector2 jumpCurveSize;
        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (check.changed)
                {
                    playerData.Update();
                    CalculateJumpCurve();
                }
            }
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Jump Curve", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Height", jumpCurveSize.y.ToString("0.##m"), EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Max Distance", jumpCurveSize.x.ToString("0.##m"), EditorStyles.miniLabel);
            EditorGUILayout.CurveField(jumpCurve, Color.cyan, new Rect(-.25f, -.25f, jumpCurveSize.x+.25f, jumpCurveSize.y+.5f),
                GUILayout.Height(100), GUILayout.ExpandWidth(true));
            playerData.debugJumpSim = EditorGUILayout.Toggle("Debug Jump Simulation", playerData.debugJumpSim);
            if (GUILayout.Button("Update Jump Curve"))
            {
                CalculateJumpCurve();
            }

            if(GUI.changed) EditorUtility.SetDirty(playerData);
        }

        private void CalculateJumpCurve()
        {
            jumpCurve = new AnimationCurve();
            jumpCurveSize = Vector2.zero;
            jumpCurve.AddKey(0, 0);
            var time = 0;
            Vector2 pos = Vector2.zero, vel = new Vector2(playerData.maxSpeed,0.1f);

            if(playerData.debugJumpSim) Debug.Log("Jump Simulation: Start");
            for(; pos.y+ (vel.y*Time.fixedDeltaTime) < playerData.jumpMaxHeight; time++)
            {
                vel.y += playerData.jumpAcceleration * Time.fixedDeltaTime;
                vel.x = Mathf.MoveTowards(vel.x, playerData.maxSpeed * playerData.accelInJumpMultiplier,
                    playerData.acceleration * Time.fixedDeltaTime);
                pos += vel * Time.fixedDeltaTime;
                if(playerData.debugJumpSim) Debug.Log($"Time: {time}, Pos: {pos}, Vel: {vel}");
                jumpCurve.AddKey(pos.x, pos.y);
            }

            if(playerData.debugJumpSim) Debug.Log("Jump Simulation: Max Height Reached");
            vel.y *= playerData.maxHeightDecel;

            for(; pos.y > 0; time++)
            {
                vel.y += playerData.gravity * playerData.jumpGravityMultiplier * Time.fixedDeltaTime;
                vel.y = Mathf.Max(vel.y, -playerData.maxFallSpeed);
                vel.x = Mathf.MoveTowards(vel.x, playerData.maxSpeed * playerData.accelInJumpMultiplier,
                    playerData.acceleration * Time.fixedDeltaTime);
                pos += vel * Time.fixedDeltaTime;
                jumpCurve.AddKey(pos.x, Mathf.Clamp(pos.y, 0, float.MaxValue));
                if (jumpCurveSize.y < pos.y)
                    jumpCurveSize.y = pos.y;

                if(playerData.debugJumpSim) Debug.Log($"Time: {time}, Pos: {pos}, Vel: {vel}");
            }
            jumpCurveSize.x = pos.x;
            if(playerData.debugJumpSim) Debug.Log("Jump Simulation: End");
        }

        private void OnEnable()
        {
            playerData = (PlayerData) target;
            CalculateJumpCurve();
        }
    }
    #endif
}