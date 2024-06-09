using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Solis.UI
{
    public class MenuManager : WindowManager
    {
        public TextMeshProUGUI versionText;

        private void Awake()
        {
            versionText.text = $"V: {Application.version}";
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (versionText != null) versionText.text = $"V: {Application.version}";
            base.OnValidate();
        }
#endif
    }
}