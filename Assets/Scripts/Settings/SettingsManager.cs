using System;
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
        private static bool _isInitialized;
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

        #region Unity Callbacks
        
        private void Awake()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
            }

            Load();
            onChangeWindow += OnTabSelected;
            
            intItems["resolution"].SetItems(Screen.resolutions.Select(r => $"{r.width}x{r.height}").ToList());
            intItems["graphics"].SetItems(QualitySettings.names.ToList());
            foreach (var i in intItems)
                i.Value.onChangeItem.AddListener(index => { settingsData.intItems[i.Key] = index; ApplySettings(); });
            foreach (var f in floatItems)
                f.Value.onValueChanged.AddListener(value => { settingsData.floatItems[f.Key] = value; ApplySettings(); });
            foreach (var b in boolItems)
                b.Value.onValueChanged.AddListener(value => { settingsData.boolItems[b.Key] = value; ApplySettings(); });
        }

        private void OnEnable()
        {
            OnSettingsChanged += ApplySettings;
        }

        private void OnDestroy()
        {
            OnSettingsChanged -= ApplySettings;
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

                foreach (var b in settingsData.boolItems)
                    boolItems.Add(b.Key, null);
                foreach (var i in settingsData.intItems)
                    intItems.Add(i.Key, null);
                foreach (var f in settingsData.floatItems)
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
                settingsData.boolItems.Clear();
                settingsData.intItems.Clear();
                settingsData.floatItems.Clear();
                
                foreach (var item in boolItems)
                    settingsData.boolItems.Add(item.Key, false);
                foreach (var item in intItems)
                    settingsData.intItems.Add(item.Key, 0);
                foreach (var item in floatItems)
                    settingsData.floatItems.Add(item.Key, 0);
                
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
            QualitySettings.vSyncCount = settingsData.boolItems["vsync"] ? 1 : 0;
            QualitySettings.SetQualityLevel(settingsData.intItems["graphics"]);
            Screen.SetResolution(Screen.resolutions[settingsData.intItems["resolution"]].width, Screen.resolutions[settingsData.intItems["resolution"]].height, Screen.fullScreenMode);
            Screen.fullScreenMode = settingsData.boolItems["fullscreen"] ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        private void Load()
        {
            if (File.Exists(Path + "/settings.json"))
            {
                settingsData.LoadFromJson(File.ReadAllText(Path + "/settings.json"));
                SetItems();
            }else this.Save();
        }

        public void Save()
        {
            if (!File.Exists(Path + "/settings.json"))
            {
                File.Create(Path + "/settings.json").Dispose();
                settingsData.intItems["resolution"] = Screen.resolutions.ToList().FindIndex(r => r.width == Screen.width && r.height == Screen.height);
                settingsData.intItems["graphics"] = QualitySettings.GetQualityLevel();
                SetItems();
            }
            File.WriteAllText(Path + "/settings.json", JsonUtility.ToJson(settingsData, true));
            OnSettingsChanged?.Invoke();
        }

        private void SetItems()
        {
            foreach (var g in settingsData.boolItems)
                boolItems[g.Key].isOn = g.Value;
            foreach (var g in settingsData.intItems)
                intItems[g.Key].currentIndex = g.Value;
            foreach (var g in settingsData.floatItems)
                floatItems[g.Key].value = g.Value;
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