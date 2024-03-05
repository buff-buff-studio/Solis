using System;
using System.Collections.Generic;
using System.IO;
using LightJson;
using UnityEngine;

namespace SolarBuff.Data
{
    public class SaveManager : SingletonBehaviour<SaveManager>
    {
        public static string SaveFolder => Application.persistentDataPath + "/saves";
        
        [SerializeField, HideInInspector]
        public SaveProfile currentProfile;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);

            var profile = CreateNewSave();
            profile.Save();
            DontDestroyOnLoad(gameObject);
        }
        
        public IEnumerable<SaveProfile> GetSaveProfiles()
        {
            var dirs = Directory.GetDirectories(SaveFolder);
            foreach (var dir in dirs)
            {
                var dataPath = dir + "/data.json";
                if (!File.Exists(dataPath))
                    continue;
                
                var data = File.ReadAllText(dataPath);
                var obj = JsonValue.Parse(data);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(dir + "/thumbnail.png"));
                
                var profile = new SaveProfile
                {
                    name = Path.GetFileName(dir),
                    creationTime = long.Parse(obj["creationTime"]),
                    playTime = long.Parse(obj["playTime"]) / (float) TimeSpan.TicksPerSecond,
                    modifiedTime = long.Parse(obj["modifiedTime"]),
                    thumbnail = tex
                };
                yield return profile;
            }
        }
        
        public SaveProfile CreateNewSave()
        {
            return new SaveProfile
            {
                creationTime = DateTime.Now.Ticks,
                playTime = 0
            };
        }
        
        public bool Exists(string saveName)
        {
            return Directory.Exists(SaveFolder + "/" + saveName);
        }
        
        public void Delete(string saveName)
        {
            if (Exists(saveName))
                Directory.Delete(SaveFolder + "/" + saveName, true);
        }

        private void Update()
        {
            if(currentProfile != null)
                currentProfile.playTime += Time.deltaTime;
        }
    }
}