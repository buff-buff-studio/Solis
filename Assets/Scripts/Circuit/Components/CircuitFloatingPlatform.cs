using System;
using System.Collections.Generic;
using Solis.Player;
using UnityEditor;
using UnityEngine;
using VFX;
using Object = UnityEngine.Object;

namespace Solis.Circuit.Components
{
    public class CircuitFloatingPlatform : CircuitPlatform
    {
        [Space]
        [Header("WATER")]
        public WaterFlowingRegion flowingRegion;
#if UNITY_EDITOR
        [HideInInspector]
        public bool WaterFlowingRegionFoldout;
#endif
        private float waterSpeed;

        private void Start()
        {
            waterSpeed = flowingRegion.speed;
            flowingRegion.speed = 0;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if(flowingRegion == null) return;
            var isMoving = position.Value is < 1 and > 0;
            if (isMoving && canBeMoving && flowingRegion.speed == 0)
            {
                flowingRegion.speed = value ? waterSpeed : -waterSpeed;
            }else if (!isMoving && flowingRegion.speed != 0)
            {
                flowingRegion.speed = 0;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CircuitFloatingPlatform), true), CanEditMultipleObjects]
    public class PlayerControllerBaseEditor : UnityEditor.Editor
    {
        private CircuitFloatingPlatform _platform;
        private Editor _waterFlowingRegionEditor;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawSettingsEditor(_platform.flowingRegion, null, ref _platform.WaterFlowingRegionFoldout, ref _waterFlowingRegionEditor);
        }

        public void DrawSettingsEditor(Object settings, Action onSettingsUpdated, ref bool foldout, ref Editor editor)
        {
            if (settings == null) return;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                foldout =  EditorGUILayout.InspectorTitlebar(foldout, settings);
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        onSettingsUpdated?.Invoke();
                    }
                }
            }
        }

        private void OnEnable()
        {
            _platform = (CircuitFloatingPlatform) target;
        }
    }
#endif
}