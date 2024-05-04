
using Solis.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Interface.Widgets
{
    /// <summary>
    /// Used to automatically set up a button widget.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class WidgetButton : Widget
    {
        #region Inspector Fields
        public string clickSound = "click";
        #endregion
        
        #region Private Fields
        private Button _button;
        #endregion

        #region Unity Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _button = GetComponent<Button>();
            _button.onClick.AddListener(_OnClick);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            _button.onClick.RemoveListener(_OnClick);
            _button = null;
        }
        #endregion

        #region Private Methods
        private void _OnClick()
        {
            AudioSystem.PlayVfxStatic(clickSound);
        }
        #endregion
    }
}