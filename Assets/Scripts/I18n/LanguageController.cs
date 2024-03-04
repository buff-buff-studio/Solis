using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SolarBuff.i18n
{
    public class LanguageController : SingletonBehaviour<LanguageController>
    {
        private Dictionary<int, string[]> _cache = null;
    
        [SerializeField]
        private string currentLanguage = "EnUs";
        public Action onLanguageChange;
        public string CurrentLanguage => currentLanguage;

        [SerializedDictionary("key", "name")]
        public SerializedDictionary<string, string> languages = new SerializedDictionary<string, string>();

        public void SetCurrentLanguage(string language)
        {
            if (currentLanguage == language) return;
            
            currentLanguage = language;
            _cache = null;
            
            onLanguageChange?.Invoke();
        }

        private void _CheckCache()
        {
            if (_cache != null) return;
            
            //Read Content
            var asset = Resources.Load<TextAsset>("Languages/" + currentLanguage);

            if (asset == null)
            {
                Debug.LogWarning($"Language {currentLanguage} not found!");
                _cache = new Dictionary<int, string[]>();
                return;
            }
            
            //Parse Entries
            var unbakedCache = new Dictionary<string, Stack<string>>();
            var key = "null";
            foreach (var line in asset.text.Split(new[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                var i = line.IndexOf("=", StringComparison.Ordinal);
                if(i == -1) continue;

                if (i > 0)
                    key = line.Substring(0, i);
                var value = line.Substring(i + 1);

                if (unbakedCache.TryGetValue(key, out var cache))
                {
                    cache.Push(value);
                }
                else
                {
                    var stack = new Stack<string>();
                    stack.Push(value);
                    
                    unbakedCache.Add(key, stack);
                }
            }
        
            //Bake Cache
            _cache = new Dictionary<int, string[]>();
            foreach (var pair in unbakedCache)
            {
                _cache[StringToHash(pair.Key)] = pair.Value.ToArray();
            }
        }
        
        #region Default Methods
        public string LocalizeLocal(int key)
        {
            _CheckCache();
            if (!_cache.TryGetValue(key, out var values)) return "???";
            return values.Length == 1 ? values[0] : values[Random.Range(0, values.Length)];
        }
        
        public string LocalizeLocal(int key, int index)
        {
            _CheckCache();
            return !_cache.TryGetValue(key, out var values) ? "???" : values[index];
        }

        public int GetLocalizationCountLocal(int key)
        {
            _CheckCache();
            return !_cache.TryGetValue(key, out var values) ? 0 : values.Length;
        }
        #endregion

        #region Helpers
        public string LocalizeLocal(int key, params object[] args)
        {
            return string.Format(LocalizeLocal(key), args);
        }
        
        public string LocalizeLocal(int key, int index, params object[] args)
        {
            return string.Format(LocalizeLocal(key, index), args);
        }
        
        public string LocalizeLocal(string key)
        {
            return LocalizeLocal(StringToHash(key));
        }

        public int GetLocalizationCountLocal(string key)
        {
            return !_cache.TryGetValue(StringToHash(key), out var values) ? 0 : values.Length;
        }
        
        public string LocalizeLocal(string key, int index)
        {
            return LocalizeLocal(StringToHash(key), index);
        }
        
        public string LocalizeLocal(string key, params object[] args)
        {
            return string.Format(LocalizeLocal(StringToHash(key)), args);
        }
        
        public string LocalizeLocal(string key, int index , params object[] args)
        {
            return string.Format(LocalizeLocal(StringToHash(key), index), args);
        }
        #endregion

        #region Static Methods
        public static string Localize(int key)
        {
            return Instance.LocalizeLocal(key);
        }
        
        public static string Localize(int key, int index)
        {
            return Instance.LocalizeLocal(key, index);
        }
        
        public static string Localize(int key, params object[] args)
        {
            return string.Format(Instance.LocalizeLocal(key), args);
        }
        
        public static string Localize(int key, int index, params object[] args)
        {
            return string.Format(Instance.LocalizeLocal(key, index), args);
        }
        
        public static string Localize(string key)
        {
            return Instance.LocalizeLocal(StringToHash(key));
        }
        
        public static int GetLocalizationCount(string key)
        {
            return Instance.GetLocalizationCountLocal(key);
        }
        
        public static int GetLocalizationCount(int key)
        {
            return Instance.GetLocalizationCountLocal(key);
        }
        
        public static string Localize(string key, int index)
        {
            return Instance.LocalizeLocal(StringToHash(key), index);
        }
        
        public static string Localize(string key, params object[] args)
        {
            return string.Format(Instance.LocalizeLocal(StringToHash(key)), args);
        }
        
        public static string Localize(string key, int index , params object[] args)
        {
            return string.Format(Instance.LocalizeLocal(StringToHash(key), index), args);
        }
        #endregion

        public static int StringToHash(string key)
        {
            return Animator.StringToHash(key);
        }
    }
}