using System;
using SolarBuff.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SolarBuff.Interface.Menu
{
    public class SaveListItem : MonoBehaviour
    {
        [Header("REFERENCES")]
        public TMP_Text labelSaveName;
        public TMP_Text labelSaveLastModified;
        public RawImage iconSaveThumbnail;
        private SaveProfile _profile;
        
        public void Show(SaveProfile profile)
        {
            _profile = profile;
            labelSaveName.text = profile.name;
            labelSaveLastModified.text = new DateTime(profile.modifiedTime).ToString("yyyy-MM-dd HH:mm");
            iconSaveThumbnail.texture = profile.thumbnail;
        }

        public void Load()
        {
            var sm = SaveManager.Instance;
            sm.currentProfile = _profile;
            SaveList.StartServer();
        }
    }
}