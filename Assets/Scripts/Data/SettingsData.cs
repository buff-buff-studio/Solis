using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Solis.Data
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Solis/Settings", order = 0)]
    public class SettingsData : ScriptableObject
    {
        [FormerlySerializedAs("boolItems")] 
        public SerializedDictionary<string, bool> toggleItems;
        
        [FormerlySerializedAs("intItems")] 
        public SerializedDictionary<string, int> arrowItems;
        
        [FormerlySerializedAs("floatItems")] 
        public SerializedDictionary<string, float> sliderItems;

        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}