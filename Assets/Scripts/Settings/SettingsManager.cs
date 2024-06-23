using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Solis.Data;
using Solis.Data.Saves;
using Solis.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Solis.Settings
{
    public class SettingsManager : WindowManager
    {
        private static string Path => Application.persistentDataPath;
        
        [SerializeField]
        private SettingsData settingsData;
        
        public SettingsTab[] settingsTabs;

        [Space(10)] 
        [SerializeField] private SerializedDictionary<string, Toggle> boolItems;
        [SerializeField] private SerializedDictionary<string, ArrowItems> intItems;
        [SerializeField] private SerializedDictionary<string, Slider> floatItems;

#if UNITY_EDITOR
        public bool tryLocateItems;
        public bool resetItems;        
        public bool renameItems;   
        public bool resetSO;
#endif
        
        public static Action OnSettingsChanged;
        
        private readonly List<Resolution> _supportedResolutions = new List<Resolution>
        {//4k 21:9 to FHD, 4k 16:9 to HD, FHD 4:3 to SD
            new Resolution {width = 3840, height = 1600}, //21:9
            new Resolution {width = 3840, height = 2160}, //16:9
            new Resolution {width = 2560, height = 1080}, //21:9
            new Resolution {width = 1920, height = 1080}, //16:9
            new Resolution {width = 1440, height = 1080}, //4:3
            new Resolution {width = 1280, height = 960}, //4:3
            new Resolution {width = 1280, height = 720}, //16:9
            new Resolution {width = 1024, height = 768}, //4:3
            new Resolution {width = 800, height = 600}, //4:3
        };

        #region Unity Callbacks
        
        private void Awake()
        {
            Load();
            
            foreach (var i in intItems)
                i.Value.onChangeItem.AddListener(index => { settingsData.arrowItems[i.Key] = index; OnSettingsChanged?.Invoke(); });
            foreach (var f in floatItems)
                f.Value.onValueChanged.AddListener(value => { settingsData.sliderItems[f.Key] = value; OnSettingsChanged?.Invoke(); });
            foreach (var b in boolItems)
                b.Value.onValueChanged.AddListener(value => { settingsData.toggleItems[b.Key] = value; OnSettingsChanged?.Invoke(); });
            
            
            intItems["resolution"].SetItems(_supportedResolutions.Select(r => $"{r.width}x{r.height}").ToList());
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F))
                Screen.fullScreen = !Screen.fullScreen;
        }

        private void OnEnable()
        {
            OnSettingsChanged += ApplySettings;
        }

        private void OnDisable()
        {
            OnSettingsChanged -= ApplySettings;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if(tryLocateItems)
            {
                var toggles = GetComponentsInChildren<Toggle>(false);
                var arrows = GetComponentsInChildren<ArrowItems>(false);
                var sliders = GetComponentsInChildren<Slider>(false);
                var b = boolItems.Keys.ToList();
                var i = intItems.Keys.ToList();
                var f = floatItems.Keys.ToList();
                foreach (var item in b)
                {
                    boolItems[item] = Array.Find(toggles, t => t.name == item);
                }
                foreach (var item in i)
                {
                    intItems[item] = Array.Find(arrows, a => a.name == item);
                }
                foreach (var item in f)
                {
                    floatItems[item] = Array.Find(sliders, s => s.name == item);
                }
                
                tryLocateItems = false;
            }
            if (resetItems)
            {
                boolItems.Clear();
                intItems.Clear();
                floatItems.Clear();

                foreach (var b in settingsData.toggleItems)
                    boolItems.Add(b.Key, null);
                foreach (var i in settingsData.arrowItems)
                    intItems.Add(i.Key, null);
                foreach (var f in settingsData.sliderItems)
                    floatItems.Add(f.Key, null);
                
                resetItems = false;
            }
            if (renameItems)
            {
                foreach (var item in boolItems)
                {
                    item.Value.gameObject.name = item.Key;
                    item.Value.transform.parent.name = item.Key + " (Toggle)";
                }
                
                foreach (var item in intItems)
                {
                    item.Value.gameObject.name = item.Key;
                    item.Value.transform.parent.name = item.Key + " (Arrow)";
                }
                
                foreach (var item in floatItems)
                {
                    item.Value.gameObject.name = item.Key;
                    item.Value.transform.parent.name = item.Key + " (Slider)";
                }
                
                renameItems = false;
            }
            if (resetSO)
            {
                settingsData.toggleItems.Clear();
                settingsData.arrowItems.Clear();
                settingsData.sliderItems.Clear();
                
                foreach (var item in boolItems)
                    settingsData.toggleItems.Add(item.Key, false);
                foreach (var item in intItems)
                    settingsData.arrowItems.Add(item.Key, 0);
                foreach (var item in floatItems)
                    settingsData.sliderItems.Add(item.Key, 0);
                
                resetSO = false;
            }

            if(!Application.isPlaying)
            {
                OnTabSelected(currentIndex);
            }
            
            base.OnValidate();
        }
#endif

        #endregion
        
        private void ApplySettings()
        {
            try
            {
                QualitySettings.vSyncCount = settingsData.toggleItems["vsync"] ? 1 : 0;
                QualitySettings.SetQualityLevel(settingsData.arrowItems["graphics"]);
                Screen.fullScreen = settingsData.toggleItems["fullscreen"];
                if (settingsData.arrowItems["resolution"] < 0 || settingsData.arrowItems["resolution"] >= _supportedResolutions.Count)
                {
                    settingsData.arrowItems["resolution"] = GetScreenResolution();
                    intItems["resolution"].currentIndex = settingsData.arrowItems["resolution"];
                }
                Screen.SetResolution(_supportedResolutions[settingsData.arrowItems["resolution"]].width, _supportedResolutions[settingsData.arrowItems["resolution"]].height, settingsData.toggleItems["fullscreen"]);
                Debug.Log($"Settings applied: {QualitySettings.GetQualityLevel()} {Screen.width}x{Screen.height} {Screen.fullScreen} - VSync: {QualitySettings.vSyncCount}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Load()
        {
            if (File.Exists(Path + "/game.config"))
            {
                settingsData.LoadFromJson(File.ReadAllText(Path + "/game.config"));
                SetItems();
            }else this.Save();
        }

        public void Save()
        {
            if (!File.Exists(Path + "/game.config"))
            {
                File.Create(Path + "/game.config").Dispose();
                ResetToDefault();
            }
            File.WriteAllText(Path + "/game.config", JsonUtility.ToJson(settingsData, true));
            OnSettingsChanged?.Invoke();
            ApplySettings();
        }

        private void SetItems()
        {
            foreach (var g in settingsData.toggleItems)
                boolItems[g.Key].isOn = g.Value;
            foreach (var g in settingsData.arrowItems)
                intItems[g.Key].currentIndex = g.Value;
            foreach (var g in settingsData.sliderItems)
            {
                floatItems[g.Key].value = g.Value;
                floatItems[g.Key].onValueChanged.Invoke(g.Value);
            }

            ApplySettings();
            OnSettingsChanged?.Invoke();
        }

        private void OnTabSelected(int index)
        {
            Debug.Log($"Selected tab {index}");
            for (var i = 0; i < settingsTabs.Length; i++)
            {
                if (i == index)
                {
                    settingsTabs[i].SelectTab();
                }
                else
                {
                    settingsTabs[i].DeselectTab();
                }
            }
        }
        
        public override void ChangeWindow(int index)
        {
            base.ChangeWindow(index);
            OnTabSelected(index);
        }

        public void ResetToDefault()
        {
            //Video
            settingsData.arrowItems["resolution"] = GetScreenResolution();
            settingsData.arrowItems["graphics"] = 1;
            settingsData.toggleItems["fullscreen"] = true;
            settingsData.toggleItems["vsync"] = true;
            settingsData.toggleItems["motionBlur"] = true;
            
            //Gameplay
            settingsData.sliderItems["cameraSensitivity"] = 1;
            settingsData.toggleItems["invertXAxis"] = false;
            settingsData.toggleItems["invertYAxis"] = true;
            
            //Sound
            settingsData.sliderItems["masterVolume"] = 100;
            settingsData.sliderItems["musicVolume"] = 50;
            settingsData.sliderItems["characterVolume"] = 50;
            settingsData.sliderItems["sfxVolume"] = 50;
            
            SetItems();
        }
        
        public int GetScreenResolution()
        {
            var display = UnityEngine.Device.Screen.mainWindowDisplayInfo;
            if(_supportedResolutions.Exists(r => r.width == display.width && r.height == display.height))
                return _supportedResolutions.FindIndex(r => r.width == display.width && r.height == display.height);
            else if(_supportedResolutions.Exists(r => r.width == display.width))
                return _supportedResolutions.FindIndex(r => r.width == display.width);
            else if(_supportedResolutions.Exists(r => r.height == display.height))
                return _supportedResolutions.FindIndex(r => r.height == display.height);
            else return _supportedResolutions.FindIndex(r => r is { width: 1920, height: 1080 });
        }

        [Serializable]
        public struct SettingsTab
        {
            public Button button;
            public TextMeshProUGUI text;
            
            public void SelectTab()
            {
                text.color = Color.black;
                text.fontSize = 45;
                button.interactable = false;
            }
            
            public void DeselectTab()
            {
                text.color = Color.white;
                text.fontSize = 40;
                button.interactable = true;
            }
        }
    }
}