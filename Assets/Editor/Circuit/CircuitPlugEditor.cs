using Solis.Circuit;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Editor.Circuit
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(CircuitPlug), true)]
    [CanEditMultipleObjects]
    public class CircuitPlugEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var plug = target as CircuitPlug;
            if (plug == null)
                return;

            EditorGUI.BeginDisabledGroup(true);
            if (plug.acceptMultipleConnections)
            {
                var connections = plug.Connections;
                foreach (var circuitConnection in connections)
                {
                    var connection = circuitConnection as MonoBehaviour;
                    EditorGUILayout.ObjectField("Connection", connection != null ? connection.gameObject : null,
                        typeof(GameObject), true);
                }
            }
            else
            {
                var connection = plug.Connection as MonoBehaviour;
                EditorGUILayout.ObjectField("Connection", connection != null ? connection.gameObject : null,
                    typeof(GameObject), true);
            }

            EditorGUI.EndDisabledGroup();
        }

        public bool HasFrameBounds()
        {
            return true;
        }

        public Bounds OnGetFrameBounds()
        {
            return new Bounds(((CircuitPlug)target).transform.position, Vector3.one * 0.5f);
        }
    }
    #endif
}