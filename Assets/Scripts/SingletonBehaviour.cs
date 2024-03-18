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
                if (Application.isEditor)
                    Debug.LogWarning($"Multiple instances of {typeof(T)} found. Destroying {this}.");
                
                if(gameObject.GetComponents<Component>().Length <= 2)
                    Destroy(gameObject);
                else
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