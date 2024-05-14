using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solis.UI
{
    public class MenuManager : WindowManager
    {
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}