using System;
using Interface;
using Interface.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Solis.Interface.Menu
{
    /// <summary>
    /// Represents a single server in the server list.
    /// </summary>
    public class ServerListEntry : WidgetListEntry
    {
        #region Inspector Fields
        public TMP_Text textName;
        public TMP_Text textPlayerCount;
        public TMP_Text textPlatform;
        public Image iconHasPassword;
        #endregion
    }
}