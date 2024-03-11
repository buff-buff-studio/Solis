using System;
using MoonSharp.Interpreter;
using NetBuff.Components;
using UnityEngine;
using UnityEngine.Events;

namespace SolarBuff.Misc
{
    public enum SystemType
    {
        Any,
        Number,
        Boolean,
        String,
    }
    
    public abstract class SystemInput : NetworkBehaviour
    {
        public UnityEvent onValueChanged;
        public abstract SystemType GetSystemInputType();
        public abstract object GetSystemInput();
    }

    public abstract class SystemOutput : NetworkBehaviour
    {
        public abstract SystemType GetSystemOutputType();
        public abstract void SetSystemOutput(object output);
    }
    
    public class System : MonoBehaviour
    {
        public TextAsset script;
        private Script _script;
        
        public SystemInput[] inputs;
        public SystemOutput[] outputs;

        public void OnEnable()
        {
            foreach (var input in inputs)
                input.onValueChanged.AddListener(Refresh);

            _script = new Script
            {
                Globals =
                {
                    ["print"] = new Action<object>(Debug.Log),
                    
                    ["getBool"] = new Func<string, bool>(name => (bool)GetInput(name)),
                    ["getFloat"] = new Func<string, float>(name => (float)GetInput(name)),
                    ["getString"] = new Func<string, string>(name => (string)GetInput(name)),
                    ["setBool"] = new Action<string, bool>((name, value) => SetOutput(name, value)),
                    ["setFloat"] = new Action<string, float>((name, value) => SetOutput(name, value)),
                    ["setString"] = new Action<string, string>((name, value) => SetOutput(name, value))
                }
            };
            
            _script.DoString(script.text);
            Call("load");
            Refresh();
        }
        private void OnDisable()
        {
            foreach (var input in inputs)
                input.onValueChanged.RemoveListener(Refresh);
            
            Call("unload");
        }

        public void Refresh()
        {
            Call("update");
        }

        public void Restart() //util to reload-object positions
        {
            Call("restart");
        }

        private void Call(string funcName)
        {
            var func = _script.Globals[funcName];
            if (func == null)
                return;
            
            _script.Call(func);
        }

        private object GetInput(string name)
        {
            foreach (var input in inputs)
            {
                if (input.name == name)
                {
                    return input.GetSystemInput();
                }
            }
            
            throw new Exception("Input not found");
        }
        
        private void SetOutput(string name, object output)
        {
            foreach (var outputObj in outputs)
            {
                if (outputObj.name != name) continue;
                outputObj.SetSystemOutput(output);
                return;
            }
            
            throw new Exception("Output not found");
        }
    }
}