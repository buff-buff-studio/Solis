using System;
using System.Collections.Generic;
using System.Linq;
using NetBuff.Components;
using UnityEditor;
using UnityEngine;

namespace SolarBuff.Circuit
{
    public abstract class CircuitComponent : NetworkBehaviour
    {
        [SerializeField]
        public List<CircuitPlug> plugs = new();

        private int input = -1, output = -1;

        protected virtual void OnEnable()
        {
            input = plugs.Exists(p => p.type == CircuitPlug.Type.Input) ? plugs.FindIndex(p => p.type == CircuitPlug.Type.Input) : -1;
            output = plugs.Exists(p => p.type == CircuitPlug.Type.Output) ? plugs.FindIndex(p => p.type == CircuitPlug.Type.Output) : -1;
            if(plugs.Count == 0)
                plugs = GetComponentsInChildren<CircuitPlug>().ToList();
            OnRefresh();
        }

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnDestroy()
        {
            //Destroy all connections
            foreach (var plug in GetPlugs())
            {
                if (plug.Connection == null) continue;
                Destroy((plug.Connection as MonoBehaviour)!.gameObject);
            }
        }
        
        public virtual T ReadOutput<T>(CircuitPlug plug)
        {
            return default;
        }
        
        public virtual bool IsHighVoltage(CircuitPlug plug)
        {
            return false;
        }
        
        protected virtual void OnRefresh()
        {
            
        }

        public void Refresh()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            OnRefresh();
            
            Debug.Log(gameObject.name + " " + GetType());
            //Spread through output ports
            foreach (var plug in GetPlugs())
            {
                if (plug.type != CircuitPlug.Type.Output) continue;
                if (plug.Connection == null) continue;

                plug.Connection.Refresh();
            }
        }

        public static T SafeOutput<T>(bool b)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)b;
                case TypeCode.Single:
                    return (T)(object)(b ? 1f : 0f);
                default:
                    return default;
            }
        }
        
        public static T SafeOutput<T>(float f)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)(f > 0.5f);
                case TypeCode.Single:
                    return (T)(object)f;
                default:
                    return default;
            }
        }

        public IEnumerable<CircuitPlug> GetPlugs()
        {
            return plugs;
        }
        
        public bool GetPlugValue(CircuitPlug.Type type)
        {
            try
            {
                return type switch
                {
                    CircuitPlug.Type.Output => plugs[0].ReadValue<bool>(),
                    CircuitPlug.Type.Input => plugs[0].ReadValue<bool>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
            catch (Exception e)
            {
                Debug.LogError(e, this);
                return false;
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(CircuitComponent), true), CanEditMultipleObjects]
    public class VrButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (CircuitComponent) this.target;
            if (target.plugs == null || target.plugs.Count == 0 || target.plugs.Exists(p => p == null))
            {
                EditorGUILayout.HelpBox("This script has no plugs, it won't do anything.\nO script não tem plugs referenciados, ele não fará nada.", MessageType.Error);
                EditorGUILayout.Space(5f);
                if (GUILayout.Button("Create Output")) CreatePlug(target, true);
                if (GUILayout.Button("Create Input")) CreatePlug(target, false);
                if (GUILayout.Button("Locate Plugs")) target.plugs = target.GetComponentsInChildren<CircuitPlug>().ToList();
                EditorGUILayout.Space(10f);
            }

            base.OnInspectorGUI();
        }
            
        private static void CreatePlug(CircuitComponent target, bool isOutput)
        {
            var plug = new GameObject(target.name + (isOutput ? " Output" : " Input"));
            plug.transform.SetParent(target.transform);
            plug.transform.localPosition = Vector3.zero;
            plug.AddComponent<CircuitPlug>().type = isOutput ? CircuitPlug.Type.Output : CircuitPlug.Type.Input;
            target.plugs.Add(plug.GetComponent<CircuitPlug>());
        }
    }
#endif
}