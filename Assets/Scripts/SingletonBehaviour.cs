using System;
using UnityEngine;

namespace SolarBuff
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; }

        public bool IsValidInstance => this == Instance;

        protected virtual void OnEnable()
        {
            if (Instance != null && this != Instance)
            {
                Destroy(this);
                return;
            }
            
            Instance = (T) this;
            
        }

        protected virtual void OnDisable()
        {
            if (this == Instance)
                Instance = null;
        }
    }
}