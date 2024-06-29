using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Solis.Data
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Solis/Settings", order = 0)]
    public class SettingsData : ScriptableObject
    {
        public SerializedDictionary<string, bool> boolItems;
        
        public SerializedDictionary<string, int> intItems;
        
        public SerializedDictionary<string, float> floatItems;

        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}